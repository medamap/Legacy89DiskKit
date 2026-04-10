#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"

#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileEntry HuBasicDirParser::Parse(const HuBasicDirectoryEntry& entry)
{
    const auto file_type = HuBasicModeRules::GetFileType(entry.mode_byte);
    const auto is_ascii = file_type == HuBasicFileType::Ascii;
    const auto is_directory = (entry.mode_byte & 0x80) != 0;
    const auto is_read_only = (entry.mode_byte & 0x40) != 0;
    const auto is_verify = (entry.mode_byte & 0x20) != 0;
    const auto is_hidden = (entry.mode_byte & 0x10) != 0;

    const auto metadata = HuBasicFileMetadata
    {
        file_type,
        entry.password_byte != 0x20,
        is_hidden,
        is_verify,
        is_read_only,
        is_directory,
        entry.recorded_size,
        entry.load_address,
        entry.execution_address,
        entry.start_cluster,
        entry.mode_byte,
        entry.password_byte
    };

    std::uint16_t end_address = 0;
    if (entry.recorded_size > 0)
    {
        end_address = static_cast<std::uint16_t>(entry.load_address + entry.recorded_size - 1);
    }

    return HuBasicFileEntry
    {
        entry.file_name,
        entry.extension,
        entry.recorded_size,
        HuBasicFileAttributes
        {
            is_ascii,
            entry.mode_byte,
            is_directory,
            is_read_only,
            is_hidden
        },
        entry.start_cluster,
        entry.load_address,
        end_address,
        entry.execution_address,
        metadata
    };
}
}
