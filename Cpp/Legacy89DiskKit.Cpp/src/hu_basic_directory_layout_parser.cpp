#include "legacy89diskkit/cpp/hu_basic_directory_layout_parser.hpp"

#include "legacy89diskkit/cpp/hu_basic_directory_listing.hpp"
#include "legacy89diskkit/cpp/hu_basic_label_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
namespace
{
bool TryMergeVirtualLabelExtension(
    std::vector<HuBasicDirectoryLayoutItem>& items,
    const HuBasicDirectoryLayoutItem& item)
{
    if (item.kind != HuBasicDirectoryLayoutItemKind::VirtualLabel || items.empty())
    {
        return false;
    }

    auto& previous = items.back();
    if (previous.kind != HuBasicDirectoryLayoutItemKind::VirtualLabel)
    {
        return false;
    }

    if (!HuBasicLabelRules::CanMergeLabelEntries(previous.entry, item.entry))
    {
        return false;
    }

    previous.entry.extension = item.entry.file_name.substr(1);
    previous.display_name = HuBasicNameRules::BuildDisplayName(previous.entry.file_name, previous.entry.extension);
    return true;
}
}

HuBasicDirectoryLayout HuBasicDirectoryLayoutParser::Parse(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size)
{
    HuBasicDirectoryLayout layout{};
    const auto files = HuBasicDirectoryListing::ListFiles(directory_sectors, sector_size);

    auto order = 0;
    for (const auto& entry : files)
    {
        const auto display_name = HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension);
        const auto item = HuBasicDirectoryLayoutItem
        {
            display_name,
            order++,
            HuBasicLabelRules::IsVirtualLabelEntry(entry) ? HuBasicDirectoryLayoutItemKind::VirtualLabel : HuBasicDirectoryLayoutItemKind::FileEntry,
            display_name,
            entry
        };

        if (TryMergeVirtualLabelExtension(layout.items, item))
        {
            continue;
        }

        layout.items.push_back(item);
    }

    return layout;
}
}
