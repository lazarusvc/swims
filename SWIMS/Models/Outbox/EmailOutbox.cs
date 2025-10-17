namespace SWIMS.Models.Outbox;

public sealed class EmailOutbox
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string To { get; set; } = default!;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }

    public string Subject { get; set; } = default!;
    public string? BodyText { get; set; }
    public string? BodyHtml { get; set; }

    public string? HeadersJson { get; set; }

    public int Attempts { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentUtc { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
    public string? LastError { get; set; }
}

public sealed class EmailDeadLetter
{
    public Guid Id { get; set; }
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string? BodyText { get; set; }
    public string? BodyHtml { get; set; }
    public string? HeadersJson { get; set; }
    public int Attempts { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime FailedUtc { get; set; } = DateTime.UtcNow;
    public string Error { get; set; } = default!;
}
