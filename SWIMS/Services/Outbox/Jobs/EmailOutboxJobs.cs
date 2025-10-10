namespace SWIMS.Services.Outbox.Jobs;

public sealed class EmailOutboxJobs
{
    private readonly IEmailOutbox _outbox;
    public EmailOutboxJobs(IEmailOutbox outbox) => _outbox = outbox;

    // Hangfire runs this
    public Task<int> RunOnceAsync(int take = 20, CancellationToken ct = default)
        => _outbox.DispatchBatchAsync(take, ct);
}
