using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Domain.Validators;

namespace Microwave.Application;

public class CustomProgramService
{
    private readonly ICustomProgramRepository _repository;
    private readonly CustomProgramValidator _validator;

    public CustomProgramService(ICustomProgramRepository repository, IProgramDisplayService programDisplayService)
    {
        _repository = repository;
        _validator = new CustomProgramValidator(programDisplayService);
    }

    public async Task<OperationResult<CustomProgram>> CreateAsync(CustomProgram program)
    {
        var validationResult = await _validator.ValidateAsync(program);
        if (!validationResult.IsValid)
        {
            return OperationResult<CustomProgram>.CreateError(
                string.Join(", ", validationResult.Errors),
                "VALIDATION_FAILED");
        }

        try
        {
            await _repository.CreateAsync(program);
            return OperationResult<CustomProgram>.CreateSuccess(program, "Programa customizado criado com sucesso.");
        }
        catch (Exception ex)
        {
            return OperationResult<CustomProgram>.CreateError($"Erro ao criar programa: {ex.Message}", "CREATION_FAILED");
        }
    }

    public async Task<OperationResult<CustomProgram>> UpdateAsync(CustomProgram program)
    {
        var existingProgram = await _repository.GetByIdAsync(program.Id);
        if (existingProgram == null)
        {
            return OperationResult<CustomProgram>.CreateError("Programa não encontrado.", "PROGRAM_NOT_FOUND");
        }

        var validationResult = await _validator.ValidateAsync(program, isUpdate: true);
        if (!validationResult.IsValid)
        {
            return OperationResult<CustomProgram>.CreateError(
                string.Join(", ", validationResult.Errors),
                "VALIDATION_FAILED");
        }

        try
        {
            await _repository.UpdateAsync(program);
            return OperationResult<CustomProgram>.CreateSuccess(program, "Programa customizado atualizado com sucesso.");
        }
        catch (Exception ex)
        {
            return OperationResult<CustomProgram>.CreateError($"Erro ao atualizar programa: {ex.Message}", "UPDATE_FAILED");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var existingProgram = await _repository.GetByIdAsync(id);
        if (existingProgram == null)
        {
            return OperationResult.CreateError("Programa não encontrado.", "PROGRAM_NOT_FOUND");
        }

        try
        {
            await _repository.DeleteAsync(id);
            return OperationResult.CreateSuccess("Programa customizado deletado com sucesso.");
        }
        catch (Exception ex)
        {
            return OperationResult.CreateError($"Erro ao deletar programa: {ex.Message}", "DELETE_FAILED");
        }
    }

    public async Task<CustomProgram?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<CustomProgram>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<bool> ExistsCharacterAsync(char character, Guid? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            return await _repository.ExistsCharacterAsync(character, excludeId.Value);
        }
        return await _repository.ExistsCharacterAsync(character);
    }
}
