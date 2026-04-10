#include "legacy89diskkit/cpp/hu_basic_label_rules.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
bool HuBasicLabelRules::IsVirtualLabelEntry(const HuBasicFileEntry& entry)
{
    if (entry.metadata.file_type != HuBasicFileType::Ascii)
    {
        return false;
    }

    const auto looks_decorative = std::all_of(
        entry.file_name.begin(),
        entry.file_name.end(),
        [](const char ch)
        {
            return ch == '-' || ch == '.' || ch == ' ';
        });
    const auto has_sentinel_addresses = entry.load_address == 0xffff &&
                                        entry.execution_address == 0xffff &&
                                        (entry.end_address == 0xffff || entry.size == 0);
    const auto suspicious_cluster = entry.start_cluster >= 0x7fff;
    const auto label_flags = entry.metadata.has_password &&
                             entry.metadata.is_write_protected &&
                             !entry.metadata.is_hidden &&
                             !entry.metadata.is_verify;

    return (looks_decorative || suspicious_cluster || has_sentinel_addresses) &&
           (label_flags || suspicious_cluster || has_sentinel_addresses);
}

bool HuBasicLabelRules::CanMergeLabelEntries(const HuBasicFileEntry& previous, const HuBasicFileEntry& current)
{
    if (!previous.extension.empty() || previous.file_name.empty())
    {
        return false;
    }

    if (!current.extension.empty() || current.file_name.empty())
    {
        return false;
    }

    if (!current.file_name.starts_with("."))
    {
        return false;
    }

    return previous.metadata.raw_mode_byte == current.metadata.raw_mode_byte &&
           previous.metadata.password_byte == current.metadata.password_byte &&
           previous.size == current.size &&
           previous.load_address == current.load_address &&
           previous.end_address == current.end_address &&
           previous.execution_address == current.execution_address &&
           previous.start_cluster == current.start_cluster;
}
}
