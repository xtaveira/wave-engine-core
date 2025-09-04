using Microwave.Application;
using Microwave.Tests.Shared;

namespace Microwave.Tests.Scenarios;

public class Level2PredefinedScenarios
{
    [Fact]
    public void Scenario_PredefinedPrograms_ConvenientCooking()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        var programs = service.GetPredefinedPrograms();
        Assert.Equal(5, programs.Count());
        Assert.Contains(programs, p => p.Name == "Pipoca");
        Assert.Contains(programs, p => p.Name == "Leite");
        Assert.Contains(programs, p => p.Name == "Carnes de boi");
        Assert.Contains(programs, p => p.Name == "Frango");
        Assert.Contains(programs, p => p.Name == "Feijão");
    }

    [Fact]
    public void Scenario_PopcornProgram_QuickSnack()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        var result = service.StartPredefinedProgram("Pipoca", storage);
        Assert.True(result.Success);
        Assert.Contains("Programa 'Pipoca' iniciado: 180s a potência 7", result.Message);
        var status = service.GetHeatingProgress(storage);
        Assert.Equal(180, status.RemainingTime);
        Assert.Equal(7, status.PowerLevel);
        Assert.True(status.IsRunning);
    }

    [Fact]
    public void Scenario_MilkProgram_BeverageHeating()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        var result = service.StartPredefinedProgram("Leite", storage);
        Assert.True(result.Success);
        Assert.Contains("300s a potência 5", result.Message);

        var status = service.GetHeatingProgress(storage);
        Assert.Equal(300, status.RemainingTime);
        Assert.Equal(5, status.PowerLevel);
    }

    [Fact]
    public void Scenario_MeatPrograms_ProperCooking()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        var beefResult = service.StartPredefinedProgram("Carnes de boi", storage);
        Assert.True(beefResult.Success);
        Assert.Contains("840s a potência 4", beefResult.Message);
        service.PauseOrCancel(storage);
        service.PauseOrCancel(storage);
        var chickenResult = service.StartPredefinedProgram("Frango", storage);
        Assert.True(chickenResult.Success);
        Assert.Contains("480s a potência 7", chickenResult.Message);
    }

    [Fact]
    public void Scenario_CustomHeatingCharacters_VisualFeedback()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        service.StartPredefinedProgram("Pipoca", storage);
        var status = service.GetHeatingProgress(storage);
        Assert.True(status.IsRunning);
        Assert.Equal(180, status.RemainingTime);
    }

    [Fact]
    public void Scenario_TimeAdditionRestriction_ProgramIntegrity()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        service.StartPredefinedProgram("Feijão", storage);
        var addTimeResult = service.IncreaseTime(30, storage);
        Assert.False(addTimeResult.Success);
        Assert.Contains("não é permitido aumentar tempo", addTimeResult.Message.ToLower());
    }

    [Fact]
    public void Scenario_PredefinedProgramPauseResume_FlexibleControl()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        service.StartPredefinedProgram("Frango", storage);
        var pauseResult = service.PauseOrCancel(storage);
        Assert.True(pauseResult.Success);
        var resumeResult = service.StartHeating(0, 0, storage);
        Assert.True(resumeResult.Success);
        var status = service.GetHeatingProgress(storage);
        Assert.Equal(7, status.PowerLevel);
        Assert.True(status.IsRunning);
    }

    [Fact]
    public void Scenario_InvalidProgram_UserGuidance()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();
        var result = service.StartPredefinedProgram("Pizza", storage);
        Assert.False(result.Success);
        Assert.Contains("não encontrado", result.Message.ToLower());
    }

    [Fact]
    public void Scenario_ProgramRestart_CleanState()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartPredefinedProgram("Leite", storage);
        service.PauseOrCancel(storage);
        service.PauseOrCancel(storage);
        var result = service.StartPredefinedProgram("Pipoca", storage);
        Assert.True(result.Success);
        var status = service.GetHeatingProgress(storage);
        Assert.Equal(180, status.RemainingTime);
        Assert.Equal(7, status.PowerLevel);
    }
}
