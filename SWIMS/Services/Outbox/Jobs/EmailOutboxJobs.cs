using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using SWIMS.Services.Outbox;

namespace SWIMS.Services.Outbox.Jobs;

public sealed class EmailOutboxJobs
{
    private readonly IEmailOutbox _outbox;
    public EmailOutboxJobs(IEmailOutbox outbox) => _outbox = outbox;

    // Hangfire will inject PerformContext if present
    [Queue("outbox")] // 👈 shows in the “outbox” queue in the dashboard
    public async Task<int> RunOnceAsync(int take = 20, PerformContext? context = null, CancellationToken ct = default)
    {
        context?.WriteLine($"Scanning outbox… (take={take})");
        var sent = await _outbox.DispatchBatchAsync(take, ct);
        context?.WriteLine($"Dispatched: {sent}");
        return sent;
    }
}
