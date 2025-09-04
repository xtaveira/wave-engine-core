namespace Microwave.Domain.Interfaces;

public interface IExceptionLogger
{
    Task LogExceptionAsync(Exception exception, string? requestId = null, string? userId = null, Dictionary<string, object>? additionalData = null);
    Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 50);
    Task<IEnumerable<LogEntry>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

public class LogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Error";
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? InnerException { get; set; }
    public string? StackTrace { get; set; }
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
    public string? AdditionalData { get; set; }
}
