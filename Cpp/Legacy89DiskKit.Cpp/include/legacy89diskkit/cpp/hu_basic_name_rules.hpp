#pragma once

#include <string>

namespace legacy89diskkit::cpp
{
struct HuBasicParsedName
{
    std::string file_name;
    std::string extension;
};

class HuBasicNameRules
{
public:
    static HuBasicParsedName ParseFileName(const std::string& file_name);
    static std::string BuildDisplayName(const std::string& file_name, const std::string& extension);
};
}
