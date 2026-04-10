#include "legacy89diskkit/cpp/application/boot_and_clone_service.hpp"
#include "legacy89diskkit/cpp/hu_basic_boot_record_parser.hpp"
#include <algorithm>

namespace legacy89diskkit::cpp::application
{
Status BootAndCloneService::TransferBootArea(NativeFileSystemSession* source, NativeFileSystemSession* target)
{
    if (!source || !target) return {StatusCode::InvalidArgument, "Source or target session is null."};

    auto boot_result = source->ReadBootArea();
    if (!boot_result.ok()) return boot_result.status();

    return target->WriteBootArea(boot_result.value());
}

Status BootAndCloneService::TransferFiles(NativeFileSystemSession* source, NativeFileSystemSession* target, const std::vector<std::string>& file_names)
{
    if (!source || !target) return {StatusCode::InvalidArgument, "Source or target session is null."};

    for (const auto& name : file_names)
    {
        auto data_result = source->ReadFile(name);
        if (!data_result.ok()) return data_result.status();

        auto source_files = source->GetFiles();
        auto it = std::find_if(source_files.begin(), source_files.end(), [&](const auto& f) {
            std::string full_name = f.file_name;
            if (!f.extension.empty()) full_name += "." + f.extension;
            return full_name == name;
        });

        if (it == source_files.end())
        {
            return {StatusCode::InvalidArgument, "Source file metadata not found: " + name};
        }

        auto write_status = target->WriteFile(name, data_result.value(), it->attributes, it->load_address, it->execution_address);
        if (!write_status.ok()) return write_status;
    }

    return Status::OkStatus();
}

Result<BootInfoSummary> BootAndCloneService::GetBootInfoSummary(const NativeFileSystemSession* session)
{
    if (!session) return Result<BootInfoSummary>::Failure(StatusCode::InvalidArgument, "Session is null.");

    auto boot_result = session->ReadBootArea();
    if (!boot_result.ok()) return Result<BootInfoSummary>::Failure(boot_result.status().code, boot_result.status().message);

    const auto& boot_data = boot_result.value();
    
    // Check for "None" mode (all zero or all FF commonly used for uninitialized areas)
    bool is_empty = std::all_of(boot_data.begin(), boot_data.end(), [](std::uint8_t b) { return b == 0x00 || b == 0xFF; });
    if (is_empty)
    {
        BootInfoSummary summary;
        summary.mode = BootInfoMode::None;
        return Result<BootInfoSummary>::Success(std::move(summary));
    }

    if (session->Family() == FileSystemFamily::HuBasic)
    {
        auto record = HuBasicBootRecordParser::Parse(boot_data);
        // Valid Hu-BASIC boot record must have a non-empty filename
        if (record.has_value() && !record->file_name.empty())
        {
            BootInfoSummary summary;
            summary.mode = BootInfoMode::FileBacked;
            summary.file_name = record->file_name;
            summary.load_address = record->load_address;
            summary.execution_address = record->execution_address;
            return Result<BootInfoSummary>::Success(std::move(summary));
        }
    }

    // Default for recognized but non-file-backed areas
    BootInfoSummary summary;
    summary.mode = BootInfoMode::SectorResident;
    return Result<BootInfoSummary>::Success(std::move(summary));
}
} // namespace legacy89diskkit::cpp::application
