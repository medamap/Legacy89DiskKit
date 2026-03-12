using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Application.FileSystem;

public enum DirectoryLayoutValidationSeverity
{
    Warning,
    Error
}

public sealed record DirectoryLayoutTextPlan(
    IReadOnlyList<DirectoryLayoutTextPlanEntry> Entries
);

public sealed record DirectoryLayoutTextPlanEntry(
    int LineNumber,
    bool IsLabel,
    string StableId,
    string Text
);

public sealed record DirectoryLayoutValidationMessage(
    DirectoryLayoutValidationSeverity Severity,
    int LineNumber,
    string Message
);

public sealed record DirectoryLayoutValidationResult(
    DirectoryLayoutTextPlan Plan,
    IReadOnlyList<DirectoryLayoutValidationMessage> Messages,
    DirectoryEntryLayout? ProposedLayout
)
{
    public int ErrorCount => Messages.Count(message => message.Severity == DirectoryLayoutValidationSeverity.Error);
    public int WarningCount => Messages.Count(message => message.Severity == DirectoryLayoutValidationSeverity.Warning);
    public bool IsValid => ErrorCount == 0;
}
