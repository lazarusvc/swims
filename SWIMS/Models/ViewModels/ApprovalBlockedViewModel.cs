namespace SWIMS.Models.ViewModels;

public sealed class ApprovalBlockedViewModel : ErrorPageViewModel
{
    public string OriginalPath { get; set; } = string.Empty;
    public string OriginalQueryString { get; set; } = string.Empty;

    public int? DataId { get; set; }

    public int? FormId { get; set; }
    public string? FormName { get; set; }
    public string? FormUuid { get; set; }

    public int? AttemptedApprovalLevel { get; set; }
    public int? ActiveApprovalLevel { get; set; }
    public bool IsFullyApproved { get; set; }

    public int ApprovalLevelsConfigured { get; set; }

    public IReadOnlyList<int> UserApprovalLevels { get; set; } = Array.Empty<int>();

    public IReadOnlyList<ApprovalStepStatus> Steps { get; set; } = Array.Empty<ApprovalStepStatus>();

    public string? Hint { get; set; }

    public string? ApprovalsDashboardUrl { get; set; }
    public string? ApprovalsReturnUrl { get; set; }
}

public sealed class ApprovalStepStatus
{
    public int Level { get; set; }
    public bool IsComplete { get; set; }

    public string? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Comment { get; set; }
}
