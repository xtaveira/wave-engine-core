using Xunit;
using Microwave.Infrastructure.Logging;
using Microwave.Domain.Interfaces;

namespace Microwave.Tests.Unit.Infrastructure;

public class FileExceptionLoggerTests : IDisposable
{
    private readonly string _testLogPath;
    private readonly FileExceptionLogger _logger;

    public FileExceptionLoggerTests()
    {
        _testLogPath = Path.Combine(Path.GetTempPath(), $"test-log-{Guid.NewGuid()}.log");
        _logger = new FileExceptionLogger(_testLogPath);
    }

    [Fact]
    public async Task LogExceptionAsync_WithBasicException_LogsSuccessfully()
    {
        var exception = new InvalidOperationException("Test exception message");
        var requestId = "test-request-123";
        var userId = "test-user";

        await _logger.LogExceptionAsync(exception, requestId, userId);

        Assert.True(File.Exists(_testLogPath));
        var logContent = await File.ReadAllTextAsync(_testLogPath);
        Assert.Contains("Test exception message", logContent);
        Assert.Contains(requestId, logContent);
        Assert.Contains(userId, logContent);
    }

    [Fact]
    public async Task LogExceptionAsync_WithAdditionalData_IncludesDataInLog()
    {
        var exception = new ArgumentException("Invalid argument");
        var additionalData = new Dictionary<string, object>
        {
            ["Operation"] = "TestOperation",
            ["UserId"] = 123,
            ["Timestamp"] = DateTime.UtcNow
        };

        await _logger.LogExceptionAsync(exception, "req-123", "user-456", additionalData);

        var logContent = await File.ReadAllTextAsync(_testLogPath);
        Assert.Contains("TestOperation", logContent);
        Assert.Contains("123", logContent);
    }

    [Fact]
    public async Task LogExceptionAsync_WithInnerException_LogsBothExceptions()
    {
        var innerException = new ArgumentNullException("parameter", "Parameter cannot be null");
        var outerException = new InvalidOperationException("Operation failed", innerException);

        await _logger.LogExceptionAsync(outerException);

        var logContent = await File.ReadAllTextAsync(_testLogPath);
        Assert.Contains("Operation failed", logContent);
        Assert.Contains("Parameter cannot be null", logContent);
    }

    [Fact]
    public async Task GetRecentLogsAsync_WhenNoLogs_ReturnsEmptyCollection()
    {
        var logs = await _logger.GetRecentLogsAsync();

        Assert.Empty(logs);
    }

    [Fact]
    public async Task GetRecentLogsAsync_WithMultipleLogs_ReturnsInReverseOrder()
    {
        var exception1 = new Exception("First exception");
        var exception2 = new Exception("Second exception");
        var exception3 = new Exception("Third exception");

        await _logger.LogExceptionAsync(exception1, "req-1");
        await Task.Delay(10);
        await _logger.LogExceptionAsync(exception2, "req-2");
        await Task.Delay(10);
        await _logger.LogExceptionAsync(exception3, "req-3");

        var logs = await _logger.GetRecentLogsAsync(10);

        var logList = logs.ToList();
        Assert.Equal(3, logList.Count);

        Assert.Contains("Third exception", logList[0].Message);
        Assert.Contains("Second exception", logList[1].Message);
        Assert.Contains("First exception", logList[2].Message);
    }

    [Fact]
    public async Task GetRecentLogsAsync_WithCountLimit_ReturnsOnlyRequestedCount()
    {
        for (int i = 1; i <= 5; i++)
        {
            await _logger.LogExceptionAsync(new Exception($"Exception {i}"), $"req-{i}");
            await Task.Delay(5);
        }

        var logs = await _logger.GetRecentLogsAsync(3);

        Assert.Equal(3, logs.Count());
    }

    [Fact]
    public async Task GetLogsByDateRangeAsync_WithinRange_ReturnsCorrectLogs()
    {
        var startDate = DateTime.UtcNow.AddHours(-1);
        var endDate = DateTime.UtcNow.AddHours(1);

        await _logger.LogExceptionAsync(new Exception("Test exception"), "req-1");

        var logs = await _logger.GetLogsByDateRangeAsync(startDate, endDate);

        Assert.Single(logs);
        Assert.Contains("Test exception", logs.First().Message);
    }

    [Fact]
    public async Task GetLogsByDateRangeAsync_OutsideRange_ReturnsEmpty()
    {
        var startDate = DateTime.UtcNow.AddHours(-3);
        var endDate = DateTime.UtcNow.AddHours(-2);

        await _logger.LogExceptionAsync(new Exception("Test exception"), "req-1");

        var logs = await _logger.GetLogsByDateRangeAsync(startDate, endDate);

        Assert.Empty(logs);
    }

    [Fact]
    public async Task LogExceptionAsync_CreatesLogDirectoryIfNotExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-log-dir-{Guid.NewGuid()}");
        var logPath = Path.Combine(tempDir, "test.log");
        var logger = new FileExceptionLogger(logPath);

        var exception = new Exception("Test exception");

        try
        {
            await logger.LogExceptionAsync(exception);

            Assert.True(Directory.Exists(tempDir));
            Assert.True(File.Exists(logPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task LogExceptionAsync_MultipleThreads_HandlesThreadSafety()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var exception = new Exception($"Exception from thread {index}");
                await _logger.LogExceptionAsync(exception, $"req-{index}");
            }));
        }

        await Task.WhenAll(tasks);

        var logs = await _logger.GetRecentLogsAsync(20);
        Assert.Equal(10, logs.Count());
    }

    public void Dispose()
    {
        if (File.Exists(_testLogPath))
        {
            File.Delete(_testLogPath);
        }
    }
}
