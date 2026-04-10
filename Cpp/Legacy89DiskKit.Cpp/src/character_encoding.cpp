#include "legacy89diskkit/cpp/character_encoding.hpp"

#include <array>

namespace legacy89diskkit::cpp
{
namespace
{
struct EncodingDescriptor
{
    std::string_view encoding_id;
    std::string_view display_name;
    std::string_view machine_type;
};

constexpr std::array<EncodingDescriptor, 4> Encodings{{
    {"x1", "x1", "X1"},
    {"pc8801", "pc8801", "Pc8801"},
    {"msx1", "msx1", "Msx1"},
    {"sjis", "sjis", "Unknown"}
}};

const EncodingDescriptor* FindDescriptor(std::string_view encoding_id)
{
    for (const auto& descriptor : Encodings)
    {
        if (descriptor.encoding_id == encoding_id)
        {
            return &descriptor;
        }
    }

    return nullptr;
}
}

Result<CharacterEncodingProfile> CharacterEncodingResolver::ResolveProfile(
    std::string_view encoding_override,
    std::string_view default_encoding_id,
    std::string_view platform_id)
{
    for (const auto candidate : {encoding_override, default_encoding_id, platform_id})
    {
        if (candidate.empty())
        {
            continue;
        }

        if (const auto* descriptor = FindDescriptor(candidate))
        {
            return Result<CharacterEncodingProfile>::Success(CharacterEncodingProfile{
                std::string(descriptor->encoding_id),
                std::string(descriptor->display_name),
                std::string(descriptor->machine_type)});
        }
    }

    return Result<CharacterEncodingProfile>::Failure(
        StatusCode::InvalidArgument,
        "No known character encoding profile matches the requested identifiers.");
}
}
