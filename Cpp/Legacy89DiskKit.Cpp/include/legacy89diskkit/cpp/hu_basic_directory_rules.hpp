#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicDirectoryRules
{
public:
    static HuBasicFileEntry CreateFileEntryForWrite(
        const std::string& file_name,
        const std::vector<std::uint8_t>& data,
        const HuBasicFileAttributes& attributes,
        int start_cluster,
        std::uint16_t load_address,
        std::uint16_t execution_address);

    static HuBasicFileMetadata CreateMetadataForWrite(
        const std::vector<std::uint8_t>& data,
        const HuBasicFileAttributes& attributes,
        int start_cluster,
        std::uint16_t load_address,
        std::uint16_t execution_address);
};
}
