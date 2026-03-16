#include "legacy89diskkit/cpp/infrastructure/filesystem/hu_basic/hu_basic_file_system.hpp"

#include "legacy89diskkit/cpp/hu_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_attribute_update_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_cluster_write_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_configuration.hpp"
#include "legacy89diskkit/cpp/hu_basic_delete_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_listing.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_sector_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_file_entry_writer.hpp"
#include "legacy89diskkit/cpp/hu_basic_file_lookup.hpp"
#include "legacy89diskkit/cpp/hu_basic_format_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_read_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_record_address_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_rename_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_write_transaction.hpp"

#include <algorithm>
#include <array>
#include <cctype>

namespace legacy89diskkit::cpp
{
namespace
{
std::string NormalizeName(const std::string_view name)
{
    std::string normalized;
    normalized.reserve(name.size());
    for (const auto ch : name)
    {
        normalized.push_back(static_cast<char>(std::toupper(static_cast<unsigned char>(ch))));
    }
    return normalized;
}

std::string BuildFullName(const HuBasicFileEntry& entry)
{
    return entry.extension.empty() ? entry.file_name : entry.file_name + "." + entry.extension;
}
}

HuBasicFileSystem HuBasicFileSystem::Open(RawDiskContainer& container)
{
    return HuBasicFileSystem(&container, nullptr, container.DiskTypeValue(), container.IsReadOnly());
}

HuBasicFileSystem HuBasicFileSystem::Open(D88DiskContainer& container)
{
    return HuBasicFileSystem(nullptr, &container, container.DiskTypeValue(), container.IsReadOnly());
}

HuBasicFileSystem::HuBasicFileSystem(
    RawDiskContainer* raw_container,
    D88DiskContainer* d88_container,
    const DiskType disk_type,
    const bool read_only)
    : raw_container_(raw_container),
      d88_container_(d88_container),
      config_(HuBasicConfigurationProvider::GetDefault(disk_type)),
      disk_type_(disk_type),
      read_only_(read_only)
{
}

const HuBasicConfiguration& HuBasicFileSystem::GetConfiguration() const
{
    return config_;
}

DiskType HuBasicFileSystem::DiskTypeValue() const
{
    return disk_type_;
}

bool HuBasicFileSystem::IsReadOnly() const
{
    return read_only_;
}

HuBasicFileSystemInfo HuBasicFileSystem::GetFileSystemInfo() const
{
    return HuBasicFileSystemInfoRules::BuildInfo(ReadFat(), disk_type_, config_);
}

std::vector<HuBasicFileEntry> HuBasicFileSystem::GetFiles() const
{
    return HuBasicDirectoryListing::ListFiles(ReadDirectorySectors(), config_.sector_size);
}

bool HuBasicFileSystem::FileExists(const std::string_view file_name) const
{
    const auto file_name_text = std::string(file_name);
    return HuBasicFileLookup::Exists(ReadDirectorySectors(), config_.sector_size, file_name_text.c_str());
}

Result<std::vector<std::uint8_t>> HuBasicFileSystem::ReadFile(const std::string_view file_name) const
{
    const auto files = GetFiles();
    const auto target = NormalizeName(file_name);
    const auto it = std::find_if(
        files.begin(),
        files.end(),
        [&](const auto& entry)
        {
            return NormalizeName(BuildFullName(entry)) == target;
        });
    if (it == files.end())
    {
        return Result<std::vector<std::uint8_t>>::Failure(StatusCode::InvalidArgument, "File not found.");
    }

    const auto fat_data = ReadFat();
    const auto chain_result = HuBasicFatRules::GetClusterChain(fat_data, config_, it->start_cluster);
    std::vector<std::uint8_t> data;
    for (const auto cluster : chain_result.chain)
    {
        const auto cluster_data = ReadCluster(cluster);
        if (!cluster_data.ok())
        {
            return cluster_data;
        }

        data.insert(data.end(), cluster_data.value().begin(), cluster_data.value().end());
    }

    return Result<std::vector<std::uint8_t>>::Success(
        HuBasicReadRules::ResolveReadPayload(data, *it, disk_type_, config_, static_cast<int>(chain_result.chain.size()), chain_result.terminal_flag));
}

Status HuBasicFileSystem::WriteFile(
    const std::string_view file_name,
    const std::vector<std::uint8_t>& data,
    const HuBasicFileAttributes& attributes,
    const std::uint16_t load_address,
    const std::uint16_t execution_address)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    if (FileExists(file_name))
    {
        return {StatusCode::InvalidArgument, "File already exists."};
    }

    auto fat_data = ReadFat();
    const auto plan = HuBasicWriteTransaction::CreatePlan(
        std::string(file_name),
        data,
        attributes,
        disk_type_,
        config_,
        fat_data,
        load_address,
        execution_address);
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create write plan."};
    }

    const auto cluster_buffers = HuBasicClusterWriteRules::SplitIntoClusterBuffers(plan->payload, plan->allocated_clusters, config_);
    for (std::size_t i = 0; i < plan->allocated_clusters.size(); ++i)
    {
        const auto write_status = WriteCluster(plan->allocated_clusters[i], cluster_buffers[i]);
        if (!write_status.ok())
        {
            return write_status;
        }
    }

    HuBasicFatRules::ApplyChain(fat_data, plan->allocated_clusters, plan->terminal_flag);
    const auto fat_status = WriteFat(fat_data);
    if (!fat_status.ok())
    {
        return fat_status;
    }

    auto directory_sectors = ReadDirectorySectors();
    bool added = false;
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        auto writable_offset = HuBasicDirectorySectorRules::FindWritableSlotOffset(directory_sectors[sector_index], config_.sector_size);
        if (!writable_offset.has_value())
        {
            continue;
        }

        const auto encoded = HuBasicDirectoryEntryCodec::Write(plan->directory_entry);
        std::copy(encoded.begin(), encoded.end(), directory_sectors[sector_index].begin() + *writable_offset);
        if (*writable_offset + 32 < config_.sector_size && directory_sectors[sector_index][*writable_offset + 32] == 0x00)
        {
            directory_sectors[sector_index][*writable_offset + 32] = 0xff;
        }

        const auto write_status = WriteDirectorySector(static_cast<int>(sector_index), directory_sectors[sector_index]);
        if (!write_status.ok())
        {
            return write_status;
        }

        added = true;
        break;
    }

    return added ? Status::OkStatus() : Status{StatusCode::OutOfRange, "Directory is full."};
}

Status HuBasicFileSystem::DeleteFile(const std::string_view file_name)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    const auto normalized_name = std::string(file_name);
    const auto directory_sectors = ReadDirectorySectors();
    const auto file = HuBasicFileLookup::FindByName(directory_sectors, config_.sector_size, normalized_name.c_str());
    if (!file.has_value())
    {
        return {StatusCode::InvalidArgument, "File not found."};
    }

    auto fat_data = ReadFat();
    const auto chain = HuBasicFatRules::GetClusterChain(fat_data, config_, file->start_cluster);
    const auto plan = HuBasicDeleteTransaction::CreatePlan(
        fat_data,
        chain.chain,
        directory_sectors,
        config_.sector_size,
        normalized_name.c_str());
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create delete plan."};
    }

    const auto fat_status = WriteFat(plan->fat_data);
    if (!fat_status.ok())
    {
        return fat_status;
    }

    auto sector = directory_sectors[plan->sector_index];
    HuBasicDirectorySectorRules::MarkEntryDeleted(sector, plan->entry_offset);
    return WriteDirectorySector(plan->sector_index, sector);
}

Status HuBasicFileSystem::RenameFile(const std::string_view old_name, const std::string_view new_name)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    auto directory_sectors = ReadDirectorySectors();
    const auto old_name_text = std::string(old_name);
    const auto new_name_text = std::string(new_name);
    const auto plan = HuBasicRenameTransaction::CreatePlan(
        directory_sectors,
        config_.sector_size,
        old_name_text.c_str(),
        new_name_text.c_str());
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create rename plan."};
    }

    const auto encoded = HuBasicDirectoryEntryCodec::Write(HuBasicFileEntryWriter::ToDirectoryEntry(plan->updated_entry));
    std::copy(encoded.begin(), encoded.end(), directory_sectors[plan->sector_index].begin() + plan->entry_offset);
    return WriteDirectorySector(plan->sector_index, directory_sectors[plan->sector_index]);
}

Status HuBasicFileSystem::UpdateAttributes(const std::string_view file_name, const HuBasicFileAttributes& attributes)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    auto directory_sectors = ReadDirectorySectors();
    const auto file_name_text = std::string(file_name);
    const auto plan = HuBasicAttributeUpdateTransaction::CreatePlan(
        directory_sectors,
        config_.sector_size,
        file_name_text.c_str(),
        attributes);
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create attribute update plan."};
    }

    const auto encoded = HuBasicDirectoryEntryCodec::Write(HuBasicFileEntryWriter::ToDirectoryEntry(plan->updated_entry));
    std::copy(encoded.begin(), encoded.end(), directory_sectors[plan->sector_index].begin() + plan->entry_offset);
    return WriteDirectorySector(plan->sector_index, directory_sectors[plan->sector_index]);
}

Status HuBasicFileSystem::Format()
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    const auto fat_status = WriteFat(HuBasicFormatRules::CreateFatData(config_));
    if (!fat_status.ok())
    {
        return fat_status;
    }

    const auto directory = HuBasicFormatRules::CreateDirectorySectors(config_);
    for (std::size_t i = 0; i < directory.size(); ++i)
    {
        const auto write_status = WriteDirectorySector(static_cast<int>(i), directory[i]);
        if (!write_status.ok())
        {
            return write_status;
        }
    }

    return Status::OkStatus();
}

Result<std::vector<std::uint8_t>> HuBasicFileSystem::ReadSector(const int cylinder, const int head, const int sector) const
{
    if (raw_container_ != nullptr)
    {
        return raw_container_->ReadSector(cylinder, head, sector);
    }

    return d88_container_->ReadSector(cylinder, head, sector);
}

Status HuBasicFileSystem::WriteSector(const int cylinder, const int head, const int sector, const std::vector<std::uint8_t>& data)
{
    if (raw_container_ != nullptr)
    {
        return raw_container_->WriteSector(cylinder, head, sector, data);
    }

    return d88_container_->WriteSector(cylinder, head, sector, data);
}

std::vector<std::uint8_t> HuBasicFileSystem::ReadFat() const
{
    std::vector<std::uint8_t> fat_data(config_.fat_sectors * config_.sector_size, 0x00);
    const auto start_record = (config_.fat_track * config_.sectors_per_track) + (config_.fat_sector - 1);
    for (int sector_index = 0; sector_index < config_.fat_sectors; ++sector_index)
    {
        const auto address = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(start_record + sector_index, config_);
        const auto sector = ReadSector(address.cylinder, address.head, address.sector);
        if (!sector.ok())
        {
            return {};
        }

        std::copy(
            sector.value().begin(),
            sector.value().begin() + config_.sector_size,
            fat_data.begin() + (sector_index * config_.sector_size));
    }

    return fat_data;
}

Status HuBasicFileSystem::WriteFat(const std::vector<std::uint8_t>& fat_data)
{
    const auto start_record = (config_.fat_track * config_.sectors_per_track) + (config_.fat_sector - 1);
    for (int sector_index = 0; sector_index < config_.fat_sectors; ++sector_index)
    {
        const auto address = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(start_record + sector_index, config_);
        std::vector<std::uint8_t> sector(config_.sector_size, 0x00);
        std::copy(
            fat_data.begin() + (sector_index * config_.sector_size),
            fat_data.begin() + ((sector_index + 1) * config_.sector_size),
            sector.begin());
        const auto status = WriteSector(address.cylinder, address.head, address.sector, sector);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}

std::vector<std::vector<std::uint8_t>> HuBasicFileSystem::ReadDirectorySectors() const
{
    std::vector<std::vector<std::uint8_t>> sectors;
    sectors.reserve(config_.directory_sectors);
    const auto start_record = (config_.directory_track * config_.sectors_per_track) + (config_.directory_sector - 1);
    for (int sector_index = 0; sector_index < config_.directory_sectors; ++sector_index)
    {
        const auto address = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(start_record + sector_index, config_);
        const auto sector = ReadSector(address.cylinder, address.head, address.sector);
        sectors.push_back(sector.ok() ? sector.value() : std::vector<std::uint8_t>(config_.sector_size, 0x00));
    }
    return sectors;
}

Status HuBasicFileSystem::WriteDirectorySector(const int sector_index, const std::vector<std::uint8_t>& sector_data)
{
    const auto start_record = (config_.directory_track * config_.sectors_per_track) + (config_.directory_sector - 1);
    const auto address = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(start_record + sector_index, config_);
    return WriteSector(address.cylinder, address.head, address.sector, sector_data);
}

Result<std::vector<std::uint8_t>> HuBasicFileSystem::ReadCluster(const int cluster) const
{
    const auto sectors_per_cluster = config_.cluster_size / config_.sector_size;
    std::vector<std::uint8_t> cluster_data(config_.cluster_size, 0x00);
    const auto start_record = cluster * sectors_per_cluster;
    for (int sector_index = 0; sector_index < sectors_per_cluster; ++sector_index)
    {
        const auto address = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(start_record + sector_index, config_);
        const auto sector = ReadSector(address.cylinder, address.head, address.sector);
        if (!sector.ok())
        {
            return sector;
        }

        std::copy(
            sector.value().begin(),
            sector.value().begin() + config_.sector_size,
            cluster_data.begin() + (sector_index * config_.sector_size));
    }

    return Result<std::vector<std::uint8_t>>::Success(cluster_data);
}

Status HuBasicFileSystem::WriteCluster(const int cluster, const std::vector<std::uint8_t>& cluster_data)
{
    const auto sectors_per_cluster = config_.cluster_size / config_.sector_size;
    const auto start_record = cluster * sectors_per_cluster;
    for (int sector_index = 0; sector_index < sectors_per_cluster; ++sector_index)
    {
        const auto address = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(start_record + sector_index, config_);
        std::vector<std::uint8_t> sector(config_.sector_size, 0x00);
        std::copy(
            cluster_data.begin() + (sector_index * config_.sector_size),
            cluster_data.begin() + ((sector_index + 1) * config_.sector_size),
            sector.begin());
        const auto status = WriteSector(address.cylinder, address.head, address.sector, sector);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}
}
