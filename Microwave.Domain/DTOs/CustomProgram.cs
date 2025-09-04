using System.ComponentModel.DataAnnotations;

namespace Microwave.Domain.DTOs;

public class CustomProgram
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Nome do programa é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome do alimento é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Alimento deve ter entre 2 e 50 caracteres")]
    public string Food { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Potência deve estar entre 1 e 10")]
    public int PowerLevel { get; set; }

    [Range(1, 7200, ErrorMessage = "Tempo deve estar entre 1 e 7200 segundos")]
    public int TimeInSeconds { get; set; }

    [Required(ErrorMessage = "Caractere de aquecimento é obrigatório")]
    public char Character { get; set; }

    [StringLength(200, ErrorMessage = "Instruções não podem exceder 200 caracteres")]
    public string Instructions { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsCustom => true;

    public CustomProgram()
    {
    }

    public CustomProgram(string name, string food, int powerLevel, int timeInSeconds, char character, string instructions = "")
    {
        Name = name;
        Food = food;
        PowerLevel = powerLevel;
        TimeInSeconds = timeInSeconds;
        Character = character;
        Instructions = instructions;
    }

    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Nome do programa é obrigatório");

        if (string.IsNullOrWhiteSpace(Food))
            errors.Add("Nome do alimento é obrigatório");

        if (PowerLevel < 1 || PowerLevel > 10)
            errors.Add("Potência deve estar entre 1 e 10");

        if (TimeInSeconds < 1 || TimeInSeconds > 7200)
            errors.Add("Tempo deve estar entre 1 e 7200 segundos");

        if (Character == '\0')
            errors.Add("Caractere de aquecimento é obrigatório");

        if (Character == '.')
            errors.Add("Caractere '.' é reservado e não pode ser usado");

        if (!string.IsNullOrEmpty(Instructions) && Instructions.Length > 200)
            errors.Add("Instruções não podem exceder 200 caracteres");

        return errors.Count == 0;
    }
}
