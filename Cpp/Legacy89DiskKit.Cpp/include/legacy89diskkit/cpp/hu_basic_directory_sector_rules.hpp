#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicDirectorySectorRules
{
public:
    static std::optional<int> FindWritableSlotOffset(const std::vector<std::uint8_t>& sector_data, int sector_size);
    static std::optional<int> FindEntryOffset(const std::vector<std::uint8_t>& sector_data, int sector_size, const std::string& full_name);
    static int CountActiveEntries(const std::vector<std::uint8_t>& sector_data, int sector_size);
    static void MarkEntryDeleted(std::vector<std::uint8_t>& sector_data, int offset);
};
}
