#include "legacy89diskkit/cpp/infrastructure/character_encoding/byte_text_encoding_table.hpp"

namespace legacy89diskkit::cpp
{
ByteTextEncodingIndex::ByteTextEncodingIndex(const ByteTextEncodingTable& table)
    : table_(&table)
{
    for (std::size_t i = 0; i < table.byte_to_text.size(); ++i)
    {
        if (!table.byte_to_text[i].empty())
        {
            text_to_byte_.try_emplace(std::string(table.byte_to_text[i]), static_cast<std::uint8_t>(i));
        }
    }

    text_to_byte_["\r"] = 0x0d;
    text_to_byte_["\n"] = 0x0d;
    text_to_byte_["\x1A"] = 0x1a;
}

std::string ByteTextEncodingIndex::Decode(const std::span<const std::uint8_t> data, const std::string_view newline) const
{
    std::string text;
    for (const auto value : data)
    {
        if (value == 0x1a)
        {
            break;
        }

        if (value == 0x0d)
        {
            text.append(newline);
            continue;
        }

        text.append(table_->byte_to_text[value]);
    }

    return text;
}

std::vector<std::uint8_t> ByteTextEncodingIndex::Encode(const std::string_view text) const
{
    std::vector<std::uint8_t> encoded;
    for (std::size_t i = 0; i < text.size(); ++i)
    {
        if (text[i] == '\r')
        {
            encoded.push_back(0x0d);
            if (i + 1 < text.size() && text[i + 1] == '\n')
            {
                ++i;
            }
            continue;
        }

        if (text[i] == '\n')
        {
            encoded.push_back(0x0d);
            continue;
        }

        std::string symbol(1, text[i]);
        if ((static_cast<unsigned char>(text[i]) & 0x80u) != 0u)
        {
            if (i + 1 < text.size() &&
                (static_cast<unsigned char>(text[i + 1]) & 0xc0u) == 0x80u)
            {
                symbol.push_back(text[++i]);
                if (i + 1 < text.size() &&
                    (static_cast<unsigned char>(text[i + 1]) & 0xc0u) == 0x80u)
                {
                    symbol.push_back(text[++i]);
                }
            }
        }

        const auto it = text_to_byte_.find(symbol);
        encoded.push_back(it == text_to_byte_.end() ? 0x20 : it->second);
    }

    return encoded;
}
}
