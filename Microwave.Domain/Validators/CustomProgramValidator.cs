using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;

namespace Microwave.Domain.Validators;

public class CustomProgramValidator
{
    private readonly IProgramDisplayService _programDisplayService;
    private static readonly char[] ForbiddenCharacters = { '.', ' ', '\t', '\n', '\r' };
    private static readonly char[] SpecialCharacters = { '∩', '∿', '≡', '∴', '◊' };

    public CustomProgramValidator(IProgramDisplayService programDisplayService)
    {
        _programDisplayService = programDisplayService;
    }

    public async Task<ValidationResult> ValidateAsync(CustomProgram program, bool isUpdate = false)
    {
        var result = new ValidationResult();

        ValidateBasicFields(program, result);
        await ValidateUniqueCharacterAsync(program, result, isUpdate);
        await ValidateUniqueNameAsync(program, result, isUpdate);

        return result;
    }

    private void ValidateBasicFields(CustomProgram program, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(program.Name))
        {
            result.AddError("Nome do programa é obrigatório");
        }
        else if (program.Name.Length < 2 || program.Name.Length > 50)
        {
            result.AddError("Nome deve ter entre 2 e 50 caracteres");
        }

        if (string.IsNullOrWhiteSpace(program.Food))
        {
            result.AddError("Nome do alimento é obrigatório");
        }
        else if (program.Food.Length < 2 || program.Food.Length > 50)
        {
            result.AddError("Alimento deve ter entre 2 e 50 caracteres");
        }

        if (program.PowerLevel < 1 || program.PowerLevel > 10)
        {
            result.AddError("Potência deve estar entre 1 e 10");
        }

        if (program.TimeInSeconds < 1 || program.TimeInSeconds > 7200)
        {
            result.AddError("Tempo deve estar entre 1 e 7200 segundos (2 horas)");
        }

        ValidateCharacter(program.Character, result);

        if (!string.IsNullOrEmpty(program.Instructions) && program.Instructions.Length > 200)
        {
            result.AddError("Instruções não podem exceder 200 caracteres");
        }
    }

    private void ValidateCharacter(char character, ValidationResult result)
    {
        if (character == '\0')
        {
            result.AddError("Caractere de aquecimento é obrigatório");
            return;
        }

        if (ForbiddenCharacters.Contains(character))
        {
            result.AddError("Caractere não pode ser espaço em branco, tab, quebra de linha ou ponto");
            return;
        }

        if (char.IsControl(character))
        {
            result.AddError("Caractere não pode ser um caractere de controle");
            return;
        }

        if (char.IsWhiteSpace(character))
        {
            result.AddError("Caractere não pode ser um espaço em branco");
            return;
        }
    }

    private async Task ValidateUniqueCharacterAsync(CustomProgram program, ValidationResult result, bool isUpdate)
    {
        bool isUnique = isUpdate
            ? await _programDisplayService.IsCharacterUniqueAsync(program.Character, program.Id.ToString())
            : await _programDisplayService.IsCharacterUniqueAsync(program.Character);

        if (!isUnique)
        {
            result.AddError($"Caractere '{program.Character}' já está sendo usado por outro programa");
        }
    }

    private async Task ValidateUniqueNameAsync(CustomProgram program, ValidationResult result, bool isUpdate)
    {
        await Task.CompletedTask;
    }
}

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddErrors(IEnumerable<string> errors)
    {
        Errors.AddRange(errors);
    }
}