using Microwave.Domain;
using Microwave.Application;
using Microwave.Domain.Validators;
using Microwave.Domain.Factories;

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
        Assert.Equal(100, status.RemainingTime);
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


    [Fact]
    public void TestGetPredefinedPrograms_ShouldReturn5Programs()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();

        Assert.Equal(5, programs.Count());

        var programNames = programs.Select(p => p.Name).ToList();
        Assert.Contains("Pipoca", programNames);
        Assert.Contains("Leite", programNames);
        Assert.Contains("Carnes de boi", programNames);
        Assert.Contains("Frango", programNames);
        Assert.Contains("Feijão", programNames);
    }

    [Fact]
    public void TestPipocaProgram_ShouldHaveCorrectProperties()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var pipoca = programs.FirstOrDefault(p => p.Name == "Pipoca");

        Assert.NotNull(pipoca);
        Assert.Equal("Pipoca", pipoca.Name);
        Assert.Equal("Pipoca (de micro-ondas)", pipoca.Food);
        Assert.Equal(180, pipoca.TimeInSeconds);
        Assert.Equal(7, pipoca.PowerLevel);
        Assert.NotEqual(".", pipoca.HeatingChar);
        Assert.Contains("barulho de estouros", pipoca.Instructions);
    }

    [Fact]
    public void TestLeiteProgram_ShouldHaveCorrectProperties()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var leite = programs.FirstOrDefault(p => p.Name == "Leite");

        Assert.NotNull(leite);
        Assert.Equal("Leite", leite.Name);
        Assert.Equal("Leite", leite.Food);
        Assert.Equal(300, leite.TimeInSeconds);
        Assert.Equal(5, leite.PowerLevel);
        Assert.NotEqual(".", leite.HeatingChar);
        Assert.Contains("choque térmico", leite.Instructions);
    }

    [Fact]
    public void TestCarnesBoiProgram_ShouldHaveCorrectProperties()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var carnes = programs.FirstOrDefault(p => p.Name == "Carnes de boi");

        Assert.NotNull(carnes);
        Assert.Equal("Carnes de boi", carnes.Name);
        Assert.Equal("Carne em pedaço ou fatias", carnes.Food);
        Assert.Equal(840, carnes.TimeInSeconds);
        Assert.Equal(4, carnes.PowerLevel);
        Assert.NotEqual(".", carnes.HeatingChar);
        Assert.Contains("vire o conteúdo", carnes.Instructions);
    }

    [Fact]
    public void TestFrangoProgram_ShouldHaveCorrectProperties()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var frango = programs.FirstOrDefault(p => p.Name == "Frango");

        Assert.NotNull(frango);
        Assert.Equal("Frango", frango.Name);
        Assert.Equal("Frango (qualquer corte)", frango.Food);
        Assert.Equal(480, frango.TimeInSeconds);
        Assert.Equal(7, frango.PowerLevel);
        Assert.NotEqual(".", frango.HeatingChar);
        Assert.Contains("vire o conteúdo", frango.Instructions);
    }

    [Fact]
    public void TestFeijaoProgram_ShouldHaveCorrectProperties()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var feijao = programs.FirstOrDefault(p => p.Name == "Feijão");

        Assert.NotNull(feijao);
        Assert.Equal("Feijão", feijao.Name);
        Assert.Equal("Feijão congelado", feijao.Food);
        Assert.Equal(480, feijao.TimeInSeconds);
        Assert.Equal(9, feijao.PowerLevel);
        Assert.NotEqual(".", feijao.HeatingChar);
        Assert.Contains("recipiente destampado", feijao.Instructions);
    }

    [Fact]
    public void TestAllProgramsHaveUniqueHeatingChars()
    {
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var heatingChars = programs.Select(p => p.HeatingChar).ToList();

        Assert.Equal(heatingChars.Count, heatingChars.Distinct().Count());

        Assert.DoesNotContain(".", heatingChars);
    }

    [Fact]
    public void TestStartPredefinedProgram_ShouldUseCorrectTimeAndPower()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        var programs = service.GetPredefinedPrograms();
        var pipoca = programs.First(p => p.Name == "Pipoca");

        var result = service.StartPredefinedProgram(pipoca.Name, mockStorage);

        Console.WriteLine($"Result Success: {result.Success}");
        Console.WriteLine($"Result Message: {result.Message}");

        Assert.True(result.Success);
        Assert.Contains("Pipoca", result.Message);
        Assert.Contains("180s", result.Message);
        Assert.Contains("potência 7", result.Message);
    }
    [Fact]
    public void TestStartPredefinedProgram_ShouldUseCustomHeatingString()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartPredefinedProgram("Pipoca", mockStorage);
        var startTime = DateTime.Now.AddSeconds(-2).ToString("O");
        mockStorage.SetString("StartTime", startTime);

        var status = service.GetHeatingProgress(mockStorage);

        Assert.True(status.IsRunning);
        Assert.DoesNotContain(".", status.StatusMessage);
        var segments = status.StatusMessage.Split(' ');
        Assert.Equal(2, segments.Length);
    }

    [Fact]
    public void TestIncreaseTime_ShouldNotWorkForPredefinedPrograms()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartPredefinedProgram("Leite", mockStorage);

        var result = service.IncreaseTime(30, mockStorage);

        Assert.False(result.Success);
        Assert.Contains("programa pré-definido", result.Message.ToLower());
    }

    [Fact]
    public void TestPauseAndCancel_ShouldWorkForPredefinedPrograms()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartPredefinedProgram("Frango", mockStorage);

        var pauseResult = service.PauseOrCancel(mockStorage);
        Assert.True(pauseResult.Success);
        Assert.Contains("pausado", pauseResult.Message.ToLower());

        var cancelResult = service.PauseOrCancel(mockStorage);
        Assert.True(cancelResult.Success);
        Assert.Contains("cancelado", cancelResult.Message.ToLower());
    }

    [Fact]
    public void TestStartInvalidPredefinedProgram_ShouldReturnError()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        var result = service.StartPredefinedProgram("ProgramaInexistente", mockStorage);

        Assert.False(result.Success);
        Assert.Contains("não encontrado", result.Message.ToLower());
    }

    [Fact]
    public void TestResumePredefinedProgram_ShouldMaintainCustomHeatingChar()
    {
        var mockStorage = new MockStateStorage();
        var service = new MicrowaveService();

        service.StartPredefinedProgram("Feijão", mockStorage);

        var startTime = DateTime.Now.AddSeconds(-30).ToString("O");
        mockStorage.SetString("StartTime", startTime);
        service.PauseOrCancel(mockStorage);

        var resumeResult = service.StartHeating(0, 0, mockStorage);
        Assert.True(resumeResult.Success);

        var startTimeResume = DateTime.Now.AddSeconds(-10).ToString("O");
        mockStorage.SetString("StartTime", startTimeResume);

        var status = service.GetHeatingProgress(mockStorage);
        Assert.DoesNotContain(".", status.StatusMessage);
    }


    [Fact]
    public void TestManualTimeValidator_ShouldAllowValidRange()
    {
        var validator = new ManualTimeValidator();

        validator.Validate(1);
        validator.Validate(60);
        validator.Validate(120);

        Assert.True(true);
    }

    [Fact]
    public void TestManualTimeValidator_ShouldRejectInvalidRange()
    {
        var validator = new ManualTimeValidator();

        Assert.Throws<ArgumentException>(() => validator.Validate(0));
        Assert.Throws<ArgumentException>(() => validator.Validate(121));
        Assert.Throws<ArgumentException>(() => validator.Validate(300));
    }

    [Fact]
    public void TestPredefinedTimeValidator_ShouldAllowValidRange()
    {
        var validator = new PredefinedTimeValidator();

        validator.Validate(1);
        validator.Validate(300);
        validator.Validate(840);
        validator.Validate(1800);

        Assert.True(true);
    }

    [Fact]
    public void TestPredefinedTimeValidator_ShouldRejectInvalidRange()
    {
        var validator = new PredefinedTimeValidator();

        Assert.Throws<ArgumentException>(() => validator.Validate(0));
        Assert.Throws<ArgumentException>(() => validator.Validate(1801));
        Assert.Throws<ArgumentException>(() => validator.Validate(3600));
    }

    [Fact]
    public void TestTimeValidatorFactory_ShouldCreateCorrectValidators()
    {
        var manualValidator = TimeValidatorFactory.CreateManual();
        var predefinedValidator = TimeValidatorFactory.CreatePredefined();

        Assert.IsType<ManualTimeValidator>(manualValidator);
        Assert.IsType<PredefinedTimeValidator>(predefinedValidator);
    }

    [Fact]
    public void TestMicrowaveOven_CreateManual_ShouldUseManualValidator()
    {
        var oven1 = MicrowaveOven.CreateManual(60, 5);
        Assert.Equal(60, oven1.TimeInSeconds);

        Assert.Throws<ArgumentException>(() => MicrowaveOven.CreateManual(300, 5));
        Assert.Throws<ArgumentException>(() => MicrowaveOven.CreateManual(300, 5));
    }

    [Fact]
    public void TestMicrowaveOven_CreatePredefined_ShouldUsePredefinedValidator()
    {
        var oven1 = MicrowaveOven.CreatePredefined(300, 5);
        Assert.Equal(300, oven1.TimeInSeconds);

        var oven2 = MicrowaveOven.CreatePredefined(840, 4);
        Assert.Equal(840, oven2.TimeInSeconds);

        Assert.Throws<ArgumentException>(() => MicrowaveOven.CreatePredefined(2000, 5));
        Assert.Throws<ArgumentException>(() => MicrowaveOven.CreatePredefined(2000, 5));
    }

    [Fact]
    public void TestBackwardCompatibility_DefaultConstructor_ShouldUseManualValidator()
    {
        var oven1 = new MicrowaveOven(60, 5);
        Assert.Equal(60, oven1.TimeInSeconds);

        Assert.Throws<ArgumentException>(() => new MicrowaveOven(300, 5));
        Assert.Throws<ArgumentException>(() => new MicrowaveOven(300, 5));
    }
}
