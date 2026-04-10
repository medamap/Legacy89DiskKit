#include "legacy89diskkit/cpp/hu_basic_boot_record_codec.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
namespace
{
void WritePaddedText(
    std::vector<std::uint8_t>& data,
    const std::size_t offset,
    const std::size_t length,
    const std::string& text)
{
    std::fill_n(data.begin() + offset, length, static_cast<std::uint8_t>(' '));
    const auto count = std::min(length, text.size());
    for (std::size_t index = 0; index < count; ++index)
    {
        data[offset + index] = static_cast<std::uint8_t>(text[index]);
    }
}

void WriteUInt16(std::vector<std::uint8_t>& data, const std::size_t offset, const std::uint16_t value)
{
    data[offset] = static_cast<std::uint8_t>(value & 0xff);
    data[offset + 1] = static_cast<std::uint8_t>((value >> 8) & 0xff);
}
}

std::vector<std::uint8_t> HuBasicBootRecordCodec::Write(const HuBasicBootRecordInfo& record)
{
    std::vector<std::uint8_t> data(32, 0x00);
    data[0] = record.boot_flag;
    WritePaddedText(data, 1, 13, record.file_name);
    WritePaddedText(data, 0x0e, 3, record.extension);
    data[0x11] = record.has_password ? static_cast<std::uint8_t>(0x21) : static_cast<std::uint8_t>(0x20);
    WriteUInt16(data, 0x12, record.size);
    WriteUInt16(data, 0x14, record.load_address);
    WriteUInt16(data, 0x16, record.execution_address);
    WriteUInt16(data, 0x1e, record.start_record);
    return data;
}
}
