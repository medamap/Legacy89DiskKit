#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
enum class HuBasicDirectoryLayoutItemKind
{
    FileEntry,
    VirtualLabel,
};

struct HuBasicDirectoryLayoutItem
{
    std::string id;
    int order;
    HuBasicDirectoryLayoutItemKind kind;
    std::string display_name;
    HuBasicFileEntry entry;
};

struct HuBasicDirectoryLayout
{
    std::vector<HuBasicDirectoryLayoutItem> items;
};
}
