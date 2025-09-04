using Microwave.Domain.DTOs;
using Microwave.Domain.Validators;
using Microwave.Infrastructure.Repositories;
using Microwave.Infrastructure.Services;

namespace Microwave.Tests.Integration;

public class CustomProgramIntegrationTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly JsonCustomProgramRepository _repository;
    private readonly ProgramDisplayService _programDisplayService;
    private readonly CustomProgramValidator _validator;

    public CustomProgramIntegrationTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"integration-test-{Guid.NewGuid()}.json");
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
    public async Task CompleteWorkflow_CreateValidateAndRetrieve_Success()
    {
        var program = new CustomProgram("Pizza Margherita", "Pizza", 8, 180, 'M', "Aquecer pizza margherita por 3 minutos");

        var validationResult = await _validator.ValidateAsync(program);
        Assert.True(validationResult.IsValid);

        var createdProgram = await _repository.CreateAsync(program);
        Assert.NotNull(createdProgram);
        Assert.NotEqual(Guid.Empty, createdProgram.Id);

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();
        Assert.Equal(6, allPrograms.Count());
        Assert.Contains(allPrograms, p => p.Name == "Pizza Margherita" && p.IsCustom);

        var retrievedProgram = await _programDisplayService.GetProgramByIdAsync(createdProgram.Id.ToString());
        Assert.NotNull(retrievedProgram);
        Assert.Equal("Pizza Margherita", retrievedProgram.Name);
        Assert.True(retrievedProgram.IsCustom);
    }

    [Fact]
    public async Task CharacterValidation_DuplicateWithPredefined_Fails()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, '∩');

        var validationResult = await _validator.ValidateAsync(program);

        Assert.False(validationResult.IsValid);
        Assert.Contains("Caractere '∩' já está sendo usado por outro programa", validationResult.Errors);
    }

    [Fact]
    public async Task CharacterValidation_DuplicateWithCustom_Fails()
    {
        var firstProgram = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        await _repository.CreateAsync(firstProgram);

        var secondProgram = new CustomProgram("Pasta", "Pasta", 6, 150, 'P');

        var validationResult = await _validator.ValidateAsync(secondProgram);

        Assert.False(validationResult.IsValid);
        Assert.Contains("Caractere 'P' já está sendo usado por outro programa", validationResult.Errors);
    }

    [Fact]
    public async Task UpdateWorkflow_ChangeCharacterToUnique_Success()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        created.Character = 'Z';
        var validationResult = await _validator.ValidateAsync(created, isUpdate: true);
        Assert.True(validationResult.IsValid);

        var updated = await _repository.UpdateAsync(created);
        Assert.Equal('Z', updated.Character);

        var retrieved = await _repository.GetByIdAsync(created.Id);
        Assert.Equal('Z', retrieved?.Character);
    }

    [Fact]
    public async Task UpdateWorkflow_KeepSameCharacter_Success()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        created.Name = "Pizza Atualizada";
        var validationResult = await _validator.ValidateAsync(created, isUpdate: true);
        Assert.True(validationResult.IsValid);

        var updated = await _repository.UpdateAsync(created);
        Assert.Equal("Pizza Atualizada", updated.Name);
        Assert.Equal('P', updated.Character);
    }

    [Fact]
    public async Task DeleteWorkflow_RemoveProgram_Success()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        var allProgramsBefore = await _programDisplayService.GetAllProgramsAsync();
        Assert.Equal(6, allProgramsBefore.Count());

        var deleted = await _repository.DeleteAsync(created.Id);
        Assert.True(deleted);

        var allProgramsAfter = await _programDisplayService.GetAllProgramsAsync();
        Assert.Equal(5, allProgramsAfter.Count());
        Assert.DoesNotContain(allProgramsAfter, p => p.Name == "Pizza");

        var characterIsUnique = await _programDisplayService.IsCharacterUniqueAsync('P');
        Assert.True(characterIsUnique);
    }

    [Fact]
    public async Task MultiplePrograms_DifferentCharacters_AllValid()
    {
        var programs = new[]
        {
            new CustomProgram("Pizza", "Pizza", 8, 120, 'P'),
            new CustomProgram("Hamburger", "Hamburger", 7, 90, 'H'),
            new CustomProgram("Sandwich", "Sandwich", 5, 60, 'S'),
            new CustomProgram("Pasta", "Pasta", 6, 150, 'A')
        };

        foreach (var program in programs)
        {
            var validationResult = await _validator.ValidateAsync(program);
            Assert.True(validationResult.IsValid);

            await _repository.CreateAsync(program);
        }

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();
        Assert.Equal(9, allPrograms.Count());
        Assert.Equal(4, allPrograms.Count(p => p.IsCustom));

        var usedCharacters = await _programDisplayService.GetUsedCharactersAsync();
        Assert.Contains('P', usedCharacters);
        Assert.Contains('H', usedCharacters);
        Assert.Contains('S', usedCharacters);
        Assert.Contains('A', usedCharacters);
    }

    [Fact]
    public async Task UsedCharacters_IncludesPredefinedAndCustom_Success()
    {
        await _repository.CreateAsync(new CustomProgram("Pizza", "Pizza", 8, 120, 'P'));
        await _repository.CreateAsync(new CustomProgram("Hamburger", "Hamburger", 7, 90, 'H'));

        var usedCharacters = await _programDisplayService.GetUsedCharactersAsync();

        Assert.Contains('∩', usedCharacters);
        Assert.Contains('∿', usedCharacters);
        Assert.Contains('≡', usedCharacters);
        Assert.Contains('∴', usedCharacters);
        Assert.Contains('◊', usedCharacters);
        Assert.Contains('.', usedCharacters);
        Assert.Contains('P', usedCharacters);
        Assert.Contains('H', usedCharacters);
        Assert.Equal(8, usedCharacters.Count());
    }

    [Fact]
    public async Task ConcurrentOperations_CreateAndValidate_ThreadSafe()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 5; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var program = new CustomProgram($"Program{index}", $"Food{index}", 5, 60, (char)('A' + index));
                var validationResult = await _validator.ValidateAsync(program);

                if (validationResult.IsValid)
                {
                    await _repository.CreateAsync(program);
                }
            }));
        }

        await Task.WhenAll(tasks);

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();
        var customPrograms = allPrograms.Where(p => p.IsCustom);
        Assert.Equal(5, customPrograms.Count());
    }

    [Fact]
    public async Task ProgramOrdering_PredefinedFirst_ThenCustom()
    {
        await _repository.CreateAsync(new CustomProgram("ZZZ Last", "Food", 5, 60, 'Z'));
        await _repository.CreateAsync(new CustomProgram("AAA First", "Food", 5, 60, 'A'));

        var allPrograms = await _programDisplayService.GetAllProgramsAsync();
        var programList = allPrograms.ToList();

        var predefinedCount = programList.Count(p => !p.IsCustom);
        Assert.Equal(5, predefinedCount);

        for (int i = 0; i < 5; i++)
        {
            Assert.False(programList[i].IsCustom);
        }

        for (int i = 5; i < programList.Count; i++)
        {
            Assert.True(programList[i].IsCustom);
        }
    }
}
