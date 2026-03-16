#pragma once

#include <array>
#include <cstdint>
#include <span>
#include <string>
#include <string_view>
#include <unordered_map>
#include <vector>

namespace legacy89diskkit::cpp
{
struct ByteTextEncodingTable
{
    std::string_view encoding_id;
    std::array<std::string_view, 256> byte_to_text;
};

class ByteTextEncodingIndex
{
public:
    explicit ByteTextEncodingIndex(const ByteTextEncodingTable& table);

    std::string Decode(std::span<const std::uint8_t> data, std::string_view newline) const;
    std::vector<std::uint8_t> Encode(std::string_view text) const;

private:
    const ByteTextEncodingTable* table_;
    std::unordered_map<std::string, std::uint8_t> text_to_byte_;
};
}
