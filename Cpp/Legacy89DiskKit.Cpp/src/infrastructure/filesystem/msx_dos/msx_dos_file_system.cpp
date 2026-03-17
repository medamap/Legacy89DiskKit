#include "legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/msx_dos_file_system.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_attribute_update_transaction.hpp"
#include "legacy89diskkit/cpp/msx_dos_boot_sector_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_cluster_write_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_delete_transaction.hpp"
#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_file_entry_writer.hpp"
#include "legacy89diskkit/cpp/msx_dos_read_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_rename_transaction.hpp"
#include "legacy89diskkit/cpp/msx_dos_shell.hpp"

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

std::string BuildFullName(const MsxDosFileEntry& entry)
{
    return HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension);
}
}

Result<MsxDosFileSystem> MsxDosFileSystem::Open(RawDiskContainer& container)
{
    const auto boot_sector = container.ReadSector(0, 0, 1);
    if (!boot_sector.ok())
    {
        return Result<MsxDosFileSystem>::Failure(boot_sector.status().code, boot_sector.status().message);
    }

    const auto parsed = MsxDosBootSectorParser::Parse(boot_sector.value());
    if (!parsed.has_value())
    {
        return Result<MsxDosFileSystem>::Failure(StatusCode::UnsupportedFormat, "Invalid MSX-DOS boot sector.");
    }

    return Result<MsxDosFileSystem>::Success(
        MsxDosFileSystem(&container, nullptr, parsed->configuration, container.DiskTypeValue(), container.IsReadOnly()));
}

Result<MsxDosFileSystem> MsxDosFileSystem::Open(D88DiskContainer& container)
{
    const auto boot_sector = container.ReadSector(0, 0, 1);
    if (!boot_sector.ok())
    {
        return Result<MsxDosFileSystem>::Failure(boot_sector.status().code, boot_sector.status().message);
    }

    const auto parsed = MsxDosBootSectorParser::Parse(boot_sector.value());
    if (!parsed.has_value())
    {
        return Result<MsxDosFileSystem>::Failure(StatusCode::UnsupportedFormat, "Invalid MSX-DOS boot sector.");
    }

    return Result<MsxDosFileSystem>::Success(
        MsxDosFileSystem(nullptr, &container, parsed->configuration, container.DiskTypeValue(), container.IsReadOnly()));
}

MsxDosFileSystem MsxDosFileSystem::OpenExplicit(RawDiskContainer& container)
{
    return MsxDosFileSystem(
        &container,
        nullptr,
        MsxDosConfigurationProvider::GetDefault(container.DiskTypeValue()),
        container.DiskTypeValue(),
        container.IsReadOnly());
}

MsxDosFileSystem MsxDosFileSystem::OpenExplicit(D88DiskContainer& container)
{
    return MsxDosFileSystem(
        nullptr,
        &container,
        MsxDosConfigurationProvider::GetDefault(container.DiskTypeValue()),
        container.DiskTypeValue(),
        container.IsReadOnly());
}

MsxDosFileSystem::MsxDosFileSystem(
    RawDiskContainer* raw_container,
    D88DiskContainer* d88_container,
    const MsxDosConfiguration config,
    const DiskType disk_type,
    const bool read_only)
    : raw_container_(raw_container),
      d88_container_(d88_container),
      config_(config),
      disk_type_(disk_type),
      read_only_(read_only)
{
}

const MsxDosConfiguration& MsxDosFileSystem::GetConfiguration() const
{
    return config_;
}

DiskType MsxDosFileSystem::DiskTypeValue() const
{
    return disk_type_;
}

bool MsxDosFileSystem::IsReadOnly() const
{
    return read_only_;
}

MsxDosFileSystemInfo MsxDosFileSystem::GetFileSystemInfo() const
{
    return MsxDosShell::GetFileSystemInfo(ReadFat(), config_);
}

std::vector<MsxDosFileEntry> MsxDosFileSystem::GetFiles() const
{
    return MsxDosShell::ListFiles(ReadRootDirectorySectors(), ReadFat(), config_);
}

bool MsxDosFileSystem::FileExists(const std::string_view file_name) const
{
    const auto file_name_text = std::string(file_name);
    return MsxDosShell::FileExists(ReadRootDirectorySectors(), ReadFat(), config_, file_name_text.c_str());
}

Result<std::vector<std::uint8_t>> MsxDosFileSystem::ReadFile(const std::string_view file_name) const
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
    const auto chain = MsxDosFatRules::GetClusterChain(fat_data, config_, it->start_cluster);
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

    return Result<std::vector<std::uint8_t>>::Success(MsxDosReadRules::ResolveReadPayload(data, *it));
}

Status MsxDosFileSystem::WriteFile(
    const std::string_view file_name,
    const std::vector<std::uint8_t>& data,
    const MsxDosFileAttributes& attributes)
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
    const auto plan = MsxDosShell::PlanWrite(file_name_text.c_str(), data, attributes, config_, fat_data);
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create write plan."};
    }

    const auto cluster_buffers = MsxDosClusterWriteRules::SplitIntoClusterBuffers(plan->payload, plan->allocated_clusters, config_);
    for (std::size_t index = 0; index < plan->allocated_clusters.size(); ++index)
    {
        const auto status = WriteCluster(plan->allocated_clusters[index], cluster_buffers[index]);
        if (!status.ok())
        {
            return status;
        }
    }

    for (std::size_t index = 0; index < plan->allocated_clusters.size(); ++index)
    {
        const auto next = index + 1 == plan->allocated_clusters.size()
            ? static_cast<std::uint16_t>(0xfff)
            : static_cast<std::uint16_t>(plan->allocated_clusters[index + 1]);
        MsxDosFatRules::SetEntry(fat_data, plan->allocated_clusters[index], next);
    }

    const auto fat_status = WriteFat(fat_data);
    if (!fat_status.ok())
    {
        return fat_status;
    }

    auto directory_sectors = ReadRootDirectorySectors();
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        for (auto offset = 0; offset < config_.sector_size; offset += 32)
        {
            const auto marker = directory_sectors[sector_index][offset];
            if (marker != 0x00 && marker != 0xe5)
            {
                continue;
            }

            const auto encoded = EncodeDirectoryEntry(plan->file_entry);
            std::copy(encoded.begin(), encoded.end(), directory_sectors[sector_index].begin() + offset);
            return WriteRootDirectorySector(static_cast<int>(sector_index), directory_sectors[sector_index]);
        }
    }

    return {StatusCode::OutOfRange, "Root directory is full."};
}

Status MsxDosFileSystem::DeleteFile(const std::string_view file_name)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    const auto directory_sectors = ReadRootDirectorySectors();
    const auto fat_data = ReadFat();
    const auto file_name_text = std::string(file_name);
    const auto file = MsxDosShell::FindFile(directory_sectors, fat_data, config_, file_name_text.c_str());
    if (!file.has_value())
    {
        return {StatusCode::InvalidArgument, "File not found."};
    }

    const auto chain = MsxDosFatRules::GetClusterChain(fat_data, config_, file->start_cluster);
    const auto plan = MsxDosShell::PlanDelete(fat_data, chain, directory_sectors, config_, file_name_text.c_str());
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
    sector[plan->entry_offset] = 0xe5;
    return WriteRootDirectorySector(plan->sector_index, sector);
}

Status MsxDosFileSystem::RenameFile(const std::string_view old_name, const std::string_view new_name)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    auto directory_sectors = ReadRootDirectorySectors();
    const auto old_name_text = std::string(old_name);
    const auto new_name_text = std::string(new_name);
    const auto plan = MsxDosShell::PlanRename(directory_sectors, config_, old_name_text.c_str(), new_name_text.c_str());
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create rename plan."};
    }

    const auto encoded = EncodeDirectoryEntry(plan->updated_entry);
    std::copy(encoded.begin(), encoded.end(), directory_sectors[plan->sector_index].begin() + plan->entry_offset);
    return WriteRootDirectorySector(plan->sector_index, directory_sectors[plan->sector_index]);
}

Status MsxDosFileSystem::UpdateAttributes(const std::string_view file_name, const MsxDosFileAttributes& attributes)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    auto directory_sectors = ReadRootDirectorySectors();
    const auto file_name_text = std::string(file_name);
    const auto plan = MsxDosShell::PlanAttributeUpdate(directory_sectors, config_, file_name_text.c_str(), attributes);
    if (!plan.has_value())
    {
        return {StatusCode::InvalidArgument, "Unable to create attribute update plan."};
    }

    const auto encoded = EncodeDirectoryEntry(plan->updated_entry);
    std::copy(encoded.begin(), encoded.end(), directory_sectors[plan->sector_index].begin() + plan->entry_offset);
    return WriteRootDirectorySector(plan->sector_index, directory_sectors[plan->sector_index]);
}

Status MsxDosFileSystem::Format()
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    const auto fat_data = MsxDosShell::CreateFatData(config_);
    for (int fat_index = 0; fat_index < config_.number_of_fats; ++fat_index)
    {
        for (int sector_index = 0; sector_index < config_.sectors_per_fat; ++sector_index)
        {
            std::vector<std::uint8_t> sector(config_.sector_size, 0x00);
            std::copy(
                fat_data.begin() + (sector_index * config_.sector_size),
                fat_data.begin() + ((sector_index + 1) * config_.sector_size),
                sector.begin());
            int cylinder;
            int head;
            int physical_sector;
            LbaToPhysical(config_, config_.GetFatStartSector(fat_index) + sector_index, cylinder, head, physical_sector);
            const auto status = WriteSector(cylinder, head, physical_sector, sector);
            if (!status.ok())
            {
                return status;
            }
        }
    }

    const auto directory = MsxDosShell::CreateRootDirectorySectors(config_);
    for (std::size_t index = 0; index < directory.size(); ++index)
    {
        const auto status = WriteRootDirectorySector(static_cast<int>(index), directory[index]);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}

Result<std::vector<std::uint8_t>> MsxDosFileSystem::ReadBootArea() const
{
    return ReadSector(0, 0, 1);
}

Status MsxDosFileSystem::WriteBootArea(const std::vector<std::uint8_t>& sector_data)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Filesystem is read-only."};
    }

    return WriteSector(0, 0, 1, sector_data);
}

Result<std::vector<std::uint8_t>> MsxDosFileSystem::ReadSector(const int cylinder, const int head, const int sector) const
{
    if (raw_container_ != nullptr)
    {
        return raw_container_->ReadSector(cylinder, head, sector);
    }

    return d88_container_->ReadSector(cylinder, head, sector);
}

Status MsxDosFileSystem::WriteSector(const int cylinder, const int head, const int sector, const std::vector<std::uint8_t>& data)
{
    if (raw_container_ != nullptr)
    {
        return raw_container_->WriteSector(cylinder, head, sector, data);
    }

    return d88_container_->WriteSector(cylinder, head, sector, data);
}

std::vector<std::uint8_t> MsxDosFileSystem::ReadFat() const
{
    std::vector<std::uint8_t> fat_data(config_.sectors_per_fat * config_.sector_size, 0x00);
    for (int sector_index = 0; sector_index < config_.sectors_per_fat; ++sector_index)
    {
        int cylinder;
        int head;
        int physical_sector;
        LbaToPhysical(config_, config_.GetFatStartSector(0) + sector_index, cylinder, head, physical_sector);
        const auto sector = ReadSector(cylinder, head, physical_sector);
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

Status MsxDosFileSystem::WriteFat(const std::vector<std::uint8_t>& fat_data)
{
    for (int fat_index = 0; fat_index < config_.number_of_fats; ++fat_index)
    {
        for (int sector_index = 0; sector_index < config_.sectors_per_fat; ++sector_index)
        {
            int cylinder;
            int head;
            int physical_sector;
            LbaToPhysical(config_, config_.GetFatStartSector(fat_index) + sector_index, cylinder, head, physical_sector);
            std::vector<std::uint8_t> sector(config_.sector_size, 0x00);
            std::copy(
                fat_data.begin() + (sector_index * config_.sector_size),
                fat_data.begin() + ((sector_index + 1) * config_.sector_size),
                sector.begin());
            const auto status = WriteSector(cylinder, head, physical_sector, sector);
            if (!status.ok())
            {
                return status;
            }
        }
    }

    return Status::OkStatus();
}

std::vector<std::vector<std::uint8_t>> MsxDosFileSystem::ReadRootDirectorySectors() const
{
    std::vector<std::vector<std::uint8_t>> sectors;
    sectors.reserve(config_.RootDirectorySectors());
    for (int sector_index = 0; sector_index < config_.RootDirectorySectors(); ++sector_index)
    {
        int cylinder;
        int head;
        int physical_sector;
        LbaToPhysical(config_, config_.GetRootDirectoryStartSector() + sector_index, cylinder, head, physical_sector);
        const auto sector = ReadSector(cylinder, head, physical_sector);
        sectors.push_back(sector.ok() ? sector.value() : std::vector<std::uint8_t>(config_.sector_size, 0x00));
    }
    return sectors;
}

Status MsxDosFileSystem::WriteRootDirectorySector(const int sector_index, const std::vector<std::uint8_t>& sector_data)
{
    int cylinder;
    int head;
    int physical_sector;
    LbaToPhysical(config_, config_.GetRootDirectoryStartSector() + sector_index, cylinder, head, physical_sector);
    return WriteSector(cylinder, head, physical_sector, sector_data);
}

Result<std::vector<std::uint8_t>> MsxDosFileSystem::ReadCluster(const int cluster) const
{
    std::vector<std::uint8_t> cluster_data(config_.ClusterSize(), 0x00);
    const auto start_lba = config_.ClusterToLba(cluster);
    for (int sector_index = 0; sector_index < config_.sectors_per_cluster; ++sector_index)
    {
        int cylinder;
        int head;
        int physical_sector;
        LbaToPhysical(config_, start_lba + sector_index, cylinder, head, physical_sector);
        const auto sector = ReadSector(cylinder, head, physical_sector);
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

Status MsxDosFileSystem::WriteCluster(const int cluster, const std::vector<std::uint8_t>& cluster_data)
{
    const auto start_lba = config_.ClusterToLba(cluster);
    for (int sector_index = 0; sector_index < config_.sectors_per_cluster; ++sector_index)
    {
        int cylinder;
        int head;
        int physical_sector;
        LbaToPhysical(config_, start_lba + sector_index, cylinder, head, physical_sector);
        std::vector<std::uint8_t> sector(config_.sector_size, 0x00);
        std::copy(
            cluster_data.begin() + (sector_index * config_.sector_size),
            cluster_data.begin() + ((sector_index + 1) * config_.sector_size),
            sector.begin());
        const auto status = WriteSector(cylinder, head, physical_sector, sector);
        if (!status.ok())
        {
            return status;
        }
    }

    return Status::OkStatus();
}

void MsxDosFileSystem::LbaToPhysical(const MsxDosConfiguration& config, const int lba, int& cylinder, int& head, int& sector)
{
    cylinder = lba / (config.sectors_per_track * config.number_of_heads);
    const auto within_cylinder = lba % (config.sectors_per_track * config.number_of_heads);
    head = within_cylinder / config.sectors_per_track;
    sector = (within_cylinder % config.sectors_per_track) + 1;
}

std::array<std::uint8_t, 32> MsxDosFileSystem::EncodeDirectoryEntry(const MsxDosFileEntry& entry)
{
    auto encoded = MsxDosFileEntryWriter::Write(entry);
    for (int index = 0; index < 8; ++index)
    {
        if (encoded[index] == 0x00)
        {
            encoded[index] = static_cast<std::uint8_t>(' ');
        }
    }

    for (int index = 8; index < 11; ++index)
    {
        if (encoded[index] == 0x00)
        {
            encoded[index] = static_cast<std::uint8_t>(' ');
        }
    }

    return encoded;
}
}
