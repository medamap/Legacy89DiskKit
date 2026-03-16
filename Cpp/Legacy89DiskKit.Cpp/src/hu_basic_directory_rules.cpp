#include "legacy89diskkit/cpp/hu_basic_directory_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileMetadata HuBasicDirectoryRules::CreateMetadataForWrite(
    const std::vector<std::uint8_t>& data,
    const HuBasicFileAttributes& attributes,
    int start_cluster,
    std::uint16_t load_address,
    std::uint16_t execution_address)
{
    const auto file_type = attributes.is_ascii || (attributes.raw_attributes & 0x0c) != 0
        ? HuBasicFileType::Ascii
        : HuBasicFileType::Binary;

    return HuBasicFileMetadata
    {
        file_type,
        static_cast<std::uint16_t>(data.size()),
        load_address,
        execution_address,
        start_cluster,
        attributes.raw_attributes
    };
}

HuBasicFileEntry HuBasicDirectoryRules::CreateFileEntryForWrite(
    const std::string& file_name,
    const std::vector<std::uint8_t>& data,
    const HuBasicFileAttributes& attributes,
    int start_cluster,
    std::uint16_t load_address,
    std::uint16_t execution_address)
{
    const auto parsed_name = HuBasicNameRules::ParseFileName(file_name);
    const auto end_address = static_cast<std::uint16_t>(load_address + data.size() - 1);

    return HuBasicFileEntry
    {
        parsed_name.file_name,
        parsed_name.extension,
        static_cast<std::uint32_t>(data.size()),
        attributes,
        start_cluster,
        load_address,
        end_address,
        execution_address
    };
}
}
