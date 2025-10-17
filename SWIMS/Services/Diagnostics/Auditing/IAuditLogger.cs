namespace SWIMS.Services.Diagnostics.Auditing;

public interface IAuditLogger
{
    Task LogAsync(
        string action,
        string entity,
        string? entityId,
        int userId,
        string username,
        object? oldObj = null,
        object? newObj = null,
        string? ip = null,
        object? extra = null,
        CancellationToken ct = default);
}