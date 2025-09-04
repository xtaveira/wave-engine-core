using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Domain.Validators;
using Moq;

namespace Microwave.Tests.Unit.Domain;

public class CustomProgramValidatorTests
{
    private readonly Mock<IProgramDisplayService> _mockProgramDisplayService;
    private readonly CustomProgramValidator _validator;

    public CustomProgramValidatorTests()
    {
        _mockProgramDisplayService = new Mock<IProgramDisplayService>();
        _validator = new CustomProgramValidator(_mockProgramDisplayService.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidProgram_ReturnsValid()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P', "Aquecer pizza fria");
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_EmptyName_ReturnsError()
    {
        var program = new CustomProgram("", "Pizza", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Nome do programa é obrigatório", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_NameTooShort_ReturnsError()
    {
        var program = new CustomProgram("P", "Pizza", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Nome deve ter entre 2 e 50 caracteres", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_NameTooLong_ReturnsError()
    {
        var longName = new string('A', 51);
        var program = new CustomProgram(longName, "Pizza", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Nome deve ter entre 2 e 50 caracteres", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_EmptyFood_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Nome do alimento é obrigatório", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_PowerLevelTooLow_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 0, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Potência deve estar entre 1 e 10", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_PowerLevelTooHigh_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 11, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Potência deve estar entre 1 e 10", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_TimeTooLow_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 0, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Tempo deve estar entre 1 e 7200 segundos (2 horas)", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_TimeTooHigh_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 7201, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Tempo deve estar entre 1 e 7200 segundos (2 horas)", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_EmptyCharacter_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, '\0');

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Caractere de aquecimento é obrigatório", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_ForbiddenCharacterDot_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, '.');

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Caractere não pode ser espaço em branco, tab, quebra de linha ou ponto", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WhitespaceCharacter_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, ' ');

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Caractere não pode ser espaço em branco, tab, quebra de linha ou ponto", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_DuplicateCharacter_ReturnsError()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(false);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Caractere 'P' já está sendo usado por outro programa", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_DuplicateCharacterWithExclusion_AllowsUpdate()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P', program.Id.ToString())).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program, isUpdate: true);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_InstructionsTooLong_ReturnsError()
    {
        var longInstructions = new string('A', 201);
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P', longInstructions);
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Instruções não podem exceder 200 caracteres", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_OptionalInstructionsEmpty_IsValid()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P', "");
        _mockProgramDisplayService.Setup(x => x.IsCharacterUniqueAsync('P')).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(program);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        var program = new CustomProgram("", "", 0, 0, '\0');

        var result = await _validator.ValidateAsync(program);

        Assert.False(result.IsValid);
        Assert.Contains("Nome do programa é obrigatório", result.Errors);
        Assert.Contains("Nome do alimento é obrigatório", result.Errors);
        Assert.Contains("Potência deve estar entre 1 e 10", result.Errors);
        Assert.Contains("Tempo deve estar entre 1 e 7200 segundos (2 horas)", result.Errors);
        Assert.Contains("Caractere de aquecimento é obrigatório", result.Errors);
        Assert.True(result.Errors.Count >= 5);
    }
}
