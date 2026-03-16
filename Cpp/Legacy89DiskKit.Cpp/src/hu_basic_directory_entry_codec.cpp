#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
namespace
{
std::string DecodeTrimmed(const std::uint8_t* bytes, int length)
{
    std::string value;
    value.reserve(length);

    for (auto index = 0; index < length; ++index)
    {
        value.push_back(static_cast<char>(bytes[index]));
    }

    while (!value.empty() && value.back() == ' ')
    {
        value.pop_back();
    }

    return value;
}

void EncodePadded(const std::string& text, std::uint8_t* destination, int length)
{
    std::fill(destination, destination + length, static_cast<std::uint8_t>(' '));
    for (auto index = 0; index < length && index < static_cast<int>(text.size()); ++index)
    {
        destination[index] = static_cast<std::uint8_t>(text[index]);
    }
}
}

HuBasicDirectoryEntry HuBasicDirectoryEntryCodec::Parse(const std::array<std::uint8_t, 32>& data)
{
    HuBasicDirectoryEntry entry{};
    entry.mode_byte = data[0];
    entry.password_byte = data[0x11];
    std::copy_n(data.begin() + 1, 13, entry.raw_file_name.begin());
    std::copy_n(data.begin() + 0x0e, 3, entry.raw_extension.begin());
    entry.file_name = DecodeTrimmed(entry.raw_file_name.data(), static_cast<int>(entry.raw_file_name.size()));
    entry.extension = DecodeTrimmed(entry.raw_extension.data(), static_cast<int>(entry.raw_extension.size()));
    entry.recorded_size = static_cast<std::uint16_t>(data[0x12] | (data[0x13] << 8));
    entry.load_address = static_cast<std::uint16_t>(data[0x14] | (data[0x15] << 8));
    entry.execution_address = static_cast<std::uint16_t>(data[0x16] | (data[0x17] << 8));
    entry.start_cluster = (data[0x1f] << 7) | (data[0x1e] & 0x7f);
    return entry;
}

std::array<std::uint8_t, 32> HuBasicDirectoryEntryCodec::Write(const HuBasicDirectoryEntry& entry)
{
    std::array<std::uint8_t, 32> buffer{};
    buffer[0] = entry.mode_byte;
    EncodePadded(entry.file_name, buffer.data() + 1, 13);
    EncodePadded(entry.extension, buffer.data() + 0x0e, 3);
    buffer[0x11] = entry.password_byte;
    buffer[0x12] = static_cast<std::uint8_t>(entry.recorded_size & 0xff);
    buffer[0x13] = static_cast<std::uint8_t>((entry.recorded_size >> 8) & 0xff);
    buffer[0x14] = static_cast<std::uint8_t>(entry.load_address & 0xff);
    buffer[0x15] = static_cast<std::uint8_t>((entry.load_address >> 8) & 0xff);
    buffer[0x16] = static_cast<std::uint8_t>(entry.execution_address & 0xff);
    buffer[0x17] = static_cast<std::uint8_t>((entry.execution_address >> 8) & 0xff);
    buffer[0x1d] = static_cast<std::uint8_t>((entry.start_cluster >> 14) & 0x7f);
    buffer[0x1e] = static_cast<std::uint8_t>(entry.start_cluster & 0x7f);
    buffer[0x1f] = static_cast<std::uint8_t>((entry.start_cluster >> 7) & 0x7f);
    return buffer;
}
}
