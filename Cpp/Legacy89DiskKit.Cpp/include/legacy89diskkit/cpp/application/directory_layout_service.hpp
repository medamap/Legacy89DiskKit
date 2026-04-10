#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"
#include "legacy89diskkit/cpp/application/character_encoding_service.hpp"
#include "legacy89diskkit/cpp/domain/directory_layout_types.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string>
#include <string_view>
#include <vector>
#include <optional>

namespace legacy89diskkit::cpp::application
{
enum class DirectoryLayoutValidationSeverity
{
    Info,
    Warning,
    Error,
};

struct DirectoryLayoutValidationMessage
{
    DirectoryLayoutValidationSeverity severity;
    int line_number;
    std::string message;
};

struct DirectoryLayoutValidationResult
{
    bool is_valid;
    int warning_count;
    std::vector<DirectoryLayoutValidationMessage> messages;
    std::optional<DirectoryLayout> proposed_layout;
};

enum class DirectorySortBy
{
    Name,
    Extension,
    Type,
};

class DirectoryLayoutService
{
public:
    explicit DirectoryLayoutService(NativeFileSystemSession* session);

    Result<DirectoryLayout> GetLayout() const;
    
    Result<std::string> ExportPlan() const;
    
    Result<DirectoryLayoutValidationResult> ValidatePlan(const std::string& plan_text) const;
    
    Result<DirectoryLayoutValidationResult> ApplyPlan(const std::string& plan_text, bool strict = false);

    Result<DirectoryLayout> MoveEntryBefore(const std::string& source_name, const std::string& target_name);
    
    Result<DirectoryLayout> InsertLabelBefore(const std::string& label_text, const std::string& target_name);
    
    Result<DirectoryLayout> SortEntries(DirectorySortBy sort_by);

    static std::string CreateStableId(const std::string& raw_id);

private:
    NativeFileSystemSession* session_;
    CharacterEncodingService encoding_service_;

    // Helper methods for internal orchestration
    Result<DirectoryLayout> Reindex(const std::vector<DirectoryLayoutItem>& items) const;
};
} // namespace legacy89diskkit::cpp::application
