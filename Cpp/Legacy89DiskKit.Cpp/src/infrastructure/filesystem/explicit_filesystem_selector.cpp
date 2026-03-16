#include "legacy89diskkit/cpp/infrastructure/filesystem/explicit_filesystem_selector.hpp"

#include <algorithm>
#include <string>

namespace legacy89diskkit::cpp
{
namespace
{
std::string Normalize(std::string_view file_system_name)
{
    std::string normalized;
    normalized.reserve(file_system_name.size());
    for (const auto ch : file_system_name)
    {
        if (std::isalnum(static_cast<unsigned char>(ch)) != 0)
        {
            normalized.push_back(static_cast<char>(std::tolower(static_cast<unsigned char>(ch))));
        }
    }

    return normalized;
}

Result<ExplicitFileSystemSelection> OpenSelected(const FileSystemFamily family, RawDiskContainer& container)
{
    switch (family)
    {
    case FileSystemFamily::HuBasic:
        return Result<ExplicitFileSystemSelection>::Success(ExplicitFileSystemSelection{HuBasicFileSystem::Open(container)});
    case FileSystemFamily::N88Basic:
        return Result<ExplicitFileSystemSelection>::Success(ExplicitFileSystemSelection{N88BasicFileSystem::Open(container)});
    case FileSystemFamily::MsxDos:
        return Result<ExplicitFileSystemSelection>::Success(ExplicitFileSystemSelection{ExplicitMsxDosFileSystem::Open(container)});
    }

    return Result<ExplicitFileSystemSelection>::Failure(StatusCode::InvalidArgument, "Unsupported file system.");
}

Result<ExplicitFileSystemSelection> OpenSelected(const FileSystemFamily family, D88DiskContainer& container)
{
    switch (family)
    {
    case FileSystemFamily::HuBasic:
        return Result<ExplicitFileSystemSelection>::Success(ExplicitFileSystemSelection{HuBasicFileSystem::Open(container)});
    case FileSystemFamily::N88Basic:
        return Result<ExplicitFileSystemSelection>::Success(ExplicitFileSystemSelection{N88BasicFileSystem::Open(container)});
    case FileSystemFamily::MsxDos:
        return Result<ExplicitFileSystemSelection>::Success(ExplicitFileSystemSelection{ExplicitMsxDosFileSystem::Open(container)});
    }

    return Result<ExplicitFileSystemSelection>::Failure(StatusCode::InvalidArgument, "Unsupported file system.");
}
}

Result<FileSystemFamily> ExplicitFileSystemSelector::ParseFamily(const std::string_view file_system_name)
{
    const auto normalized = Normalize(file_system_name);
    if (normalized == "hubasic")
    {
        return Result<FileSystemFamily>::Success(FileSystemFamily::HuBasic);
    }

    if (normalized == "n88basic")
    {
        return Result<FileSystemFamily>::Success(FileSystemFamily::N88Basic);
    }

    if (normalized == "msxdos")
    {
        return Result<FileSystemFamily>::Success(FileSystemFamily::MsxDos);
    }

    return Result<FileSystemFamily>::Failure(StatusCode::InvalidArgument, "Unsupported file system.");
}

std::string_view ExplicitFileSystemSelector::GetCanonicalName(const FileSystemFamily family)
{
    const auto* entry = FileSystemSurfaceCatalog::FindByFamily(family);
    return entry == nullptr ? std::string_view{} : entry->name;
}

bool ExplicitFileSystemSelector::SupportsDiskType(const FileSystemFamily family, const DiskType disk_type)
{
    switch (family)
    {
    case FileSystemFamily::HuBasic:
        return disk_type == DiskType::TwoD || disk_type == DiskType::TwoDD || disk_type == DiskType::TwoHD;
    case FileSystemFamily::N88Basic:
        return disk_type == DiskType::TwoD || disk_type == DiskType::TwoDD;
    case FileSystemFamily::MsxDos:
        return disk_type == DiskType::TwoDD;
    }

    return false;
}

FileSystemFamily ExplicitFileSystemSelector::GetFamily(const ExplicitFileSystemSelection& selection)
{
    if (std::holds_alternative<HuBasicFileSystem>(selection))
    {
        return FileSystemFamily::HuBasic;
    }

    if (std::holds_alternative<N88BasicFileSystem>(selection))
    {
        return FileSystemFamily::N88Basic;
    }

    return FileSystemFamily::MsxDos;
}

Result<ExplicitFileSystemSelection> ExplicitFileSystemSelector::Open(
    const std::string_view file_system_name,
    RawDiskContainer& container)
{
    const auto family = ParseFamily(file_system_name);
    if (!family.ok())
    {
        return Result<ExplicitFileSystemSelection>::Failure(family.status().code, family.status().message);
    }

    if (!SupportsDiskType(family.value(), container.DiskTypeValue()))
    {
        return Result<ExplicitFileSystemSelection>::Failure(StatusCode::UnsupportedFormat, "Unsupported disk type for selected file system.");
    }

    return OpenSelected(family.value(), container);
}

Result<ExplicitFileSystemSelection> ExplicitFileSystemSelector::Open(
    const std::string_view file_system_name,
    D88DiskContainer& container)
{
    const auto family = ParseFamily(file_system_name);
    if (!family.ok())
    {
        return Result<ExplicitFileSystemSelection>::Failure(family.status().code, family.status().message);
    }

    if (!SupportsDiskType(family.value(), container.DiskTypeValue()))
    {
        return Result<ExplicitFileSystemSelection>::Failure(StatusCode::UnsupportedFormat, "Unsupported disk type for selected file system.");
    }

    return OpenSelected(family.value(), container);
}
}
