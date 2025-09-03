using Microwave.Application;
using Microwave.Domain;
using Microwave.Tests.Shared;

namespace Microwave.Tests.Integration;

public class MicrowaveServiceIntegrationTests
{
    [Fact]
    public void TestCompleteHeatingWorkflow()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var startResult = service.StartHeating(60, 8, storage);
        Assert.True(startResult.Success);

        var status = service.GetHeatingProgress(storage);
        Assert.True(status.IsRunning);
        Assert.Equal(60, status.RemainingTime);
        Assert.Equal(8, status.PowerLevel);

        var addTimeResult = service.IncreaseTime(30, storage);
        Assert.True(addTimeResult.Success);

        var pauseResult = service.PauseOrCancel(storage);
        Assert.True(pauseResult.Success);

        var pausedStatus = service.GetHeatingProgress(storage);
        Assert.False(pausedStatus.IsRunning);
        Assert.Contains("pausado", pausedStatus.StatusMessage.ToLower());

        var resumeResult = service.StartHeating(90, 7, storage);
        Assert.True(resumeResult.Success);

        var cancelResult = service.PauseOrCancel(storage);
        Assert.True(cancelResult.Success);
        var finalCancelResult = service.PauseOrCancel(storage);
        Assert.True(finalCancelResult.Success);

        var finalStatus = service.GetHeatingProgress(storage);
        Assert.False(finalStatus.IsRunning);
        Assert.Equal(0, finalStatus.RemainingTime);
    }

    [Fact]
    public void TestPredefinedProgramWorkflow()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var programs = service.GetPredefinedPrograms();
        Assert.Equal(5, programs.Count());

        var startResult = service.StartPredefinedProgram("Pipoca", storage);
        Assert.True(startResult.Success);

        var status = service.GetHeatingProgress(storage);
        Assert.True(status.IsRunning);
        Assert.Equal(180, status.RemainingTime);
        Assert.Equal(7, status.PowerLevel);

        var addTimeResult = service.IncreaseTime(30, storage);
        Assert.False(addTimeResult.Success);

        var pauseResult = service.PauseOrCancel(storage);
        Assert.True(pauseResult.Success);

        var resumeResult = service.StartPredefinedProgram("Pipoca", storage);
        Assert.True(resumeResult.Success);

        var cancelResult = service.PauseOrCancel(storage);
        Assert.True(cancelResult.Success);
        var finalCancelResult = service.PauseOrCancel(storage);
        Assert.True(finalCancelResult.Success);
    }

    [Fact]
    public void TestQuickStartWorkflow()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var quickStartResult = service.StartQuickHeat(storage);
        Assert.True(quickStartResult.Success);
        Assert.Contains("30s a potência 10", quickStartResult.Message);

        var addTimeResult = service.IncreaseTime(30, storage);
        Assert.True(addTimeResult.Success);
        Assert.Contains("60s", addTimeResult.Message);

        var status = service.GetHeatingProgress(storage);
        Assert.True(status.IsRunning);
        Assert.Equal(60, status.RemainingTime);
        Assert.Equal(10, status.PowerLevel);
    }

    [Fact]
    public void TestErrorHandlingAndRecovery()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var invalidTimeResult = service.StartHeating(200, 5, storage);
        Assert.False(invalidTimeResult.Success);

        var invalidPowerResult = service.StartHeating(30, 15, storage);
        Assert.False(invalidPowerResult.Success);

        var addTimeWithoutHeatingResult = service.IncreaseTime(30, storage);
        Assert.False(addTimeWithoutHeatingResult.Success);

        var validStartResult = service.StartHeating(45, 6, storage);
        Assert.True(validStartResult.Success);

        var status = service.GetHeatingProgress(storage);
        Assert.True(status.IsRunning);
        Assert.Equal(45, status.RemainingTime);
        Assert.Equal(6, status.PowerLevel);
    }
}
