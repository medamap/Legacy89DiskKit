#pragma once

#include "legacy89diskkit/cpp/status.hpp"
#include <string>
#include <string_view>
#include <vector>
#include <span>

namespace legacy89diskkit::cpp::application
{
class CharacterEncodingService
{
public:
    CharacterEncodingService() = default;

    Result<std::vector<std::uint8_t>> EncodeText(
        std::string_view text,
        std::string_view encoding_id) const;

    Result<std::string> DecodeText(
        std::span<const std::uint8_t> data,
        std::string_view encoding_id,
        std::string_view newline = "\n") const;
};
} // namespace legacy89diskkit::cpp::application
