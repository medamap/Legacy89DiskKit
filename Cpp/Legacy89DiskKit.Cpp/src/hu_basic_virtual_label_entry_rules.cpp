#include "legacy89diskkit/cpp/hu_basic_virtual_label_entry_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileEntry HuBasicVirtualLabelEntryRules::CreateEntry(
    const std::string& file_name,
    const std::string& extension,
    const std::uint8_t raw_mode_byte,
    const std::uint8_t password_byte,
    const std::uint16_t size,
    const std::uint16_t load_address,
    const std::uint16_t end_address,
    const std::uint16_t execution_address,
    const int start_cluster)
{
    return HuBasicFileEntry
    {
        file_name,
        extension,
        size,
        HuBasicFileAttributes{ true, raw_mode_byte, false, true, false },
        start_cluster,
        load_address,
        end_address,
        execution_address,
        HuBasicFileMetadata
        {
            HuBasicFileType::Ascii,
            true,
            false,
            false,
            true,
            false,
            size,
            load_address,
            execution_address,
            start_cluster,
            raw_mode_byte,
            password_byte
        }
    };
}
}
