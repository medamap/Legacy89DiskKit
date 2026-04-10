#include "legacy89diskkit/cpp/application/directory_layout_service.hpp"
#include <algorithm>
#include <iomanip>
#include <sstream>
#include <set>

namespace legacy89diskkit::cpp::application
{
DirectoryLayoutService::DirectoryLayoutService(NativeFileSystemSession* session)
    : session_(session)
{
}

Result<DirectoryLayout> DirectoryLayoutService::GetLayout() const
{
    if (!session_)
    {
        return Result<DirectoryLayout>::Failure(StatusCode::InvalidArgument, "Native session is not initialized.");
    }

    auto layout_result = session_->ReadDirectoryLayout();
    if (!layout_result.ok())
    {
        return layout_result;
    }

    auto layout = std::move(layout_result.value());
    for (auto& item : layout.items)
    {
        item.stable_id = CreateStableId(item.id);
    }

    return Result<DirectoryLayout>::Success(std::move(layout));
}

Result<std::string> DirectoryLayoutService::ExportPlan() const
{
    auto layout_result = GetLayout();
    if (!layout_result.ok())
    {
        return Result<std::string>::Failure(layout_result.status().code, layout_result.status().message);
    }

    std::stringstream ss;
    auto items = layout_result.value().items;
    std::sort(items.begin(), items.end(), [](const auto& a, const auto& b) { return a.order < b.order; });

    for (const auto& item : items)
    {
        if (item.kind == DirectoryLayoutItemKind::VirtualLabel)
        {
            ss << "# " << item.display_name << "\n";
        }
        else
        {
            ss << item.stable_id << " " << item.display_name << "\n";
        }
    }

    return Result<std::string>::Success(ss.str());
}

Result<DirectoryLayoutValidationResult> DirectoryLayoutService::ValidatePlan(const std::string& plan_text) const
{
    auto current_layout_result = GetLayout();
    if (!current_layout_result.ok())
    {
        return Result<DirectoryLayoutValidationResult>::Failure(current_layout_result.status().code, current_layout_result.status().message);
    }

    const auto& current_layout = current_layout_result.value();
    DirectoryLayoutValidationResult result;
    result.is_valid = true;
    result.warning_count = 0;

    std::vector<DirectoryLayoutItem> proposed_items;
    std::set<std::string> seen_stable_ids;
    std::set<std::string> consumed_ids;

    std::stringstream ss(plan_text);
    std::string line;
    int line_number = 0;

    while (std::getline(ss, line))
    {
        line_number++;
        if (line.empty() || std::all_of(line.begin(), line.end(), isspace)) continue;

        if (line[0] == '#')
        {
            // Label
            DirectoryLayoutItem label;
            label.kind = DirectoryLayoutItemKind::VirtualLabel;
            label.display_name = line.substr(1);
            // Trim leading/trailing spaces from label
            label.display_name.erase(0, label.display_name.find_first_not_of(" "));
            label.display_name.erase(label.display_name.find_last_not_of(" ") + 1);
            label.id = "label:" + std::to_string(proposed_items.size()) + ":" + label.display_name;
            label.stable_id = ""; // Labels don't need stable IDs in the same way
            label.order = static_cast<int>(proposed_items.size());
            proposed_items.push_back(std::move(label));
            continue;
        }

        std::stringstream line_ss(line);
        std::string stable_id, display_name;
        line_ss >> stable_id;
        std::getline(line_ss, display_name);
        if (!display_name.empty() && display_name[0] == ' ') display_name.erase(0, 1);

        if (seen_stable_ids.count(stable_id))
        {
            result.messages.push_back({DirectoryLayoutValidationSeverity::Error, line_number, "Duplicate entry id: " + stable_id});
            result.is_valid = false;
            continue;
        }
        seen_stable_ids.insert(stable_id);

        auto it = std::find_if(current_layout.items.begin(), current_layout.items.end(), 
            [&](const auto& item) { return item.stable_id == stable_id; });

        if (it == current_layout.items.end())
        {
            result.messages.push_back({DirectoryLayoutValidationSeverity::Error, line_number, "Unknown entry id: " + stable_id});
            result.is_valid = false;
            continue;
        }

        consumed_ids.insert(it->id);
        DirectoryLayoutItem proposed = *it;
        proposed.display_name = display_name;
        proposed.order = static_cast<int>(proposed_items.size());
        proposed_items.push_back(std::move(proposed));
    }

    // Check for omitted items
    for (const auto& item : current_layout.items)
    {
        if (item.kind == DirectoryLayoutItemKind::FileEntry && consumed_ids.find(item.id) == consumed_ids.end())
        {
            result.warning_count++;
            result.messages.push_back({DirectoryLayoutValidationSeverity::Warning, 0, "Entry omitted from plan and moved to the end: " + item.display_name});
            DirectoryLayoutItem omitted = item;
            omitted.order = static_cast<int>(proposed_items.size());
            proposed_items.push_back(std::move(omitted));
        }
    }

    if (result.is_valid)
    {
        DirectoryLayout layout;
        layout.items = std::move(proposed_items);
        result.proposed_layout = std::move(layout);
    }

    return Result<DirectoryLayoutValidationResult>::Success(std::move(result));
}

Result<DirectoryLayoutValidationResult> DirectoryLayoutService::ApplyPlan(const std::string& plan_text, bool strict)
{
    auto validation_result = ValidatePlan(plan_text);
    if (!validation_result.ok()) return validation_result;

    auto& validation = validation_result.value();
    if (!validation.is_valid || (strict && validation.warning_count > 0))
    {
        return Result<DirectoryLayoutValidationResult>::Success(std::move(validation));
    }

    if (!validation.proposed_layout.has_value())
    {
        return Result<DirectoryLayoutValidationResult>::Failure(StatusCode::InvalidArgument, "No proposed layout available.");
    }

    auto status = session_->ApplyDirectoryLayout(validation.proposed_layout.value());
    if (!status.ok())
    {
        return Result<DirectoryLayoutValidationResult>::Failure(status.code, status.message);
    }

    return Result<DirectoryLayoutValidationResult>::Success(std::move(validation));
}

Result<DirectoryLayout> DirectoryLayoutService::MoveEntryBefore(const std::string& source_name, const std::string& target_name)
{
    auto layout_result = GetLayout();
    if (!layout_result.ok()) return layout_result;

    auto items = std::move(layout_result.value().items);
    auto source_it = std::find_if(items.begin(), items.end(), [&](const auto& i) { return i.display_name == source_name; });
    auto target_it = std::find_if(items.begin(), items.end(), [&](const auto& i) { return i.display_name == target_name; });

    if (source_it == items.end() || target_it == items.end())
    {
        return Result<DirectoryLayout>::Failure(StatusCode::InvalidArgument, "Entry not found.");
    }

    auto source_item = std::move(*source_it);
    items.erase(source_it);
    
    // Refresh target_it after erase
    target_it = std::find_if(items.begin(), items.end(), [&](const auto& i) { return i.display_name == target_name; });
    items.insert(target_it, std::move(source_item));

    return Reindex(items);
}

Result<DirectoryLayout> DirectoryLayoutService::InsertLabelBefore(const std::string& label_text, const std::string& target_name)
{
    auto layout_result = GetLayout();
    if (!layout_result.ok()) return layout_result;

    auto items = std::move(layout_result.value().items);
    auto target_it = std::find_if(items.begin(), items.end(), [&](const auto& i) { return i.display_name == target_name; });

    if (target_it == items.end())
    {
        return Result<DirectoryLayout>::Failure(StatusCode::InvalidArgument, "Target entry not found.");
    }

    DirectoryLayoutItem label;
    label.kind = DirectoryLayoutItemKind::VirtualLabel;
    label.display_name = label_text;
    label.id = "label:" + std::to_string(std::hash<std::string>{}(label_text)) + ":" + label_text;
    label.stable_id = "";
    
    items.insert(target_it, std::move(label));

    return Reindex(items);
}

Result<DirectoryLayout> DirectoryLayoutService::SortEntries(DirectorySortBy sort_by)
{
    if (sort_by != DirectorySortBy::Name)
    {
        return Result<DirectoryLayout>::Failure(StatusCode::UnsupportedFormat, "Only Name-based sorting is currently supported.");
    }

    auto layout_result = GetLayout();
    if (!layout_result.ok()) return layout_result;

    auto items = std::move(layout_result.value().items);
    
    // Separate labels and files to maintain label positions if possible (like C#)
    std::stable_sort(items.begin(), items.end(), [&](const auto& a, const auto& b) {
        if (a.kind == DirectoryLayoutItemKind::VirtualLabel || b.kind == DirectoryLayoutItemKind::VirtualLabel) return false;
        return a.display_name < b.display_name;
    });

    return Reindex(items);
}

std::string DirectoryLayoutService::CreateStableId(const std::string& raw_id)
{
    // WARNING: std::hash is not guaranteed to be stable across different runs or platforms.
    // This is a temporary measure for Phase V2-25. Full parity with C# SHA256 IDs 
    // should be implemented in a later phase when a crypto provider is available.
    size_t hash = std::hash<std::string>{}(raw_id);
    std::stringstream ss;
    ss << std::hex << std::setw(8) << std::setfill('0') << (hash & 0xFFFFFFFF);
    return ss.str().substr(0, 8);
}

Result<DirectoryLayout> DirectoryLayoutService::Reindex(const std::vector<DirectoryLayoutItem>& items) const
{
    DirectoryLayout layout;
    layout.items = items;
    for (size_t i = 0; i < layout.items.size(); ++i)
    {
        layout.items[i].order = static_cast<int>(i);
    }

    auto status = session_->ApplyDirectoryLayout(layout);
    if (!status.ok())
    {
        return Result<DirectoryLayout>::Failure(status.code, status.message);
    }

    return Result<DirectoryLayout>::Success(std::move(layout));
}
} // namespace legacy89diskkit::cpp::application
