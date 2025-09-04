using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;

namespace Microwave.Infrastructure.Services;

public class ProgramDisplayService : IProgramDisplayService
{
    private readonly ICustomProgramRepository _customProgramRepository;
    private readonly List<PredefinedProgram> _predefinedPrograms;

    public ProgramDisplayService(ICustomProgramRepository customProgramRepository)
    {
        _customProgramRepository = customProgramRepository;

        _predefinedPrograms = new List<PredefinedProgram>
        {
            new() { Name = "Pipoca", Food = "Pipoca", TimeInSeconds = 180, PowerLevel = 7, HeatingChar = "∩", Instructions = "Observar o barulho dos estouros. Parar quando os intervalos forem maiores que 3 segundos." },
            new() { Name = "Leite", Food = "Leite", TimeInSeconds = 300, PowerLevel = 5, HeatingChar = "∿", Instructions = "Aquecer o leite em temperatura média. Mexer a cada 1 minuto para aquecer uniformemente." },
            new() { Name = "Carnes de boi", Food = "Carnes de boi", TimeInSeconds = 840, PowerLevel = 4, HeatingChar = "≡", Instructions = "Descongelar e aquecer a carne. Virar a carne na metade do tempo para aquecimento uniforme." },
            new() { Name = "Frango", Food = "Frango", TimeInSeconds = 480, PowerLevel = 7, HeatingChar = "∴", Instructions = "Descongelar e aquecer frango. Certificar que esteja bem aquecido antes de consumir." },
            new() { Name = "Feijão", Food = "Feijão", TimeInSeconds = 480, PowerLevel = 9, HeatingChar = "◊", Instructions = "Aquecer feijão já cozido. Mexer na metade do tempo. Adicionar água se necessário." }
        };
    }

    public async Task<IEnumerable<ProgramDisplayInfo>> GetAllProgramsAsync()
    {
        var customPrograms = await GetCustomProgramsAsync();
        var predefinedPrograms = await GetPredefinedProgramsAsync();

        var allPrograms = predefinedPrograms.Concat(customPrograms).ToList();

        return allPrograms;
    }

    public async Task<ProgramDisplayInfo?> GetProgramByIdAsync(string id)
    {
        var allPrograms = await GetAllProgramsAsync();
        return allPrograms.FirstOrDefault(p => p.Id == id);
    }

    public async Task<bool> IsCharacterUniqueAsync(char character)
    {
        if (character == '.')
            return false;

        var existsInPredefined = _predefinedPrograms.Any(p =>
            !string.IsNullOrEmpty(p.HeatingChar) && p.HeatingChar[0] == character);

        if (existsInPredefined)
            return false;

        var existsInCustom = await _customProgramRepository.ExistsCharacterAsync(character);

        return !existsInCustom;
    }

    public async Task<bool> IsCharacterUniqueAsync(char character, string excludeId)
    {
        if (character == '.')
            return false;

        var existsInPredefined = _predefinedPrograms.Any(p =>
            !string.IsNullOrEmpty(p.HeatingChar) && p.HeatingChar[0] == character);

        if (existsInPredefined)
            return false;

        if (Guid.TryParse(excludeId, out var excludeGuid))
        {
            var existsInCustom = await _customProgramRepository.ExistsCharacterAsync(character, excludeGuid);
            return !existsInCustom;
        }

        var exists = await _customProgramRepository.ExistsCharacterAsync(character);
        return !exists;
    }

    public async Task<IEnumerable<char>> GetUsedCharactersAsync()
    {
        var usedChars = new List<char>();

        usedChars.AddRange(_predefinedPrograms
            .Where(p => !string.IsNullOrEmpty(p.HeatingChar))
            .Select(p => p.HeatingChar[0]));

        var customPrograms = await _customProgramRepository.GetAllAsync();
        usedChars.AddRange(customPrograms.Select(p => p.Character));

        usedChars.Add('.');

        return usedChars.Distinct();
    }

    public Task<IEnumerable<ProgramDisplayInfo>> GetPredefinedProgramsAsync()
    {
        var result = _predefinedPrograms.Select(ProgramDisplayInfo.FromPredefined).ToList();
        return Task.FromResult<IEnumerable<ProgramDisplayInfo>>(result);
    }

    public async Task<IEnumerable<ProgramDisplayInfo>> GetCustomProgramsAsync()
    {
        var customPrograms = await _customProgramRepository.GetAllAsync();
        return customPrograms.Select(ProgramDisplayInfo.FromCustom).ToList();
    }
}
