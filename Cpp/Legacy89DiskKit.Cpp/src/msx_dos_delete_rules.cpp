#include "legacy89diskkit/cpp/msx_dos_delete_rules.hpp"

#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
void MsxDosDeleteRules::FreeClusters(std::vector<std::uint8_t>& fat_data, const std::vector<int>& clusters)
{
    for (const auto cluster : clusters)
    {
        MsxDosFatRules::SetEntry(fat_data, cluster, 0x000);
    }
}
}
