using Microwave.Domain.DTOs;

namespace Microwave.Domain.Interfaces;

public interface ICustomProgramRepository
{
    Task<IEnumerable<CustomProgram>> GetAllAsync();
    Task<CustomProgram?> GetByIdAsync(Guid id);
    Task<CustomProgram> CreateAsync(CustomProgram program);
    Task<CustomProgram> UpdateAsync(CustomProgram program);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsCharacterAsync(char character);
    Task<bool> ExistsCharacterAsync(char character, Guid excludeId);
    Task<bool> ExistsNameAsync(string name);
    Task<bool> ExistsNameAsync(string name, Guid excludeId);
    Task<int> GetCountAsync();
}
