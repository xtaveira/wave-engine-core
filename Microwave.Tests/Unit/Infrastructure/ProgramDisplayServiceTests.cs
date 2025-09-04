using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Infrastructure.Services;
using Moq;

namespace Microwave.Tests.Unit.Infrastructure;

public class ProgramDisplayServiceTests
{
    private readonly Mock<ICustomProgramRepository> _mockRepository;
    private readonly ProgramDisplayService _service;

    public ProgramDisplayServiceTests()
    {
        _mockRepository = new Mock<ICustomProgramRepository>();
        _service = new ProgramDisplayService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllProgramsAsync_NoProgramsInRepository_ReturnsPredefinedOnly()
    {
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CustomProgram>());

        var result = await _service.GetAllProgramsAsync();

        Assert.Equal(5, result.Count());
        Assert.All(result, p => Assert.False(p.IsCustom));
        Assert.Contains(result, p => p.Name == "Pipoca");
        Assert.Contains(result, p => p.Name == "Leite");
        Assert.Contains(result, p => p.Name == "Carnes de boi");
        Assert.Contains(result, p => p.Name == "Frango");
        Assert.Contains(result, p => p.Name == "Feijão");
    }

    [Fact]
    public async Task GetAllProgramsAsync_WithCustomPrograms_ReturnsBothTypes()
    {
        var customPrograms = new List<CustomProgram>
        {
            new("Pizza", "Pizza", 8, 120, 'P', "Aquecer pizza"),
            new("Lasanha", "Lasanha", 6, 180, 'L', "Aquecer lasanha")
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(customPrograms);

        var result = await _service.GetAllProgramsAsync();

        Assert.Equal(7, result.Count());
        Assert.Equal(5, result.Count(p => !p.IsCustom));
        Assert.Equal(2, result.Count(p => p.IsCustom));
        Assert.Contains(result, p => p.Name == "Pizza" && p.IsCustom);
        Assert.Contains(result, p => p.Name == "Lasanha" && p.IsCustom);
    }

    [Fact]
    public async Task GetProgramByIdAsync_PredefinedProgram_ReturnsProgram()
    {
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CustomProgram>());

        var result = await _service.GetProgramByIdAsync("pipoca");

        Assert.NotNull(result);
        Assert.Equal("Pipoca", result.Name);
        Assert.False(result.IsCustom);
    }

    [Fact]
    public async Task GetProgramByIdAsync_CustomProgram_ReturnsProgram()
    {
        var customId = Guid.NewGuid();
        var customPrograms = new List<CustomProgram>
        {
            new("Pizza", "Pizza", 8, 120, 'P', "Aquecer pizza") { Id = customId }
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(customPrograms);

        var result = await _service.GetProgramByIdAsync(customId.ToString());

        Assert.NotNull(result);
        Assert.Equal("Pizza", result.Name);
        Assert.True(result.IsCustom);
    }

    [Fact]
    public async Task GetProgramByIdAsync_NonExistingProgram_ReturnsNull()
    {
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CustomProgram>());

        var result = await _service.GetProgramByIdAsync("non-existing");

        Assert.Null(result);
    }

    [Fact]
    public async Task IsCharacterUniqueAsync_PredefinedCharacter_ReturnsFalse()
    {
        _mockRepository.Setup(x => x.ExistsCharacterAsync('∩')).ReturnsAsync(false);

        var result = await _service.IsCharacterUniqueAsync('∩');

        Assert.False(result);
    }

    [Fact]
    public async Task IsCharacterUniqueAsync_CustomCharacterExists_ReturnsFalse()
    {
        _mockRepository.Setup(x => x.ExistsCharacterAsync('P')).ReturnsAsync(true);

        var result = await _service.IsCharacterUniqueAsync('P');

        Assert.False(result);
    }

    [Fact]
    public async Task IsCharacterUniqueAsync_UniqueCharacter_ReturnsTrue()
    {
        _mockRepository.Setup(x => x.ExistsCharacterAsync('X')).ReturnsAsync(false);

        var result = await _service.IsCharacterUniqueAsync('X');

        Assert.True(result);
    }

    [Fact]
    public async Task IsCharacterUniqueAsync_WithExclusion_CustomProgram_ReturnsTrue()
    {
        var excludeId = Guid.NewGuid().ToString();
        _mockRepository.Setup(x => x.ExistsCharacterAsync('P', It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _service.IsCharacterUniqueAsync('P', excludeId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsCharacterUniqueAsync_WithExclusion_PredefinedCharacter_ReturnsFalse()
    {
        var excludeId = "pipoca";

        var result = await _service.IsCharacterUniqueAsync('∩', excludeId);

        Assert.False(result);
    }

    [Fact]
    public async Task GetUsedCharactersAsync_WithCustomPrograms_ReturnsAllUsedCharacters()
    {
        var customPrograms = new List<CustomProgram>
        {
            new("Pizza", "Pizza", 8, 120, 'P'),
            new("Lasanha", "Lasanha", 6, 180, 'L')
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(customPrograms);

        var result = await _service.GetUsedCharactersAsync();

        var usedChars = result.ToList();
        Assert.Contains('∩', usedChars);
        Assert.Contains('∿', usedChars);
        Assert.Contains('≡', usedChars);
        Assert.Contains('∴', usedChars);
        Assert.Contains('◊', usedChars);
        Assert.Contains('P', usedChars);
        Assert.Contains('L', usedChars);
        Assert.Contains('.', usedChars);
        Assert.Equal(8, usedChars.Count);
    }

    [Fact]
    public async Task GetPredefinedProgramsAsync_ReturnsAllPredefinedPrograms()
    {
        var result = await _service.GetPredefinedProgramsAsync();

        Assert.Equal(5, result.Count());
        Assert.All(result, p => Assert.False(p.IsCustom));
        Assert.Contains(result, p => p.Name == "Pipoca" && p.Character == '∩');
        Assert.Contains(result, p => p.Name == "Leite" && p.Character == '∿');
        Assert.Contains(result, p => p.Name == "Carnes de boi" && p.Character == '≡');
        Assert.Contains(result, p => p.Name == "Frango" && p.Character == '∴');
        Assert.Contains(result, p => p.Name == "Feijão" && p.Character == '◊');
    }

    [Fact]
    public async Task GetCustomProgramsAsync_WithPrograms_ReturnsAllCustomPrograms()
    {
        var customPrograms = new List<CustomProgram>
        {
            new("Pizza", "Pizza", 8, 120, 'P'),
            new("Lasanha", "Lasanha", 6, 180, 'L')
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(customPrograms);

        var result = await _service.GetCustomProgramsAsync();

        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.True(p.IsCustom));
        Assert.Contains(result, p => p.Name == "Pizza" && p.Character == 'P');
        Assert.Contains(result, p => p.Name == "Lasanha" && p.Character == 'L');
    }

    [Fact]
    public async Task GetCustomProgramsAsync_EmptyRepository_ReturnsEmptyList()
    {
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CustomProgram>());

        var result = await _service.GetCustomProgramsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ProgramDisplayInfo_CustomProgram_HasCorrectProperties()
    {
        var customProgram = new CustomProgram("Pizza Especial", "Pizza", 8, 150, 'P', "Aquecer bem")
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CustomProgram> { customProgram });

        var result = await _service.GetCustomProgramsAsync();
        var program = result.First();

        Assert.Equal("Pizza Especial", program.Name);
        Assert.Equal("Pizza", program.Food);
        Assert.Equal(8, program.PowerLevel);
        Assert.Equal(150, program.TimeInSeconds);
        Assert.Equal('P', program.Character);
        Assert.Equal("Aquecer bem", program.Instructions);
        Assert.True(program.IsCustom);
        Assert.Equal("Pizza Especial (Personalizado)", program.DisplayName);
        Assert.Equal("2:30", program.TimeFormatted);
        Assert.Equal("custom-program", program.CssClass);
        Assert.Equal("italic", program.FontStyle);
    }

    [Fact]
    public async Task ProgramDisplayInfo_PredefinedProgram_HasCorrectProperties()
    {
        var result = await _service.GetPredefinedProgramsAsync();
        var pipocaProgram = result.First(p => p.Name == "Pipoca");

        Assert.Equal("Pipoca", pipocaProgram.Name);
        Assert.Equal("Pipoca", pipocaProgram.Food);
        Assert.Equal(7, pipocaProgram.PowerLevel);
        Assert.Equal(180, pipocaProgram.TimeInSeconds);
        Assert.Equal('∩', pipocaProgram.Character);
        Assert.False(pipocaProgram.IsCustom);
        Assert.Equal("Pipoca", pipocaProgram.DisplayName);
        Assert.Equal("3:00", pipocaProgram.TimeFormatted);
        Assert.Equal("predefined-program", pipocaProgram.CssClass);
        Assert.Equal("normal", pipocaProgram.FontStyle);
    }
}
