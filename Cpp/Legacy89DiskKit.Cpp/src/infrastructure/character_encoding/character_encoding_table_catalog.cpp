#include "legacy89diskkit/cpp/infrastructure/character_encoding/character_encoding_table_catalog.hpp"

#include "legacy89diskkit/cpp/infrastructure/character_encoding/x1_encoding_table.hpp"

#include <cctype>
#include <string>

namespace legacy89diskkit::cpp
{
Result<const ByteTextEncodingTable*> CharacterEncodingTableCatalog::Find(const std::string_view encoding_id)
{
    if (encoding_id.empty())
    {
        return Result<const ByteTextEncodingTable*>::Failure(StatusCode::InvalidArgument, "Encoding identifier must be specified.");
    }

    std::string normalized;
    normalized.reserve(encoding_id.size());
    for (const auto ch : encoding_id)
    {
        normalized.push_back(static_cast<char>(std::tolower(static_cast<unsigned char>(ch))));
    }

    if (normalized == "x1")
    {
        return Result<const ByteTextEncodingTable*>::Success(&X1EncodingTable::Get());
    }

    return Result<const ByteTextEncodingTable*>::Failure(StatusCode::UnsupportedFormat, "No concrete encoding table matches the requested identifier.");
}
}
