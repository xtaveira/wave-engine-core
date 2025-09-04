using Xunit;
using Microsoft.Extensions.Configuration;
using Microwave.Application;
using Microwave.Infrastructure.Services;
using Microwave.Infrastructure.Repositories;
using Microwave.Infrastructure.Logging;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Domain;

namespace Microwave.Tests.Integration;

public class HeatingEndpointsIntegrationTests : IDisposable
{
    private readonly string _authFilePath;
    private readonly string _logFilePath;
    private readonly AuthService _authService;
    private readonly MicrowaveService _microwaveService;
    private readonly IConfiguration _configuration;

    public HeatingEndpointsIntegrationTests()
    {
        _authFilePath = Path.Combine(Path.GetTempPath(), $"heating-auth-{Guid.NewGuid()}.json");
        _logFilePath = Path.Combine(Path.GetTempPath(), $"heating-log-{Guid.NewGuid()}.log");

        var configData = new Dictionary<string, string>
        {
            ["Jwt:Secret"] = "TestSecretKey123456789012345678901234567890",
            ["Encryption:Key"] = "TestEncryptionKey123"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var authRepository = new JsonAuthRepository(_authFilePath);
        var cryptographyService = new CryptographyService(_configuration);
        var exceptionLogger = new FileExceptionLogger(_logFilePath);

        _authService = new AuthService(authRepository, cryptographyService, exceptionLogger);

        var programDataPath = Path.Combine(Path.GetTempPath(), $"heating-programs-{Guid.NewGuid()}.json");
        var customProgramRepository = new JsonCustomProgramRepository(programDataPath);
        var programDisplayService = new ProgramDisplayService(customProgramRepository);

        _microwaveService = new MicrowaveService(programDisplayService);
    }

    [Fact]
    public void StartHeating_ValidParameters_Success()
    {
        var stateStorage = new InMemoryStateStorage();

        var result = _microwaveService.StartHeating(60, 5, stateStorage);

        Assert.True(result.IsSuccess);
        Assert.Contains("Aquecimento iniciado", result.Message);
    }

    [Fact]
    public void StartHeating_InvalidTime_ReturnsError()
    {
        var stateStorage = new InMemoryStateStorage();

        var result = _microwaveService.StartHeating(0, 5, stateStorage);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_PARAMETERS", result.ErrorCode);
    }

    [Fact]
    public void StartQuickHeat_Always_Success()
    {
        var stateStorage = new InMemoryStateStorage();

        var result = _microwaveService.StartQuickHeat(stateStorage);

        Assert.True(result.IsSuccess);
        Assert.Contains("30s", result.Message);
    }

    [Fact]
    public void PauseOrCancel_WhenNotHeating_ReturnsError()
    {
        var stateStorage = new InMemoryStateStorage();

        var result = _microwaveService.PauseOrCancel(stateStorage);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_RUNNING", result.ErrorCode);
    }

    [Fact]
    public void IncreaseTime_WhenHeating_Success()
    {
        var stateStorage = new InMemoryStateStorage();

        _microwaveService.StartHeating(60, 5, stateStorage);
        var result = _microwaveService.IncreaseTime(30, stateStorage);

        Assert.True(result.IsSuccess);
        Assert.Contains("1:30", result.Message);
    }

    [Fact]
    public void GetHeatingProgress_WhenStopped_ReturnsStoppedStatus()
    {
        var stateStorage = new InMemoryStateStorage();

        var status = _microwaveService.GetHeatingProgress(stateStorage);

        Assert.False(status.IsRunning);
        Assert.Equal(0, status.RemainingTime);
        Assert.Equal(0, status.PowerLevel);
    }

    [Fact]
    public void GetHeatingProgress_WhenHeating_ReturnsHeatingStatus()
    {
        var stateStorage = new InMemoryStateStorage();

        _microwaveService.StartHeating(120, 8, stateStorage);
        var status = _microwaveService.GetHeatingProgress(stateStorage);

        Assert.True(status.IsRunning);
        Assert.True(status.RemainingTime > 0);
        Assert.Equal(8, status.PowerLevel);
    }

    [Fact]
    public void GetPredefinedPrograms_Always_ReturnsPrograms()
    {
        var programs = _microwaveService.GetPredefinedPrograms();

        Assert.NotEmpty(programs);
        Assert.Contains(programs, p => p.Name == "Pipoca");
        Assert.Contains(programs, p => p.Name == "Leite");
        Assert.Contains(programs, p => p.Name == "Carnes de boi");
        Assert.Contains(programs, p => p.Name == "Frango");
        Assert.Contains(programs, p => p.Name == "Feijão");
    }

    [Fact]
    public void StartPredefinedProgram_ValidProgram_Success()
    {
        var stateStorage = new InMemoryStateStorage();

        var result = _microwaveService.StartPredefinedProgram("Pipoca", stateStorage);

        Assert.True(result.IsSuccess);
        Assert.Contains("Pipoca", result.Message);
    }

    [Fact]
    public void StartPredefinedProgram_InvalidProgram_ReturnsError()
    {
        var stateStorage = new InMemoryStateStorage();

        var result = _microwaveService.StartPredefinedProgram("ProgramaInexistente", stateStorage);

        Assert.False(result.IsSuccess);
        Assert.Equal("PROGRAM_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public void HeatingWorkflow_StartPauseResume_Success()
    {
        var stateStorage = new InMemoryStateStorage();

        var startResult = _microwaveService.StartHeating(120, 7, stateStorage);
        Assert.True(startResult.IsSuccess);

        var pauseResult = _microwaveService.PauseOrCancel(stateStorage);
        Assert.True(pauseResult.IsSuccess);

        var resumeResult = _microwaveService.StartHeating(60, 5, stateStorage);
        Assert.True(resumeResult.IsSuccess);
        Assert.Contains("retomado", resumeResult.Message);
    }

    public void Dispose()
    {
        if (File.Exists(_authFilePath))
        {
            File.Delete(_authFilePath);
        }

        if (File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }
    }
}

public class InMemoryStateStorage : IStateStorage
{
    private readonly Dictionary<string, object> _storage = new();

    public string? GetString(string key)
    {
        return _storage.TryGetValue(key, out var value) ? value as string : null;
    }

    public void SetString(string key, string value)
    {
        _storage[key] = value;
    }

    public int? GetInt32(string key)
    {
        return _storage.TryGetValue(key, out var value) ? value as int? : null;
    }

    public void SetInt32(string key, int value)
    {
        _storage[key] = value;
    }

    public void Remove(string key)
    {
        _storage.Remove(key);
    }
}
