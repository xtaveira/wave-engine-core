namespace Microwave.Domain.DTOs;

public class PredefinedProgram
{
    public string Name { get; set; } = string.Empty;
    public string Food { get; set; } = string.Empty;
    public int TimeInSeconds { get; set; }
    public int PowerLevel { get; set; }
    public string HeatingChar { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}
