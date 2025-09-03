using Microwave.Application;
using Microwave.Tests.Shared;

namespace Microwave.Tests.Unit.Application;

public class MicrowaveServiceTests
{
    [Fact]
    public void TestMicrowaveServiceStartHeating()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartHeating(30, 5, storage);

        Assert.True(result.Success);
        Assert.Contains("Aquecimento iniciado: 30s a potência 5", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceStartHeatingWithFormatting()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartHeating(90, 8, storage);

        Assert.True(result.Success);
        Assert.Contains("Aquecimento iniciado: 1:30 a potência 8", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceInvalidTime()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartHeating(150, 5, storage);

        Assert.False(result.Success);
        Assert.Contains("Time must be between 1 and 120", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceInvalidPower()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartHeating(30, 15, storage);

        Assert.False(result.Success);
        Assert.Contains("Power level must be between 1 and 10", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceQuickStart()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartQuickHeat(storage);

        Assert.True(result.Success);
        Assert.Contains("Aquecimento iniciado: 30s a potência 10", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceAddTime()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartHeating(30, 5, storage);

        var result = service.IncreaseTime(30, storage);

        Assert.True(result.Success);
        Assert.Contains("Tempo aumentado para 60s", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceAddTimeWithoutHeating()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.IncreaseTime(30, storage);

        Assert.False(result.Success);
        Assert.Contains("Micro-ondas não está aquecendo", result.Message);
    }

    [Fact]
    public void TestMicrowaveServiceGetStatus()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var status = service.GetHeatingProgress(storage);

        Assert.False(status.IsRunning);
        Assert.Equal(0, status.RemainingTime);
        Assert.Equal(0, status.PowerLevel);
    }

    [Fact]
    public void TestMicrowaveServicePauseAndCancel()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartHeating(30, 5, storage);

        var pauseResult = service.PauseOrCancel(storage);
        Assert.True(pauseResult.Success);
        Assert.Contains("pausado", pauseResult.Message.ToLower());

        var cancelResult = service.PauseOrCancel(storage);
        Assert.True(cancelResult.Success);
        Assert.Contains("cancelado", cancelResult.Message.ToLower());
    }

    [Fact]
    public void TestMicrowaveServicePauseHeating()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartHeating(30, 5, storage);
        service.PauseOrCancel(storage);

        var status = service.GetHeatingProgress(storage);

        Assert.False(status.IsRunning);
        Assert.Equal(5, status.PowerLevel);
        Assert.Contains("pausado", status.StatusMessage.ToLower());
    }

    [Fact]
    public void TestMicrowaveServiceResumeHeating()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        storage.SetString("MicrowaveState", "Paused");
        storage.SetString("PausedRemainingTime", "30");
        storage.SetString("CurrentOven", "{\"timeInSeconds\":60,\"powerLevel\":5}");

        var result = service.StartPredefinedProgram("Pipoca", storage);

        Assert.True(result.Success);
        Assert.Contains("Programa 'Pipoca' iniciado", result.Message);
    }

    [Fact]
    public void TestGetPredefinedPrograms_ShouldReturn5Programs()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();

        Assert.Equal(5, programs.Count());

        var pipoca = programs.First(p => p.Name == "Pipoca");
        Assert.Equal(180, pipoca.TimeInSeconds);
        Assert.Equal(7, pipoca.PowerLevel);
        Assert.Equal("∩", pipoca.HeatingChar);
    }

    [Fact]
    public void TestStartPredefinedProgram_Pipoca()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartPredefinedProgram("Pipoca", storage);

        Assert.True(result.Success);
        Assert.Contains("Programa 'Pipoca' iniciado: 180s a potência 7", result.Message);
    }

    [Fact]
    public void TestStartInvalidPredefinedProgram_ShouldReturnError()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        var result = service.StartPredefinedProgram("ProgramaInexistente", storage);

        Assert.False(result.Success);
        Assert.Contains("não encontrado", result.Message.ToLower());
    }

    [Fact]
    public void TestResumePredefinedProgram_ShouldMaintainCustomHeatingChar()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartPredefinedProgram("Leite", storage);

        storage.SetString("MicrowaveState", "Paused");
        storage.SetString("PausedRemainingTime", "250");

        var result = service.StartPredefinedProgram("Leite", storage);

        Assert.True(result.Success);
        Assert.Contains("Programa 'Leite' iniciado", result.Message);
    }

    [Fact]
    public void TestAddTimeBlockedForPredefinedPrograms()
    {
        var service = new MicrowaveService();
        var storage = new MockStateStorage();

        service.StartPredefinedProgram("Frango", storage);

        var result = service.IncreaseTime(30, storage);

        Assert.False(result.Success);
        Assert.Contains("Não é permitido aumentar tempo em p", result.Message);
    }
}
