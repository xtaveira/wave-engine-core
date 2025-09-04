using Microwave.Domain.DTOs;
using Microwave.Infrastructure.Repositories;

namespace Microwave.Tests.Unit.Infrastructure;

public class JsonCustomProgramRepositoryTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly JsonCustomProgramRepository _repository;

    public JsonCustomProgramRepositoryTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test-custom-programs-{Guid.NewGuid()}.json");
        _repository = new JsonCustomProgramRepository(_testFilePath);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public async Task CreateAsync_ValidProgram_ReturnsCreatedProgram()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P', "Aquecer pizza");

        var result = await _repository.CreateAsync(program);

        Assert.NotNull(result);
        Assert.Equal("Pizza", result.Name);
        Assert.Equal("Pizza", result.Food);
        Assert.Equal(8, result.PowerLevel);
        Assert.Equal(120, result.TimeInSeconds);
        Assert.Equal('P', result.Character);
        Assert.Equal("Aquecer pizza", result.Instructions);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ProgramWithEmptyId_GeneratesNewId()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = Guid.Empty };

        var result = await _repository.CreateAsync(program);

        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithPrograms_ReturnsAllPrograms()
    {
        var program1 = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var program2 = new CustomProgram("Lasanha", "Lasanha", 6, 180, 'L');

        await _repository.CreateAsync(program1);
        await _repository.CreateAsync(program2);

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Name == "Pizza");
        Assert.Contains(result, p => p.Name == "Lasanha");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProgram_ReturnsProgram()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        var result = await _repository.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Pizza", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProgram_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProgram_UpdatesProgram()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        created.Name = "Pizza Atualizada";
        created.PowerLevel = 9;

        var result = await _repository.UpdateAsync(created);

        Assert.Equal("Pizza Atualizada", result.Name);
        Assert.Equal(9, result.PowerLevel);

        var retrieved = await _repository.GetByIdAsync(created.Id);
        Assert.Equal("Pizza Atualizada", retrieved?.Name);
        Assert.Equal(9, retrieved?.PowerLevel);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProgram_ThrowsException()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P') { Id = Guid.NewGuid() };

        await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(program));
    }

    [Fact]
    public async Task DeleteAsync_ExistingProgram_DeletesAndReturnsTrue()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        var result = await _repository.DeleteAsync(created.Id);

        Assert.True(result);

        var retrieved = await _repository.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProgram_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsCharacterAsync_ExistingCharacter_ReturnsTrue()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        await _repository.CreateAsync(program);

        var result = await _repository.ExistsCharacterAsync('P');

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsCharacterAsync_NonExistingCharacter_ReturnsFalse()
    {
        var result = await _repository.ExistsCharacterAsync('X');

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsCharacterAsync_WithExclusion_ExcludesSpecifiedProgram()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        var created = await _repository.CreateAsync(program);

        var result = await _repository.ExistsCharacterAsync('P', created.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsNameAsync_ExistingName_ReturnsTrue()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        await _repository.CreateAsync(program);

        var result = await _repository.ExistsNameAsync("Pizza");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsNameAsync_ExistingNameDifferentCase_ReturnsTrue()
    {
        var program = new CustomProgram("Pizza", "Pizza", 8, 120, 'P');
        await _repository.CreateAsync(program);

        var result = await _repository.ExistsNameAsync("PIZZA");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsNameAsync_NonExistingName_ReturnsFalse()
    {
        var result = await _repository.ExistsNameAsync("Hambúrguer");

        Assert.False(result);
    }

    [Fact]
    public async Task GetCountAsync_EmptyRepository_ReturnsZero()
    {
        var result = await _repository.GetCountAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCountAsync_WithPrograms_ReturnsCorrectCount()
    {
        await _repository.CreateAsync(new CustomProgram("Pizza", "Pizza", 8, 120, 'P'));
        await _repository.CreateAsync(new CustomProgram("Lasanha", "Lasanha", 6, 180, 'L'));
        await _repository.CreateAsync(new CustomProgram("Hambúrguer", "Hambúrguer", 7, 90, 'H'));

        var result = await _repository.GetCountAsync();

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task ConcurrentOperations_MultipleCreates_AllSucceed()
    {
        var tasks = new List<Task<CustomProgram>>();

        for (int i = 0; i < 10; i++)
        {
            var program = new CustomProgram($"Program{i}", $"Food{i}", 5, 60, (char)('A' + i));
            tasks.Add(_repository.CreateAsync(program));
        }

        var results = await Task.WhenAll(tasks);

        Assert.Equal(10, results.Length);
        Assert.All(results, r => Assert.NotEqual(Guid.Empty, r.Id));

        var allPrograms = await _repository.GetAllAsync();
        Assert.Equal(10, allPrograms.Count());
    }

    [Fact]
    public async Task FileRecovery_CorruptedFile_RecreatesStructure()
    {
        await File.WriteAllTextAsync(_testFilePath, "invalid json content");

        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
