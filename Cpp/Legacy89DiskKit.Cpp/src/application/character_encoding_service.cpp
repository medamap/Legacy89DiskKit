#include "legacy89diskkit/cpp/application/character_encoding_service.hpp"
#include "legacy89diskkit/cpp/infrastructure/character_encoding/character_encoding_table_catalog.hpp"

namespace legacy89diskkit::cpp::application
{
Result<std::vector<std::uint8_t>> CharacterEncodingService::EncodeText(
    std::string_view text,
    std::string_view encoding_id) const
{
    auto table_result = CharacterEncodingTableCatalog::Find(encoding_id);
    if (!table_result.ok())
    {
        return Result<std::vector<std::uint8_t>>::Failure(table_result.status().code, table_result.status().message);
    }

    ByteTextEncodingIndex index(*table_result.value());
    return Result<std::vector<std::uint8_t>>::Success(index.Encode(text));
}

Result<std::string> CharacterEncodingService::DecodeText(
    std::span<const std::uint8_t> data,
    std::string_view encoding_id,
    std::string_view newline) const
{
    auto table_result = CharacterEncodingTableCatalog::Find(encoding_id);
    if (!table_result.ok())
    {
        return Result<std::string>::Failure(table_result.status().code, table_result.status().message);
    }

    ByteTextEncodingIndex index(*table_result.value());
    return Result<std::string>::Success(index.Decode(data, newline));
}
} // namespace legacy89diskkit::cpp::application
