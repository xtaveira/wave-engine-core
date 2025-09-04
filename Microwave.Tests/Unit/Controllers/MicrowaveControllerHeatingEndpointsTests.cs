using Xunit;
using Moq;
using Microwave.Domain;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Application;

namespace Microwave.Tests.Unit.Controllers;

public class MicrowaveControllerHeatingEndpointsTests
{
    private readonly Mock<IMicrowaveService> _mockMicrowaveService;
    private readonly Mock<CustomProgramService> _mockCustomProgramService;
    private readonly Mock<IProgramDisplayService> _mockProgramDisplayService;

    public MicrowaveControllerHeatingEndpointsTests()
    {
        _mockMicrowaveService = new Mock<IMicrowaveService>();
        _mockCustomProgramService = new Mock<CustomProgramService>();
        _mockProgramDisplayService = new Mock<IProgramDisplayService>();
    }

    [Fact]
    public void StartHeating_ValidRequest_Success()
    {
        var request = new StartHeatingRequest { TimeInSeconds = 60, PowerLevel = 5 };
        var expectedResult = OperationResult.CreateSuccess("Aquecimento iniciado");

        _mockMicrowaveService.Setup(x => x.StartHeating(60, 5, It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.True(expectedResult.IsSuccess);
        Assert.Equal("Aquecimento iniciado", expectedResult.Message);
    }

    [Fact]
    public void StartHeating_InvalidRequest_ReturnsError()
    {
        var request = new StartHeatingRequest { TimeInSeconds = 60, PowerLevel = 5 };
        var expectedResult = OperationResult.CreateError("Erro", "INVALID_PARAMETERS");

        _mockMicrowaveService.Setup(x => x.StartHeating(60, 5, It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.False(expectedResult.IsSuccess);
        Assert.Equal("INVALID_PARAMETERS", expectedResult.ErrorCode);
    }

    [Fact]
    public void StartQuickHeating_ValidRequest_Success()
    {
        var expectedResult = OperationResult.CreateSuccess("Início rápido iniciado");

        _mockMicrowaveService.Setup(x => x.StartQuickHeat(It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.True(expectedResult.IsSuccess);
        Assert.Equal("Início rápido iniciado", expectedResult.Message);
    }

    [Fact]
    public void PauseHeating_ValidRequest_Success()
    {
        var expectedResult = OperationResult.CreateSuccess("Aquecimento pausado");

        _mockMicrowaveService.Setup(x => x.PauseOrCancel(It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.True(expectedResult.IsSuccess);
        Assert.Equal("Aquecimento pausado", expectedResult.Message);
    }

    [Fact]
    public void AddTime_ValidRequest_Success()
    {
        var request = new AddTimeRequest { AdditionalSeconds = 30 };
        var expectedResult = OperationResult.CreateSuccess("Tempo adicionado");

        _mockMicrowaveService.Setup(x => x.IncreaseTime(30, It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.True(expectedResult.IsSuccess);
        Assert.Equal("Tempo adicionado", expectedResult.Message);
    }

    [Fact]
    public void GetHeatingStatus_ReturnsValidStatus()
    {
        var expectedStatus = new MicrowaveStatus
        {
            IsRunning = true,
            RemainingTime = 120,
            PowerLevel = 7,
            Progress = 50,
            StatusMessage = "Aquecendo..."
        };

        _mockMicrowaveService.Setup(x => x.GetHeatingProgress(It.IsAny<IStateStorage>()))
            .Returns(expectedStatus);

        Assert.True(expectedStatus.IsRunning);
        Assert.Equal(120, expectedStatus.RemainingTime);
        Assert.Equal(7, expectedStatus.PowerLevel);
        Assert.Equal(50, expectedStatus.Progress);
    }

    [Fact]
    public void GetPredefinedPrograms_ReturnsPrograms()
    {
        var expectedPrograms = new List<PredefinedProgram>
        {
            new PredefinedProgram
            {
                Name = "Pipoca",
                Food = "Pipoca",
                TimeInSeconds = 180,
                PowerLevel = 7,
                HeatingChar = "∩"
            }
        };

        _mockMicrowaveService.Setup(x => x.GetPredefinedPrograms())
            .Returns(expectedPrograms);

        var programs = _mockMicrowaveService.Object.GetPredefinedPrograms();
        Assert.Single(programs);
        Assert.Equal("Pipoca", programs.First().Name);
    }

    [Fact]
    public void StartPredefinedProgram_ValidName_Success()
    {
        var programName = "Pipoca";
        var expectedResult = OperationResult.CreateSuccess("Programa iniciado");

        _mockMicrowaveService.Setup(x => x.StartPredefinedProgram(programName, It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.True(expectedResult.IsSuccess);
        Assert.Equal("Programa iniciado", expectedResult.Message);
    }

    [Fact]
    public void StartPredefinedProgram_InvalidProgram_ReturnsError()
    {
        var programName = "Inexistente";
        var expectedResult = OperationResult.CreateError("Programa não encontrado", "PROGRAM_NOT_FOUND");

        _mockMicrowaveService.Setup(x => x.StartPredefinedProgram(programName, It.IsAny<IStateStorage>()))
            .Returns(expectedResult);

        Assert.False(expectedResult.IsSuccess);
        Assert.Equal("PROGRAM_NOT_FOUND", expectedResult.ErrorCode);
    }

    [Fact]
    public void HeatingStatusResponse_PropertiesWork()
    {
        var response = new HeatingStatusResponse
        {
            IsRunning = true,
            RemainingTime = 180,
            PowerLevel = 8,
            Progress = 25,
            CurrentState = "HEATING",
            HeatingChar = "∩",
            CurrentProgram = "Pipoca",
            ProgressDisplay = "Aquecendo pipoca...",
            StartTime = DateTime.Now
        };

        Assert.True(response.IsRunning);
        Assert.Equal(180, response.RemainingTime);
        Assert.Equal(8, response.PowerLevel);
        Assert.Equal("HEATING", response.CurrentState);
        Assert.Equal("∩", response.HeatingChar);
    }

    [Fact]
    public void StartHeatingRequest_PropertiesWork()
    {
        var request = new StartHeatingRequest
        {
            TimeInSeconds = 120,
            PowerLevel = 6
        };

        Assert.Equal(120, request.TimeInSeconds);
        Assert.Equal(6, request.PowerLevel);
    }

    [Fact]
    public void AddTimeRequest_PropertiesWork()
    {
        var request = new AddTimeRequest
        {
            AdditionalSeconds = 45
        };

        Assert.Equal(45, request.AdditionalSeconds);
    }
}
