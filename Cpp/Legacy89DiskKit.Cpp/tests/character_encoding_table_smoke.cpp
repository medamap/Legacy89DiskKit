#include "legacy89diskkit/cpp/infrastructure/character_encoding/byte_text_encoding_table.hpp"
#include "legacy89diskkit/cpp/infrastructure/character_encoding/character_encoding_table_catalog.hpp"
#include "legacy89diskkit/cpp/infrastructure/character_encoding/x1_encoding_table.hpp"

#include <vector>

using namespace legacy89diskkit::cpp;

int main()
{
    const auto catalog_result = CharacterEncodingTableCatalog::Find("X1");
    if (!catalog_result.ok())
    {
        return 1;
    }

    const auto* table = catalog_result.value();
    if (table != &X1EncodingTable::Get())
    {
        return 2;
    }

    ByteTextEncodingIndex index(*table);
    const auto decoded = index.Decode(std::vector<std::uint8_t>{0xa7, 0xa8, 0x0d, 0xe2, 0x1a, 0xff}, "\n");
    if (decoded != "ｧｨ\n♠")
    {
        return 3;
    }

    const auto encoded = index.Encode("A\nπ♠?");
    if (encoded.size() != 5)
    {
        return 4;
    }

    if (encoded[0] != 0x41 || encoded[1] != 0x0d || encoded[2] != 0x7f || encoded[3] != 0xe2 || encoded[4] != 0x3f)
    {
        return 5;
    }

    const auto unsupported = CharacterEncodingTableCatalog::Find("sjis");
    if (unsupported.ok() || unsupported.status().code != StatusCode::UnsupportedFormat)
    {
        return 6;
    }

    return 0;
}
