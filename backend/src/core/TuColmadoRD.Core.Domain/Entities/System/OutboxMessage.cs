namespace TuColmadoRD.Core.Domain.Entities.System;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { Type = string.Empty; Payload = string.Empty; }

    public OutboxMessage(string type, string payload)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void RecordTransientFailure(string error)
    {
        RetryCount++;
        LastError = error;
    }

    public void RecordPermanentFailure(string error)
    {
        ProcessedAt = DateTime.UtcNow;
        LastError = $"MAX_RETRIES_EXCEEDED: {error}";
    }
}
