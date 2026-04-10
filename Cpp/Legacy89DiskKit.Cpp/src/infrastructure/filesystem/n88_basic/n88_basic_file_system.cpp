#include "legacy89diskkit/cpp/infrastructure/filesystem/n88_basic/n88_basic_file_system.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_attribute_update_transaction.hpp"
#include "legacy89diskkit/cpp/n88_basic_configuration.hpp"
#include "legacy89diskkit/cpp/n88_basic_delete_transaction.hpp"
#include "legacy89diskkit/cpp/n88_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_format_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_read_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_rename_transaction.hpp"
#include "legacy89diskkit/cpp/n88_basic_shell.hpp"
#include "legacy89diskkit/cpp/n88_basic_write_transaction.hpp"

#include <algorithm>
#include <array>
#include <string>

namespace legacy89diskkit::cpp
{
namespace
{
std::string NormalizeName(const std::string_view name)
{
    std::string normalized(name);
    std::transform(
        normalized.begin(),
        normalized.end(),
        normalized.begin(),
        [](const unsigned char ch)
        {
            return static_cast<char>(std::toupper(ch));
        });
    return normalized;
}

std::string BuildFullName(const N88BasicFileEntry& entry)
{
    return HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension);
}

std::array<std::uint8_t, 16> EncodeDirectoryEntry(const N88BasicFileEntry& entry)
{
    auto encoded = N88BasicDirParser::Write(entry);
    for (int index = 0; index < 6; ++index)
    {
        if (encoded[index] == 0x00)
        {
            encoded[index] = static_cast<std::uint8_t>(' ');
        }
    }

    for (int index = 6; index < 9; ++index)
    {
        if (encoded[index] == 0x00)
        {
            encoded[index] = static_cast<std::uint8_t>(' ');
        }
    }

    return encoded;
}
}

N88BasicFileSystem N88BasicFileSystem::Open(RawDiskContainer& container)
{
    return N88BasicFileSystem(&container, nullptr, container.DiskTypeValue(), container.IsReadOnly());
}

N88BasicFileSystem N88BasicFileSystem::Open(D88DiskContainer& container)
{
    return N88BasicFileSystem(nullptr, &container, container.DiskTypeValue(), container.IsReadOnly());
}

N88BasicFileSystem::N88BasicFileSystem(
    RawDiskContainer* raw_container,
    D88DiskContainer* d88_container,
    const DiskType disk_type,
    const bool read_only)
    : raw_container_(raw_container),
      d88_container_(d88_container),
      config_(N88BasicConfigurationProvider::GetDefault(disk_type)),
      disk_type_(disk_type),
      read_only_(read_only)
{
}

const N88BasicConfiguration& N88BasicFileSystem::GetConfiguration() const
{
    return config_;
}

DiskType N88BasicFileSystem::DiskTypeValue() const
{
    return disk_type_;
}

bool N88BasicFileSystem::IsReadOnly() const
{
    return read_only_;
}

N88BasicFileSystemInfo N88BasicFileSystem::GetFileSystemInfo() const
{
    return N88BasicShell::GetFileSystemInfo(ReadFat(), config_);
}

std::vector<N88BasicFileEntry> N88BasicFileSystem::GetFiles() const
{
    return N88BasicShell::ListFiles(ReadDirectorySectors(), ReadFat(), config_);
}

bool N88BasicFileSystem::FileExists(const std::string_view file_name) const
{
    const auto file_name_text = std::string(file_name);
    return N88BasicShell::FileExists(ReadDirectorySectors(), ReadFat(), config_, file_name_text.c_str());
}

Result<std::vector<std::uint8_t>> N88BasicFileSystem::ReadFile(const std::string_view file_name) const
{
    const auto target = NormalizeName(file_name);
    const auto files = GetFiles();
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
    const auto chain = N88BasicFatRules::GetClusterChain(fat_data, config_, it->start_cluster);
    std::vector<std::uint8_t> data;
    for (const auto cluster : chain)
    {
        const auto cluster_data = ReadCluster(cluster);
        if (!cluster_data.ok())
        {
            return cluster_data;
        }

        data.insert(data.end(), cluster_data.value().begin(), cluster_data.value().end());
    }

    auto file = *it;
    file.size = N88BasicReadRules::ResolveSizeFromFat(chain, fat_data, config_);
    return Result<std::vector<std::uint8_t>>::Success(N88BasicReadRules::ResolveReadPayload(data, file));
}

Status N88BasicFileSystem::WriteFile(
    const std::string_view file_name,
    const std::vector<std::uint8_t>& data,
    const N88BasicFileAttributes& attributes)
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
    const auto file_name_text = std::string(file_name);
    const auto plan = N88BasicShell::PlanWrite(file_name_text.c_str(), data, attributes, config_, fat_data);
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create write plan."};
    }

    const auto cluster_size = static_cast<std::size_t>(config_.cluster_size);
    for (std::size_t index = 0; index < plan->allocated_clusters.size(); ++index)
    {
        std::vector<std::uint8_t> cluster_buffer(cluster_size, 0x00);
        const auto offset = index * cluster_size;
        const auto count = std::min(cluster_size, plan->payload.size() - std::min(plan->payload.size(), offset));
        if (count > 0)
        {
            std::copy_n(plan->payload.begin() + static_cast<std::ptrdiff_t>(offset), count, cluster_buffer.begin());
        }

        const auto status = WriteCluster(plan->allocated_clusters[index], cluster_buffer);
        if (!status.ok())
        {
            return status;
        }
    }

    for (std::size_t index = 0; index < plan->allocated_clusters.size(); ++index)
    {
        const auto next = index + 1 == plan->allocated_clusters.size()
            ? plan->terminal_flag
            : plan->allocated_clusters[index + 1];
        N88BasicFatRules::SetEntry(fat_data, plan->allocated_clusters[index], next);
    }

    const auto fat_status = WriteFat(fat_data);
    if (!fat_status.ok())
    {
        return fat_status;
    }

    auto directory_sectors = ReadDirectorySectors();
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        for (auto offset = 0; offset < config_.sector_size; offset += 16)
        {
            const auto marker = directory_sectors[sector_index][offset];
            if (marker != 0x00 && marker != 0xff)
            {
                continue;
            }

            const auto encoded = EncodeDirectoryEntry(plan->file_entry);
            std::copy(encoded.begin(), encoded.end(), directory_sectors[sector_index].begin() + offset);
            if (marker == 0xff && offset + 16 < config_.sector_size)
            {
                directory_sectors[sector_index][offset + 16] = 0xff;
            }

            return WriteDirectorySector(static_cast<int>(sector_index), directory_sectors[sector_index]);
        }
    }

    return {StatusCode::OutOfRange, "Directory is full."};
}

Status N88BasicFileSystem::DeleteFile(const std::string_view file_name)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    const auto directory_sectors = ReadDirectorySectors();
    const auto file_name_text = std::string(file_name);
    const auto file = N88BasicShell::FindFile(directory_sectors, ReadFat(), config_, file_name_text.c_str());
    if (!file.has_value())
    {
        return {StatusCode::InvalidArgument, "File not found."};
    }

    auto fat_data = ReadFat();
    const auto chain = N88BasicFatRules::GetClusterChain(fat_data, config_, file->start_cluster);
    const auto plan = N88BasicShell::PlanDelete(fat_data, chain, directory_sectors, config_, file_name_text.c_str());
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
    sector[plan->entry_offset] = 0x00;
    return WriteDirectorySector(plan->sector_index, sector);
}

Status N88BasicFileSystem::RenameFile(const std::string_view old_name, const std::string_view new_name)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    auto directory_sectors = ReadDirectorySectors();
    const auto old_name_text = std::string(old_name);
    const auto new_name_text = std::string(new_name);
    const auto plan = N88BasicShell::PlanRename(directory_sectors, config_, old_name_text.c_str(), new_name_text.c_str());
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create rename plan."};
    }

    const auto encoded = EncodeDirectoryEntry(plan->updated_entry);
    std::copy(encoded.begin(), encoded.end(), directory_sectors[plan->sector_index].begin() + plan->entry_offset);
    return WriteDirectorySector(plan->sector_index, directory_sectors[plan->sector_index]);
}

Status N88BasicFileSystem::UpdateAttributes(const std::string_view file_name, const N88BasicFileAttributes& attributes)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    auto directory_sectors = ReadDirectorySectors();
    const auto file_name_text = std::string(file_name);
    const auto plan = N88BasicShell::PlanAttributeUpdate(directory_sectors, config_, file_name_text.c_str(), attributes);
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create attribute update plan."};
    }

    const auto encoded = EncodeDirectoryEntry(plan->updated_entry);
    std::copy(encoded.begin(), encoded.end(), directory_sectors[plan->sector_index].begin() + plan->entry_offset);
    return WriteDirectorySector(plan->sector_index, directory_sectors[plan->sector_index]);
}

Status N88BasicFileSystem::Format()
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    const auto fat_status = WriteFat(N88BasicShell::CreateFatData(config_));
    if (!fat_status.ok())
    {
        return fat_status;
    }

    const auto directory = N88BasicShell::CreateDirectorySectors(config_);
    for (std::size_t index = 0; index < directory.size(); ++index)
    {
        const auto status = WriteDirectorySector(static_cast<int>(index), directory[index]);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}

Result<std::vector<std::uint8_t>> N88BasicFileSystem::ReadBootArea() const
{
    return ReadSector(0, 0, 1);
}

Status N88BasicFileSystem::WriteBootArea(const std::vector<std::uint8_t>& data)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    std::vector<std::uint8_t> sector_data(config_.sector_size, 0x00);
    const size_t copy_size = std::min<size_t>(data.size(), config_.sector_size);
    std::copy(data.begin(), data.begin() + copy_size, sector_data.begin());

    return WriteSector(0, 0, 1, sector_data);
}

Result<std::vector<std::uint8_t>> N88BasicFileSystem::ReadSector(const int cylinder, const int head, const int sector) const
{
    if (raw_container_ != nullptr)
    {
        return raw_container_->ReadSector(cylinder, head, sector);
    }

    return d88_container_->ReadSector(cylinder, head, sector);
}

Status N88BasicFileSystem::WriteSector(const int cylinder, const int head, const int sector, const std::vector<std::uint8_t>& data)
{
    if (raw_container_ != nullptr)
    {
        return raw_container_->WriteSector(cylinder, head, sector, data);
    }

    return d88_container_->WriteSector(cylinder, head, sector, data);
}

std::vector<std::uint8_t> N88BasicFileSystem::ReadFat() const
{
    std::vector<std::uint8_t> fat_data(config_.fat_sectors * config_.sector_size, 0x00);
    for (int sector_index = 0; sector_index < config_.fat_sectors; ++sector_index)
    {
        const auto sector = ReadSector(config_.system_track, config_.system_head, config_.fat_sector + sector_index);
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

Status N88BasicFileSystem::WriteFat(const std::vector<std::uint8_t>& fat_data)
{
    for (int sector_index = 0; sector_index < config_.fat_sectors; ++sector_index)
    {
        std::vector<std::uint8_t> sector(config_.sector_size, 0x00);
        std::copy(
            fat_data.begin() + (sector_index * config_.sector_size),
            fat_data.begin() + ((sector_index + 1) * config_.sector_size),
            sector.begin());
        const auto status = WriteSector(config_.system_track, config_.system_head, config_.fat_sector + sector_index, sector);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}

std::vector<std::vector<std::uint8_t>> N88BasicFileSystem::ReadDirectorySectors() const
{
    std::vector<std::vector<std::uint8_t>> sectors;
    sectors.reserve(config_.directory_sectors);
    for (int sector_index = 0; sector_index < config_.directory_sectors; ++sector_index)
    {
        const auto sector = ReadSector(config_.system_track, config_.system_head, config_.directory_sector + sector_index);
        sectors.push_back(sector.ok() ? sector.value() : std::vector<std::uint8_t>(config_.sector_size, 0x00));
    }
    return sectors;
}

Status N88BasicFileSystem::WriteDirectorySector(const int sector_index, const std::vector<std::uint8_t>& sector_data)
{
    return WriteSector(config_.system_track, config_.system_head, config_.directory_sector + sector_index, sector_data);
}

Result<std::vector<std::uint8_t>> N88BasicFileSystem::ReadCluster(const int cluster) const
{
    const auto sectors_per_cluster = config_.cluster_size / config_.sector_size;
    std::vector<std::uint8_t> cluster_data(config_.cluster_size, 0x00);
    const auto start_lba = cluster * sectors_per_cluster;
    for (int sector_index = 0; sector_index < sectors_per_cluster; ++sector_index)
    {
        const auto lba = start_lba + sector_index;
        const auto track = lba / config_.sectors_per_track;
        const auto sector = (lba % config_.sectors_per_track) + 1;
        int cylinder;
        int head;
        int physical_sector;
        GetPhysicalAddress(track, sector, cylinder, head, physical_sector);
        const auto result = ReadSector(cylinder, head, physical_sector);
        if (!result.ok())
        {
            return result;
        }

        std::copy(
            result.value().begin(),
            result.value().begin() + config_.sector_size,
            cluster_data.begin() + (sector_index * config_.sector_size));
    }

    return Result<std::vector<std::uint8_t>>::Success(cluster_data);
}

Status N88BasicFileSystem::WriteCluster(const int cluster, const std::vector<std::uint8_t>& cluster_data)
{
    const auto sectors_per_cluster = config_.cluster_size / config_.sector_size;
    const auto start_lba = cluster * sectors_per_cluster;
    for (int sector_index = 0; sector_index < sectors_per_cluster; ++sector_index)
    {
        const auto lba = start_lba + sector_index;
        const auto track = lba / config_.sectors_per_track;
        const auto sector = (lba % config_.sectors_per_track) + 1;
        int cylinder;
        int head;
        int physical_sector;
        GetPhysicalAddress(track, sector, cylinder, head, physical_sector);
        std::vector<std::uint8_t> sector_data(config_.sector_size, 0x00);
        std::copy(
            cluster_data.begin() + (sector_index * config_.sector_size),
            cluster_data.begin() + ((sector_index + 1) * config_.sector_size),
            sector_data.begin());
        const auto status = WriteSector(cylinder, head, physical_sector, sector_data);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}

void N88BasicFileSystem::GetPhysicalAddress(const int track, const int sector, int& cylinder, int& head, int& physical_sector)
{
    cylinder = track / 2;
    head = track % 2;
    physical_sector = sector;
}
}
