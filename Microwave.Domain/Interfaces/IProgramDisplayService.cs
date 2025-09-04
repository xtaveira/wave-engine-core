using Microwave.Domain.DTOs;

namespace Microwave.Domain.Interfaces;

public interface IProgramDisplayService
{
    Task<IEnumerable<ProgramDisplayInfo>> GetAllProgramsAsync();
    Task<ProgramDisplayInfo?> GetProgramByIdAsync(string id);
    Task<bool> IsCharacterUniqueAsync(char character);
    Task<bool> IsCharacterUniqueAsync(char character, string excludeId);
    Task<IEnumerable<char>> GetUsedCharactersAsync();
    Task<IEnumerable<ProgramDisplayInfo>> GetPredefinedProgramsAsync();
    Task<IEnumerable<ProgramDisplayInfo>> GetCustomProgramsAsync();
}
