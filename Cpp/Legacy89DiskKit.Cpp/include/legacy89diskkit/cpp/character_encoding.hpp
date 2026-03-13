#pragma once

#include "legacy89diskkit/cpp/status.hpp"

#include <string>
#include <string_view>

namespace legacy89diskkit::cpp
{
struct CharacterEncodingProfile
{
    std::string encoding_id;
    std::string display_name;
    std::string machine_type;
};

class CharacterEncodingResolver
{
public:
    static Result<CharacterEncodingProfile> ResolveProfile(
        std::string_view encoding_override,
        std::string_view default_encoding_id,
        std::string_view platform_id);
};
}
