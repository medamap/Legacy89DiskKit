#include "legacy89diskkit/cpp/application/file_transfer_service.hpp"
#include <filesystem>
#include <fstream>
#include <iostream>
#include <algorithm>

namespace legacy89diskkit::cpp::application
{
FileTransferService::FileTransferService(NativeFileSystemSession* session)
    : session_(session)
{
}

Status FileTransferService::ImportFile(
    const std::string& host_path,
    const std::string& disk_file_name,
    const bool is_ascii)
{
    if (!session_)
    {
        return {StatusCode::InvalidArgument, "Native session is not initialized."};
    }

    std::vector<std::uint8_t> disk_data;
    if (is_ascii)
    {
        std::ifstream stream(host_path);
        if (!stream)
        {
            return {StatusCode::InvalidArgument, "Could not open host file for reading."};
        }
        std::string text((std::istreambuf_iterator<char>(stream)), std::istreambuf_iterator<char>());
        
        auto encode_result = encoding_service_.EncodeText(text, GetDefaultEncodingId());
        if (!encode_result.ok())
        {
            return encode_result.status();
        }
        disk_data = std::move(encode_result.value());

        // Hu-BASIC style 0x1A terminator for text files
        if (session_->Family() == FileSystemFamily::HuBasic)
        {
            if (disk_data.empty() || disk_data.back() != 0x1A)
            {
                disk_data.push_back(0x1A);
            }
        }
    }
    else
    {
        std::ifstream stream(host_path, std::ios::binary);
        if (!stream)
        {
            return {StatusCode::InvalidArgument, "Could not open host file for reading."};
        }
        disk_data = std::vector<std::uint8_t>(std::istreambuf_iterator<char>(stream), std::istreambuf_iterator<char>());
    }

    // Use default attributes based on mode
    std::uint16_t attributes = 0;
    if (is_ascii)
    {
        // 0x01 in most our internal bit-mappings for attributes usually means ASCII or protection,
        // but here we follow the C# reference which calls CreateDefaultAttributes(isAscii).
        // For Hu-BASIC, ASCII is bit 0 of the first byte of attributes.
        attributes = 0x01; 
    }

    return session_->WriteFile(disk_file_name, disk_data, attributes);
}

Status FileTransferService::ExportFile(
    const std::string& disk_file_name,
    const std::string& host_path,
    std::optional<std::string_view> newline_override)
{
    if (!session_)
    {
        return {StatusCode::InvalidArgument, "Native session is not initialized."};
    }

    auto read_result = session_->ReadFile(disk_file_name);
    if (!read_result.ok())
    {
        return read_result.status();
    }

    const auto& disk_data = read_result.value();

    // Determine if ASCII by looking at the file entry
    bool is_ascii = false;
    const auto files = session_->GetFiles();
    auto it = std::find_if(files.begin(), files.end(), [&](const auto& f) {
        std::string full_name = f.file_name;
        if (!f.extension.empty())
        {
            full_name += "." + f.extension;
        }
        return full_name == disk_file_name;
    });

    if (it != files.end())
    {
        // In our NativeBridgeFileEntry, attributes is 16-bit. 
        // For Hu-BASIC/N88-BASIC/MSX-DOS, we need to know which bit is ASCII.
        // C# reference checks entry.Attributes.IsAscii.
        // For simplicity in this phase, we assume bit 0 is ASCII (matches our Internal mapping).
        is_ascii = (it->attributes & 0x01) != 0;
    }

    if (is_ascii)
    {
        std::string newline = newline_override.has_value() ? std::string(newline_override.value()) : "\n";
        auto decode_result = encoding_service_.DecodeText(disk_data, GetDefaultEncodingId(), newline);
        if (!decode_result.ok())
        {
            return decode_result.status();
        }

        std::ofstream stream(host_path);
        if (!stream)
        {
            return {StatusCode::InvalidArgument, "Could not open host file for writing."};
        }
        stream << decode_result.value();
    }
    else
    {
        std::ofstream stream(host_path, std::ios::binary);
        if (!stream)
        {
            return {StatusCode::InvalidArgument, "Could not open host file for writing."};
        }
        stream.write(reinterpret_cast<const char*>(disk_data.data()), static_cast<std::streamsize>(disk_data.size()));
    }

    return Status::OkStatus();
}

std::string FileTransferService::GetDefaultEncodingId() const
{
    // Default to X1 for Hu-BASIC, PC88 for N88-BASIC, etc.
    if (!session_) return "ascii";
    
    switch (session_->Family())
    {
    case FileSystemFamily::HuBasic: return "x1";
    case FileSystemFamily::N88Basic: return "pc88";
    case FileSystemFamily::MsxDos: return "msx";
    default: return "ascii";
    }
}
} // namespace legacy89diskkit::cpp::application
