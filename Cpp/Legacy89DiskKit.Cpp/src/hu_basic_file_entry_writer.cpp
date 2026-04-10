#include "legacy89diskkit/cpp/hu_basic_file_entry_writer.hpp"

#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
namespace
{
template <std::size_t Size>
std::array<std::uint8_t, Size> EncodePaddedText(const std::string& text)
{
    std::array<std::uint8_t, Size> bytes{};
    bytes.fill(static_cast<std::uint8_t>(' '));

    const auto count = std::min(Size, text.size());
    for (std::size_t index = 0; index < count; ++index)
    {
        bytes[index] = static_cast<std::uint8_t>(text[index]);
    }

    return bytes;
}
}

HuBasicDirectoryEntry HuBasicFileEntryWriter::ToDirectoryEntry(const HuBasicFileEntry& entry)
{
    const auto mode_byte = HuBasicModeRules::BuildModeByte(entry.metadata);

    return HuBasicDirectoryEntry
    {
        mode_byte,
        entry.metadata.password_byte,
        EncodePaddedText<13>(entry.file_name),
        EncodePaddedText<3>(entry.extension),
        entry.file_name,
        entry.extension,
        entry.metadata.recorded_size,
        entry.load_address,
        entry.execution_address,
        entry.start_cluster
    };
}
}
