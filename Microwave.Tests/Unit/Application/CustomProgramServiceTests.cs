using Microwave.Application;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Moq;

namespace Microwave.Tests.Unit.Application;

public class CustomProgramServiceTests
{
    private readonly Mock<ICustomProgramRepository> _mockRepository;
    private readonly Mock<IProgramDisplayService> _mockProgramDisplayService;
    private readonly CustomProgramService _service;

    public CustomProgramServiceTests()
    {
        _mockRepository = new Mock<ICustomProgramRepository>();
        _mockProgramDisplayService = new Mock<IProgramDisplayService>();
        _service = new CustomProgramService(_mockRepository.Object, _mockProgramDisplayService.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidProgram_ReturnsSuccess()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P', "Aquecer pizza");
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _service.CreateAsync(program);

        Assert.True(result.IsSuccess);
        Assert.Equal(program, result.Data);
        Assert.Contains("sucesso", result.Message);
        _mockRepository.Verify(x => x.CreateAsync(program), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidProgram_ReturnsValidationError()
    {
        var program = new CustomProgram("", "", 0, 0, '\0');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('\0')).ReturnsAsync(true);

        var result = await _service.CreateAsync(program);

        Assert.False(result.IsSuccess);
        Assert.Equal("VALIDATION_FAILED", result.ErrorCode);
        Assert.Contains("Nome do programa é obrigatório", result.Message);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<CustomProgram>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrowsException_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);
        _mockRepository.Setup(x => x.CreateAsync(program)).ThrowsAsync(new Exception("Database error"));

        var result = await _service.CreateAsync(program);

        Assert.False(result.IsSuccess);
        Assert.Equal("CREATION_FAILED", result.ErrorCode);
        Assert.Contains("Database error", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_ValidProgram_ReturnsSuccess()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = Guid.NewGuid() };
        _mockRepository.Setup(x => x.GetByIdAsync(program.Id)).ReturnsAsync(program);
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P', program.Id.ToString())).ReturnsAsync(true);

        var result = await _service.UpdateAsync(program);

        Assert.True(result.IsSuccess);
        Assert.Equal(program, result.Data);
        Assert.Contains("atualizado com sucesso", result.Message);
        _mockRepository.Verify(x => x.UpdateAsync(program), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProgramNotFound_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = Guid.NewGuid() };
        _mockRepository.Setup(x => x.GetByIdAsync(program.Id)).ReturnsAsync((CustomProgram?)null);

        var result = await _service.UpdateAsync(program);

        Assert.False(result.IsSuccess);
        Assert.Equal("PROGRAM_NOT_FOUND", result.ErrorCode);
        Assert.Contains("não encontrado", result.Message);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<CustomProgram>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidationFails_ReturnsValidationError()
    {
        var program = new CustomProgram("", "", 0, 0, '\0') { Id = Guid.NewGuid() };
        _mockRepository.Setup(x => x.GetByIdAsync(program.Id)).ReturnsAsync(program);

        var result = await _service.UpdateAsync(program);

        Assert.False(result.IsSuccess);
        Assert.Equal("VALIDATION_FAILED", result.ErrorCode);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<CustomProgram>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProgram_ReturnsSuccess()
    {
        var programId = Guid.NewGuid();
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = programId };
        _mockRepository.Setup(x => x.GetByIdAsync(programId)).ReturnsAsync(program);

        var result = await _service.DeleteAsync(programId);

        Assert.True(result.IsSuccess);
        Assert.Contains("deletado com sucesso", result.Message);
        _mockRepository.Verify(x => x.DeleteAsync(programId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProgramNotFound_ReturnsError()
    {
        var programId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(programId)).ReturnsAsync((CustomProgram?)null);

        var result = await _service.DeleteAsync(programId);

        Assert.False(result.IsSuccess);
        Assert.Equal("PROGRAM_NOT_FOUND", result.ErrorCode);
        Assert.Contains("não encontrado", result.Message);
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryThrowsException_ReturnsError()
    {
        var programId = Guid.NewGuid();
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = programId };
        _mockRepository.Setup(x => x.GetByIdAsync(programId)).ReturnsAsync(program);
        _mockRepository.Setup(x => x.DeleteAsync(programId)).ThrowsAsync(new Exception("Database error"));

        var result = await _service.DeleteAsync(programId);

        Assert.False(result.IsSuccess);
        Assert.Equal("DELETE_FAILED", result.ErrorCode);
        Assert.Contains("Database error", result.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProgram_ReturnsProgram()
    {
        var programId = Guid.NewGuid();
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = programId };
        _mockRepository.Setup(x => x.GetByIdAsync(programId)).ReturnsAsync(program);

        var result = await _service.GetByIdAsync(programId);

        Assert.NotNull(result);
        Assert.Equal(program, result);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProgram_ReturnsNull()
    {
        var programId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(programId)).ReturnsAsync((CustomProgram?)null);

        var result = await _service.GetByIdAsync(programId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithPrograms_ReturnsAllPrograms()
    {
        var programs = new List<CustomProgram>
        {
            new("Pizza", "Pizza", 8, 120, 'P'),
            new("Lasanha", "Lasanha", 6, 180, 'L')
        };
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(programs);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(programs[0], result);
        Assert.Contains(programs[1], result);
    }

    [Fact]
    public async Task ExistsCharacterAsync_WithExcludeId_CallsRepositoryWithExcludeId()
    {
        var excludeId = Guid.NewGuid();
        _mockRepository.Setup(x => x.ExistsCharacterAsync('P', excludeId)).ReturnsAsync(false);

        var result = await _service.ExistsCharacterAsync('P', excludeId);

        Assert.False(result);
        _mockRepository.Verify(x => x.ExistsCharacterAsync('P', excludeId), Times.Once);
    }

    [Fact]
    public async Task ExistsCharacterAsync_WithoutExcludeId_CallsRepositoryWithoutExcludeId()
    {
        _mockRepository.Setup(x => x.ExistsCharacterAsync('P')).ReturnsAsync(true);

        var result = await _service.ExistsCharacterAsync('P');

        Assert.True(result);
        _mockRepository.Verify(x => x.ExistsCharacterAsync('P'), Times.Once);
    }
}
