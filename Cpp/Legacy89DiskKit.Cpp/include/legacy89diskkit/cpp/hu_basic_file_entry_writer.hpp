#pragma once

#include "legacy89diskkit/cpp/hu_basic_directory_entry.hpp"
#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicFileEntryWriter
{
public:
    static HuBasicDirectoryEntry ToDirectoryEntry(const HuBasicFileEntry& entry);
};
}
