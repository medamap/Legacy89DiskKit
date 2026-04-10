#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicParsedName HuBasicNameRules::ParseFileName(const std::string& file_name)
{
    const auto separator = file_name.find('.');
    auto name = separator == std::string::npos ? file_name : file_name.substr(0, separator);
    auto extension = separator == std::string::npos ? std::string() : file_name.substr(separator + 1);

    if (name.size() > 13)
    {
        name = name.substr(0, 13);
    }

    if (extension.size() > 3)
    {
        extension = extension.substr(0, 3);
    }

    return HuBasicParsedName{ name, extension };
}

std::string HuBasicNameRules::BuildDisplayName(const std::string& file_name, const std::string& extension)
{
    if (extension.empty())
    {
        return file_name;
    }

    return file_name + "." + extension;
}
}
