using System.Text.Json;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;

namespace Microwave.Infrastructure.Repositories;

public class JsonCustomProgramRepository : ICustomProgramRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore;

    public JsonCustomProgramRepository(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "custom-programs.json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        _semaphore = new SemaphoreSlim(1, 1);
        EnsureFileExists();
    }

    public async Task<IEnumerable<CustomProgram>> GetAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            return data.Programs;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<CustomProgram?> GetByIdAsync(Guid id)
    {
        var programs = await GetAllAsync();
        return programs.FirstOrDefault(p => p.Id == id);
    }

    public async Task<CustomProgram> CreateAsync(CustomProgram program)
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = await ReadFileAsync();

            if (program.Id == Guid.Empty)
                program.Id = Guid.NewGuid();

            data.Programs.Add(program);
            await WriteFileAsync(data);

            return program;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<CustomProgram> UpdateAsync(CustomProgram program)
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = await ReadFileAsync();

            var existingIndex = data.Programs.FindIndex(p => p.Id == program.Id);
            if (existingIndex == -1)
                throw new ArgumentException($"Programa com ID {program.Id} não encontrado");

            data.Programs[existingIndex] = program;
            await WriteFileAsync(data);

            return program;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = await ReadFileAsync();

            var programToRemove = data.Programs.FirstOrDefault(p => p.Id == id);
            if (programToRemove == null)
                return false;

            data.Programs.Remove(programToRemove);
            await WriteFileAsync(data);

            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> ExistsCharacterAsync(char character)
    {
        var programs = await GetAllAsync();
        return programs.Any(p => p.Character == character);
    }

    public async Task<bool> ExistsCharacterAsync(char character, Guid excludeId)
    {
        var programs = await GetAllAsync();
        return programs.Any(p => p.Character == character && p.Id != excludeId);
    }

    public async Task<bool> ExistsNameAsync(string name)
    {
        var programs = await GetAllAsync();
        return programs.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsNameAsync(string name, Guid excludeId)
    {
        var programs = await GetAllAsync();
        return programs.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.Id != excludeId);
    }

    public async Task<int> GetCountAsync()
    {
        var programs = await GetAllAsync();
        return programs.Count();
    }

    private void EnsureFileExists()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_filePath))
        {
            var initialData = new JsonFileData { Programs = new List<CustomProgram>() };
            var json = JsonSerializer.Serialize(initialData, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }

    private async Task<JsonFileData> ReadFileAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var data = JsonSerializer.Deserialize<JsonFileData>(json, _jsonOptions);
            return data ?? new JsonFileData { Programs = new List<CustomProgram>() };
        }
        catch (Exception)
        {
            return new JsonFileData { Programs = new List<CustomProgram>() };
        }
    }

    private async Task WriteFileAsync(JsonFileData data)
    {
        data.Metadata = new JsonMetadata
        {
            Version = "1.0",
            LastModified = DateTime.Now,
            TotalPrograms = data.Programs.Count
        };

        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}

public class JsonFileData
{
    public List<CustomProgram> Programs { get; set; } = new();
    public JsonMetadata Metadata { get; set; } = new();
}

public class JsonMetadata
{
    public string Version { get; set; } = "1.0";
    public DateTime LastModified { get; set; } = DateTime.Now;
    public int TotalPrograms { get; set; } = 0;
}
