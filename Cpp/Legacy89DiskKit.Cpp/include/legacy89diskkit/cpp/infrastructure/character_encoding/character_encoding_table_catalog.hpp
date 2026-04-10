#pragma once

#include "legacy89diskkit/cpp/infrastructure/character_encoding/byte_text_encoding_table.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string_view>

namespace legacy89diskkit::cpp
{
class CharacterEncodingTableCatalog
{
public:
    static Result<const ByteTextEncodingTable*> Find(std::string_view encoding_id);
};
}
