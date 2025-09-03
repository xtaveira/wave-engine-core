using Microwave.Application;
using Microwave.Tests.Shared;

namespace Microwave.Tests.Scenarios;

public class Level1HeatingScenarios
{
    [Fact]
    public void Scenario_ManualHeating_BasicOperation()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartHeating(120, 6, storage);

        Assert.True(result.Success);
        Assert.Contains("Aquecimento iniciado: 2:00 a potência 6", result.Message);

        var status = service.GetHeatingProgress(storage);
        Assert.True(status.IsRunning);
        Assert.Equal(120, status.RemainingTime);
        Assert.Equal(6, status.PowerLevel);
    }

    [Fact]
    public void Scenario_QuickStart_ConvenientOperation()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartQuickHeat(storage);

        Assert.True(result.Success);
        Assert.Contains("30s a potência 10", result.Message);

        var addTimeResult = service.IncreaseTime(30, storage);
        Assert.True(addTimeResult.Success);
        Assert.Contains("60s", addTimeResult.Message);
    }

    [Fact]
    public void Scenario_PauseAndResume_FlexibleControl()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        service.StartHeating(90, 7, storage);

        var pauseResult = service.PauseOrCancel(storage);

        Assert.True(pauseResult.Success);

        var pausedStatus = service.GetHeatingProgress(storage);
        Assert.False(pausedStatus.IsRunning);
        Assert.Contains("pausado", pausedStatus.StatusMessage.ToLower());

        var resumeResult = service.StartHeating(90, 7, storage);

        Assert.True(resumeResult.Success);
        Assert.Contains("Aquecimento retomado", resumeResult.Message);
    }

    [Fact]
    public void Scenario_ProgressVisualization_UserFeedback()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartHeating(30, 3, storage);
        var status3 = service.GetHeatingProgress(storage);

        service.PauseOrCancel(storage);
        service.PauseOrCancel(storage);
        service.StartHeating(30, 8, storage);
        var status8 = service.GetHeatingProgress(storage);

        Assert.Equal(3, status3.PowerLevel);
        Assert.Equal(8, status8.PowerLevel);
        Assert.True(status8.IsRunning);
    }

    [Fact]
    public void Scenario_TimeFormatting_UserExperience()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var shortTimeResult = service.StartHeating(45, 5, storage);

        Assert.Contains("45s", shortTimeResult.Message);

        service.PauseOrCancel(storage);
        service.PauseOrCancel(storage);
        var longTimeResult = service.StartHeating(105, 5, storage);

        Assert.Contains("1:45", longTimeResult.Message);
    }

    [Fact]
    public void Scenario_ErrorHandling_UserGuidance()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var invalidTimeResult = service.StartHeating(150, 5, storage);

        Assert.False(invalidTimeResult.Success);
        Assert.Contains("Time must be between 1 and 120", invalidTimeResult.Message);

        var invalidPowerResult = service.StartHeating(30, 12, storage);

        Assert.False(invalidPowerResult.Success);
        Assert.Contains("Power level must be between 1 and 10", invalidPowerResult.Message);
    }
}
