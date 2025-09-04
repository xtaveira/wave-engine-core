using Microwave.Application;
using Microwave.Domain;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Moq;

namespace Microwave.Tests.Unit.Application;

public class MicrowaveServiceLevel3Tests
{
    private readonly Mock<IProgramDisplayService> _mockProgramDisplayService;
    private readonly Mock<IStateStorage> _mockStateStorage;
    private readonly MicrowaveService _service;

    public MicrowaveServiceLevel3Tests()
    {
        _mockProgramDisplayService = new Mock<IProgramDisplayService>();
        _mockStateStorage = new Mock<IStateStorage>();
        _service = new MicrowaveService(_mockProgramDisplayService.Object);
    }

    [Fact]
    public async Task StartCustomProgramAsync_ValidProgram_StartsHeating()
    {
        var customProgramId = Guid.NewGuid();
        var customProgram = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = customProgramId };

        var programInfo = new ProgramDisplayInfo
        {
            Id = customProgramId.ToString(),
            Name = "Pizza",
            Food = "Pizza",
            PowerLevel = 8,
            TimeInSeconds = 120,
            Character = 'P',
            Instructions = "Aquecer pizza",
            IsCustom = true,
            CreatedAt = DateTime.Now
        };

        _mockProgramDisplayService.Setup(x => x.GetProgramByIdAsync(customProgramId.ToString()))
            .ReturnsAsync(programInfo);
        _mockStateStorage.Setup(x => x.GetString("MicrowaveState")).Returns("STOPPED");

        var result = await _service.StartCustomProgramAsync(customProgramId, _mockStateStorage.Object);

        Assert.True(result.IsSuccess);
        Assert.Contains("Pizza", result.Message);
        Assert.Contains("iniciado", result.Message);
        Assert.Contains("120s", result.Message);
        Assert.Contains("potência 8", result.Message);

        _mockStateStorage.Verify(x => x.SetString("IsHeating", "true"), Times.Once);
        _mockStateStorage.Verify(x => x.SetString("MicrowaveState", "HEATING"), Times.Once);
        _mockStateStorage.Verify(x => x.SetString("CurrentProgram", $"custom-{customProgramId}"), Times.Once);
        _mockStateStorage.Verify(x => x.SetString("HeatingChar", "P"), Times.Once);
    }

    [Fact]
    public async Task StartCustomProgramAsync_ProgramNotFound_ReturnsError()
    {
        var customProgramId = Guid.NewGuid();

        _mockProgramDisplayService.Setup(x => x.GetProgramByIdAsync(customProgramId.ToString()))
            .ReturnsAsync((ProgramDisplayInfo?)null);
        _mockStateStorage.Setup(x => x.GetString("MicrowaveState")).Returns("STOPPED");

        var result = await _service.StartCustomProgramAsync(customProgramId, _mockStateStorage.Object);

        Assert.False(result.IsSuccess);
        Assert.Equal("CUSTOM_PROGRAM_NOT_FOUND", result.ErrorCode);
        Assert.Contains("não encontrado", result.Message);

        _mockStateStorage.Verify(x => x.SetString("IsHeating", "true"), Times.Never);
    }

    [Fact]
    public async Task StartCustomProgramAsync_AlreadyPaused_ResumesHeating()
    {
        var customProgramId = Guid.NewGuid();

        _mockStateStorage.Setup(x => x.GetString("MicrowaveState")).Returns("PAUSED");
        _mockStateStorage.Setup(x => x.GetString("PausedRemainingTime")).Returns("60");
        _mockStateStorage.Setup(x => x.GetString("CurrentOven")).Returns("{\"timeInSeconds\":120,\"powerLevel\":8}");

        var result = await _service.StartCustomProgramAsync(customProgramId, _mockStateStorage.Object);

        Assert.True(result.IsSuccess);
        Assert.Contains("retomado", result.Message);

        _mockStateStorage.Verify(x => x.SetString("IsHeating", "true"), Times.Once);
        _mockStateStorage.Verify(x => x.SetString("MicrowaveState", "HEATING"), Times.Once);
        _mockStateStorage.Verify(x => x.Remove("PausedRemainingTime"), Times.Once);
    }

    [Fact]
    public async Task GetAllProgramsAsync_ReturnsAllPrograms()
    {
        var expectedPrograms = new List<ProgramDisplayInfo>
        {
            new() { Name = "Pipoca", IsCustom = false },
            new() { Name = "Pizza", IsCustom = true },
            new() { Name = "Lasanha", IsCustom = true }
        };

        _mockProgramDisplayService.Setup(x => x.GetAllProgramsAsync())
            .ReturnsAsync(expectedPrograms);

        var result = await _service.GetAllProgramsAsync();

        Assert.Equal(3, result.Count());
        Assert.Contains(result, p => p.Name == "Pipoca" && !p.IsCustom);
        Assert.Contains(result, p => p.Name == "Pizza" && p.IsCustom);
        Assert.Contains(result, p => p.Name == "Lasanha" && p.IsCustom);
    }

    [Fact]
    public async Task GetCustomProgramAsync_ExistingCustomProgram_ReturnsProgram()
    {
        var customProgramId = Guid.NewGuid();
        var programInfo = new ProgramDisplayInfo
        {
            Id = customProgramId.ToString(),
            Name = "Pizza Especial",
            Food = "Pizza",
            PowerLevel = 8,
            TimeInSeconds = 150,
            Character = 'P',
            Instructions = "Aquecer bem",
            IsCustom = true,
            CreatedAt = DateTime.Now
        };

        _mockProgramDisplayService.Setup(x => x.GetProgramByIdAsync(customProgramId.ToString()))
            .ReturnsAsync(programInfo);

        var result = await _service.GetCustomProgramAsync(customProgramId);

        Assert.NotNull(result);
        Assert.Equal("Pizza Especial", result.Name);
        Assert.Equal("Pizza", result.Food);
        Assert.Equal(8, result.PowerLevel);
        Assert.Equal(150, result.TimeInSeconds);
        Assert.Equal('P', result.Character);
        Assert.Equal("Aquecer bem", result.Instructions);
        Assert.Equal(customProgramId, result.Id);
    }

    [Fact]
    public async Task GetCustomProgramAsync_NonExistingProgram_ReturnsNull()
    {
        var customProgramId = Guid.NewGuid();

        _mockProgramDisplayService.Setup(x => x.GetProgramByIdAsync(customProgramId.ToString()))
            .ReturnsAsync((ProgramDisplayInfo?)null);

        var result = await _service.GetCustomProgramAsync(customProgramId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomProgramAsync_PredefinedProgram_ReturnsNull()
    {
        var programInfo = new ProgramDisplayInfo
        {
            Id = "pipoca",
            Name = "Pipoca",
            Food = "Pipoca",
            PowerLevel = 7,
            TimeInSeconds = 180,
            Character = '∩',
            IsCustom = false
        };

        _mockProgramDisplayService.Setup(x => x.GetProgramByIdAsync("pipoca"))
            .ReturnsAsync(programInfo);

        var result = await _service.GetCustomProgramAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task StartCustomProgramAsync_InvalidTimeOrPower_ReturnsError()
    {
        var customProgramId = Guid.NewGuid();
        var programInfo = new ProgramDisplayInfo
        {
            Id = customProgramId.ToString(),
            Name = "Invalid Program",
            Food = "Food",
            PowerLevel = 15,
            TimeInSeconds = 8000,
            Character = 'I',
            IsCustom = true,
            CreatedAt = DateTime.Now
        };

        _mockProgramDisplayService.Setup(x => x.GetProgramByIdAsync(customProgramId.ToString()))
            .ReturnsAsync(programInfo);
        _mockStateStorage.Setup(x => x.GetString("MicrowaveState")).Returns("STOPPED");

        var result = await _service.StartCustomProgramAsync(customProgramId, _mockStateStorage.Object);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_PARAMETERS", result.ErrorCode);
    }
}
