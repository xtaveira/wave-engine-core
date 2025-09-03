namespace Microwave.Domain.DTOs;

public class MicrowaveStatus
{
    public bool IsRunning { get; set; }
    public int RemainingTime { get; set; }
    public int PowerLevel { get; set; }
    public int Progress { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string FormattedRemainingTime { get; set; } = string.Empty;
}