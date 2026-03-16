#include "legacy89diskkit/cpp/n88_basic_file_entry_writer.hpp"

#include "legacy89diskkit/cpp/n88_basic_dir_parser.hpp"

namespace legacy89diskkit::cpp
{
std::array<std::uint8_t, 16> N88BasicFileEntryWriter::Write(const N88BasicFileEntry& entry)
{
    return N88BasicDirParser::Write(entry);
}
}
