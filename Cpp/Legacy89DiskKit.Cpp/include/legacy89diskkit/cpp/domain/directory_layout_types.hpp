#pragma once

#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
enum class DirectoryLayoutItemKind
{
    FileEntry,
    VirtualLabel,
};

struct DirectoryLayoutItem
{
    std::string id;
    int order;
    DirectoryLayoutItemKind kind;
    std::string display_name;
    std::string stable_id;
};

struct DirectoryLayout
{
    std::vector<DirectoryLayoutItem> items;
};
} // namespace legacy89diskkit::cpp
