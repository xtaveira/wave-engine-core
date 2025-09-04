using System.Text.Json;
using Microwave.Domain.Interfaces;

namespace Microwave.Infrastructure.Logging;

public class FileExceptionLogger : IExceptionLogger
{
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileExceptionLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
        EnsureLogDirectoryExists();
    }

    public async Task LogExceptionAsync(Exception exception, string? requestId = null, string? userId = null, Dictionary<string, object>? additionalData = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "Error",
                Message = exception.Message,
                Exception = exception.ToString(),
                InnerException = exception.InnerException?.ToString(),
                StackTrace = exception.StackTrace,
                RequestId = requestId,
                UserId = userId,
                AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null
            };

            var logLine = JsonSerializer.Serialize(logEntry);
            await File.AppendAllTextAsync(_logFilePath, logLine + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 50)
    {
        if (!File.Exists(_logFilePath))
            return new List<LogEntry>();

        await _semaphore.WaitAsync();
        try
        {
            var lines = await File.ReadAllLinesAsync(_logFilePath);
            var logEntries = lines
                .Reverse()
                .Take(count)
                .Select(line =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<LogEntry>(line);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(entry => entry != null)
                .Cast<LogEntry>()
                .ToList();

            return logEntries;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<LogEntry>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (!File.Exists(_logFilePath))
            return new List<LogEntry>();

        await _semaphore.WaitAsync();
        try
        {
            var lines = await File.ReadAllLinesAsync(_logFilePath);
            var logEntries = lines
                .Select(line =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<LogEntry>(line);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(entry => entry != null &&
                               entry.Timestamp >= startDate &&
                               entry.Timestamp <= endDate)
                .Cast<LogEntry>()
                .OrderByDescending(entry => entry.Timestamp)
                .ToList();

            return logEntries;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void EnsureLogDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
