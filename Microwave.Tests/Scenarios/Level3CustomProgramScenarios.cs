using Microwave.Domain.DTOs;
using Microwave.Domain.Validators;
using Microwave.Infrastructure.Repositories;
using Microwave.Infrastructure.Services;

namespace Microwave.Tests.Scenarios;

public class Level3CustomProgramScenarios : IDisposable
{
    private readonly string _testFilePath;
    private readonly JsonCustomProgramRepository _repository;
    private readonly ProgramDisplayService _programDisplayService;
    private readonly CustomProgramValidator _validator;

    public Level3CustomProgramScenarios()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"scenario-test-{Guid.NewGuid()}.json");
        _repository = new JsonCustomProgramRepository(_testFilePath);
        _programDisplayService = new ProgramDisplayService(_repository);
        _validator = new CustomProgramValidator(_programDisplayService);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public async Task Scenario_UserCreatesFirstCustomProgram_Success()
    {
        var program = new CustomProgram("Minha Pizza", "Pizza Caseira", 8, 180, 'M', "Aquecer minha pizza especial");

        Assert.True(await _programDisplayService.IsCharacterUniqueAsync('M'));

        var validationResult = await _validator.ValidateAsync(program);
        Assert.True(validationResult.IsValid, string.Join(", ", validationResult.Errors));

        var created = await _repository.CreateAsync(program);
        Assert.NotEqual(Guid.Empty, created.Id);

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();
        Assert.Equal(6, allPrograms.Count());

        var customProgram = allPrograms.FirstOrDefault(p => p.IsCustom);
        Assert.NotNull(customProgram);
        Assert.Equal("Minha Pizza", customProgram.Name);
        Assert.Equal("italic", customProgram.FontStyle);
        Assert.Equal("Minha Pizza (Personalizado)", customProgram.DisplayName);
    }

    [Fact]
    public async Task Scenario_UserTriesToUseReservedCharacter_Fails()
    {
        var program = new CustomProgram("Teste", "Alimento", 5, 60, '.');

        var validationResult = await _validator.ValidateAsync(program);

        Assert.False(validationResult.IsValid);
        Assert.Contains("Caractere não pode ser espaço em branco, tab, quebra de linha ou ponto", validationResult.Errors);
    }

    [Fact]
    public async Task Scenario_UserTriesToUsePredefinedCharacter_Fails()
    {
        var program = new CustomProgram("Minha Pipoca", "Pipoca Especial", 7, 200, '∩');

        var validationResult = await _validator.ValidateAsync(program);

        Assert.False(validationResult.IsValid);
        Assert.Contains("Caractere '∩' já está sendo usado por outro programa", validationResult.Errors);
    }

    [Fact]
    public async Task Scenario_UserCreatesMultipleCustomPrograms_AllDisplayedCorrectly()
    {
        var programs = new[]
        {
            new CustomProgram("Pizza Margherita", "Pizza", 8, 180, 'M'),
            new CustomProgram("Hambúrguer Artesanal", "Hambúrguer", 7, 120, 'H'),
            new CustomProgram("Lasanha da Vovó", "Lasanha", 6, 300, 'L')
        };

        foreach (var program in programs)
        {
            var validationResult = await _validator.ValidateAsync(program);
            Assert.True(validationResult.IsValid);
            await _repository.CreateAsync(program);
        }

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();
        Assert.Equal(8, allPrograms.Count());
        Assert.Equal(5, allPrograms.Count(p => !p.IsCustom));
        Assert.Equal(3, allPrograms.Count(p => p.IsCustom));

        var customPrograms = allPrograms.Where(p => p.IsCustom).ToList();
        Assert.All(customPrograms, p => Assert.Equal("italic", p.FontStyle));
        Assert.All(customPrograms, p => Assert.Contains("(Personalizado)", p.DisplayName));
    }

    [Fact]
    public async Task Scenario_UserEditsCustomProgram_KeepsCharacter_Success()
    {
        var program = new CustomProgram("Pizza Simples", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        created.Name = "Pizza Especial";
        created.Instructions = "Aquecer por 2 minutos na potência 8";
        created.TimeInSeconds = 120;

        var validationResult = await _validator.ValidateAsync(created, isUpdate: true);
        Assert.True(validationResult.IsValid);

        var updated = await _repository.UpdateAsync(created);
        Assert.Equal("Pizza Especial", updated.Name);
        Assert.Equal("Aquecer por 2 minutos na potência 8", updated.Instructions);
        Assert.Equal('P', updated.Character);
    }

    [Fact]
    public async Task Scenario_UserEditsCustomProgram_ChangesCharacter_ValidatesUniqueness()
    {
        var program1 = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var program2 = new CustomProgram("Hamburger", "Hamburger", 7, 90, 'H');

        await _repository.CreateAsync(program1);
        var created2 = await _repository.CreateAsync(program2);

        created2.Character = 'P';
        var validationResult = await _validator.ValidateAsync(created2, isUpdate: true);

        Assert.False(validationResult.IsValid);
        Assert.Contains("Caractere 'P' já está sendo usado por outro programa", validationResult.Errors);

        created2.Character = 'B';
        validationResult = await _validator.ValidateAsync(created2, isUpdate: true);
        Assert.True(validationResult.IsValid);

        var updated = await _repository.UpdateAsync(created2);
        Assert.Equal('B', updated.Character);
    }

    [Fact]
    public async Task Scenario_UserDeletesCustomProgram_CharacterBecomesAvailable()
    {
        var program = new CustomProgram("Pizza Temporária", "Pizza", 8, 120, 'T');
        var created = await _repository.CreateAsync(program);

        Assert.False(await _programDisplayService.IsCharacterUniqueAsync('T'));

        var deleted = await _repository.DeleteAsync(created.Id);
        Assert.True(deleted);

        Assert.True(await _programDisplayService.IsCharacterUniqueAsync('T'));

        var newProgram = new CustomProgram("Torta", "Torta", 6, 150, 'T');
        var validationResult = await _validator.ValidateAsync(newProgram);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task Scenario_UserCreatesInvalidProgram_GetsDetailedErrors()
    {
        var invalidProgram = new CustomProgram("", "", 11, 0, ' ', new string('A', 250));

        var validationResult = await _validator.ValidateAsync(invalidProgram);

        Assert.False(validationResult.IsValid);
        Assert.Contains("Nome do programa é obrigatório", validationResult.Errors);
        Assert.Contains("Nome do alimento é obrigatório", validationResult.Errors);
        Assert.Contains("Potência deve estar entre 1 e 10", validationResult.Errors);
        Assert.Contains("Tempo deve estar entre 1 e 7200 segundos (2 horas)", validationResult.Errors);
        Assert.Contains("Caractere não pode ser espaço em branco, tab, quebra de linha ou ponto", validationResult.Errors);
        Assert.Contains("Instruções não podem exceder 200 caracteres", validationResult.Errors);
        Assert.True(validationResult.Errors.Count >= 6);
    }

    [Fact]
    public async Task Scenario_UserViewsAllPrograms_SeesCorrectFormatting()
    {
        await _repository.CreateAsync(new CustomProgram("Pizza Média", "Pizza", 8, 90, 'M'));
        await _repository.CreateAsync(new CustomProgram("Pasta Longa", "Pasta", 6, 240, 'P'));

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();

        var pipoca = allPrograms.First(p => p.Name == "Pipoca");
        Assert.Equal("3:00", pipoca.TimeFormatted);
        Assert.Equal("predefined-program", pipoca.CssClass);
        Assert.Equal("normal", pipoca.FontStyle);

        var pizzaMedia = allPrograms.First(p => p.Name == "Pizza Média");
        Assert.Equal("1:30", pizzaMedia.TimeFormatted);
        Assert.Equal("custom-program", pizzaMedia.CssClass);
        Assert.Equal("italic", pizzaMedia.FontStyle);

        var pastaLonga = allPrograms.First(p => p.Name == "Pasta Longa");
        Assert.Equal("4:00", pastaLonga.TimeFormatted);
        Assert.Equal("custom-program", pastaLonga.CssClass);
        Assert.Equal("italic", pastaLonga.FontStyle);
    }

    [Fact]
    public async Task Scenario_UserChecksAvailableCharacters_GetsAccurateList()
    {
        await _repository.CreateAsync(new CustomProgram("Programa A", "Alimento", 5, 60, 'A'));
        await _repository.CreateAsync(new CustomProgram("Programa B", "Alimento", 5, 60, 'B'));

        var usedCharacters = await _programDisplayService.GetUsedCharactersAsync();

        Assert.Contains('∩', usedCharacters);
        Assert.Contains('∿', usedCharacters);
        Assert.Contains('≡', usedCharacters);
        Assert.Contains('∴', usedCharacters);
        Assert.Contains('◊', usedCharacters);
        Assert.Contains('.', usedCharacters);
        Assert.Contains('A', usedCharacters);
        Assert.Contains('B', usedCharacters);

        Assert.False(await _programDisplayService.IsCharacterUniqueAsync('A'));
        Assert.False(await _programDisplayService.IsCharacterUniqueAsync('∩'));
        Assert.False(await _programDisplayService.IsCharacterUniqueAsync('.'));
        Assert.True(await _programDisplayService.IsCharacterUniqueAsync('Z'));
    }

    [Fact]
    public async Task Scenario_SystemPersistence_DataSurvivesRestart()
    {
        var program = new CustomProgram("Programa Persistente", "Alimento", 7, 150, 'P');
        await _repository.CreateAsync(program);

        var newRepository = new JsonCustomProgramRepository(_testFilePath);
        var newService = new ProgramDisplayService(newRepository);

        var allPrograms = await newService.GetAllProgramsAsync();
        Assert.Equal(6, allPrograms.Count());

        var persistedProgram = allPrograms.FirstOrDefault(p => p.Name == "Programa Persistente");
        Assert.NotNull(persistedProgram);
        Assert.True(persistedProgram.IsCustom);
        Assert.Equal('P', persistedProgram.Character);
        Assert.Equal(7, persistedProgram.PowerLevel);
        Assert.Equal(150, persistedProgram.TimeInSeconds);
    }
}
