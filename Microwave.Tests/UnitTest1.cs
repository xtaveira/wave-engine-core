using Microwave.Domain;
using Microwave.Application;

namespace Microwave.Tests;

public class MockStateStorage : IStateStorage
{
    private readonly Dictionary<string, string> _storage = new();

    public void SetString(string key, string value) => _storage[key] = value;
    public string? GetString(string key) => _storage.TryGetValue(key, out var value) ? value : null;
    public void SetInt32(string key, int value) => _storage[key] = value.ToString();
    public int? GetInt32(string key) => _storage.TryGetValue(key, out var value) && int.TryParse(value, out var intValue) ? intValue : null;
    public void Remove(string key) => _storage.Remove(key);
}

public class UnitTest1
{
    [Fact]
    public void TestMicrowaveOvenCreationAndHeating()
    {
        var microwave = new MicrowaveOven(30, 5);

        microwave.StartHeating();

        Assert.Equal(30, microwave.TimeInSeconds);
        Assert.Equal(5, microwave.PowerLevel);
    }

    [Fact]
    public void TestMicrowaveOvenInvalidTime()
    {
        Assert.Throws<ArgumentException>(() => new MicrowaveOven(150, 5));
    }

    [Fact]
    public void TestMicrowaveOvenInvalidPower()
    {
        Assert.Throws<ArgumentException>(() => new MicrowaveOven(30, 15));
    }

    [Fact]
    public void TestMicrowaveServiceStartHeating()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartHeating(30, 5, storage);

        Assert.True(result.Success);
        Assert.Equal("Aquecimento iniciado: 30s a potência 5.", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceGetHeatingProgress()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartHeating(10, 5, storage);

        var startTime = DateTime.Now.AddSeconds(-1).ToString("O");
        storage.SetString("StartTime", startTime);

        var progress1 = service.GetHeatingProgress(storage);

        Assert.True(progress1.IsRunning);
        Assert.NotEmpty(progress1.StatusMessage);
        Assert.Equal(".....", progress1.StatusMessage);
    }

    [Fact]
    public void TestMicrowaveServiceStartQuickHeat()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartQuickHeat(storage);

        Assert.True(result.Success);
        Assert.Equal("Aquecimento iniciado: 30s a potência 10.", result.Message);
    }

    [Fact]
    public void TestStopHeating_ShouldPauseHeating()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(10, 5, mockStorage);
        var result = service.PauseOrCancel(mockStorage);

        Assert.True(result.Success);
        Assert.Contains("pausado", result.Message.ToLower());
    }

    [Fact]
    public void TestProgressString_WithDifferentPowerLevels()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(3, 1, mockStorage);
        var startTime = DateTime.Now.AddSeconds(-2).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.True(status.IsRunning);
        Assert.Equal(". .", status.StatusMessage);
    }

    [Fact]
    public void TestProgressString_WithHigherPowerLevel()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(4, 5, mockStorage);
        var startTime = DateTime.Now.AddSeconds(-2).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.True(status.IsRunning);
        Assert.Equal("..... .....", status.StatusMessage);
    }

    [Fact]
    public void TestProgressString_WhenCompleted_ShouldShowCompletionMessage()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(3, 2, mockStorage);

        var startTime = DateTime.Now.AddSeconds(-3).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.False(status.IsRunning);
        Assert.Equal(100, status.Progress);
        Assert.Equal(".. .. .. Aquecimento concluído", status.StatusMessage);
    }

    [Fact]
    public void TestTimeFormatting_ShouldFormatTimeBetween60And100Seconds()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(90, 5, mockStorage);
        var startTime = DateTime.Now.AddSeconds(-10).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.True(status.IsRunning);
        Assert.Equal(80, status.RemainingTime);
        Assert.Equal("1:20", status.FormattedRemainingTime);
    }

    [Fact]
    public void TestTimeFormatting_ShouldKeepSecondsFormatFor60SecondsOrLess()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(45, 3, mockStorage);
        var startTime = DateTime.Now.AddSeconds(-5).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.True(status.IsRunning);
        Assert.Equal(40, status.RemainingTime);
        Assert.Equal("40s", status.FormattedRemainingTime);
    }

    [Fact]
    public void TestTimeFormatting_ShouldKeepSecondsFormatFor100SecondsOrMore()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(120, 8, mockStorage);
        var startTime = DateTime.Now.AddSeconds(-20).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.True(status.IsRunning);
        Assert.Equal(100, status.RemainingTime); // 120 - 20 = 100
        Assert.Equal("100s", status.FormattedRemainingTime);
    }

    [Fact]
    public void TestStartHeating_ShouldShowFormattedTimeInMessage()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        var result = service.StartHeating(75, 6, mockStorage);

        Assert.True(result.Success);
        Assert.Contains("1:15", result.Message);
        Assert.Contains("potência 6", result.Message);
    }

    [Fact]
    public void TestPauseHeating_ShouldPauseAndAllowResume()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(60, 5, mockStorage);

        var startTime = DateTime.Now.AddSeconds(-10).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var pauseResult = service.PauseOrCancel(mockStorage);

        Assert.True(pauseResult.Success);
        Assert.Contains("pausado", pauseResult.Message.ToLower());

        var statusPaused = service.GetHeatingProgress(mockStorage);
        Assert.False(statusPaused.IsRunning);
        Assert.Equal(50, statusPaused.RemainingTime);
        Assert.Contains("PAUSADO", statusPaused.StatusMessage);

        var resumeResult = service.StartHeating(0, 0, mockStorage);

        Assert.True(resumeResult.Success);
        Assert.Contains("retomado", resumeResult.Message.ToLower());
    }

    [Fact]
    public void TestCancelFromPaused_ShouldClearAll()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(60, 8, mockStorage);
        var startTime = DateTime.Now.AddSeconds(-5).ToString("O");
        mockStorage.SetString("StartTime", startTime);
        service.PauseOrCancel(mockStorage);

        var cancelResult = service.PauseOrCancel(mockStorage);

        Assert.True(cancelResult.Success);
        Assert.Contains("cancelado", cancelResult.Message.ToLower());

        var statusAfterCancel = service.GetHeatingProgress(mockStorage);
        Assert.False(statusAfterCancel.IsRunning);
        Assert.Equal(0, statusAfterCancel.RemainingTime);
        Assert.Equal("Micro-ondas parado.", statusAfterCancel.StatusMessage);
    }

    [Fact]
    public void TestClearSettings_WhenNotStarted()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        mockStorage.SetString("CurrentOven", "{\"timeInSeconds\":30,\"powerLevel\":5}");
        mockStorage.SetString("MicrowaveState", "STOPPED");

        var clearResult = service.PauseOrCancel(mockStorage);

        Assert.True(clearResult.Success);
        Assert.Contains("limpas", clearResult.Message.ToLower());
    }

    [Fact]
    public void TestIncreaseTime_ShouldWorkDuringHeating()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartHeating(30, 7, mockStorage);

        var increaseResult = service.IncreaseTime(30, mockStorage);

        Assert.True(increaseResult.Success);
        Assert.Contains("aumentado", increaseResult.Message.ToLower());
        Assert.Contains("60s", increaseResult.Message);
    }
}
