namespace Microwave.Domain.DTOs;

public class ProgramDisplayInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Food { get; set; } = string.Empty;
    public int PowerLevel { get; set; }
    public int TimeInSeconds { get; set; }
    public char Character { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string DisplayName => IsCustom ? $"{Name} (Personalizado)" : Name;
    public string TimeFormatted => TimeInSeconds > 60 ? $"{TimeInSeconds / 60}:{TimeInSeconds % 60:D2}" : $"{TimeInSeconds}s";
    public string CssClass => IsCustom ? "custom-program" : "predefined-program";
    public string FontStyle => IsCustom ? "italic" : "normal";

    public static ProgramDisplayInfo FromPredefined(PredefinedProgram predefined)
    {
        return new ProgramDisplayInfo
        {
            Id = predefined.Name.Replace(" ", "").ToLower(),
            Name = predefined.Name,
            Food = predefined.Food,
            PowerLevel = predefined.PowerLevel,
            TimeInSeconds = predefined.TimeInSeconds,
            Character = string.IsNullOrEmpty(predefined.HeatingChar) ? '.' : predefined.HeatingChar[0],
            Instructions = predefined.Instructions,
            IsCustom = false,
            CreatedAt = null
        };
    }

    public static ProgramDisplayInfo FromCustom(CustomProgram custom)
    {
        return new ProgramDisplayInfo
        {
            Id = custom.Id.ToString(),
            Name = custom.Name,
            Food = custom.Food,
            PowerLevel = custom.PowerLevel,
            TimeInSeconds = custom.TimeInSeconds,
            Character = custom.Character,
            Instructions = custom.Instructions,
            IsCustom = true,
            CreatedAt = custom.CreatedAt
        };
    }
}
