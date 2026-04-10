#include "legacy89diskkit/cpp/msx_dos_file_entry_writer.hpp"

#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"

namespace legacy89diskkit::cpp
{
std::array<std::uint8_t, 32> MsxDosFileEntryWriter::Write(const MsxDosFileEntry& entry)
{
    return MsxDosDirParser::Write(entry);
}
}
