#pragma once

#include "legacy89diskkit/cpp/infrastructure/character_encoding/byte_text_encoding_table.hpp"

namespace legacy89diskkit::cpp
{
class X1EncodingTable
{
public:
    static const ByteTextEncodingTable& Get();
};
}
