namespace Microwave.Domain.DTOs;

public class StartHeatingRequest
{
    public int TimeInSeconds { get; set; }
    public int PowerLevel { get; set; }
}

public class AddTimeRequest
{
    public int AdditionalSeconds { get; set; }
}

public class HeatingStatusResponse
{
    public bool IsRunning { get; set; }
    public int RemainingTime { get; set; }
    public int PowerLevel { get; set; }
    public int Progress { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public string? HeatingChar { get; set; }
    public string? CurrentProgram { get; set; }
    public string? ProgressDisplay { get; set; }
    public DateTime? StartTime { get; set; }
}

public class StartPredefinedProgramRequest
{
    public string ProgramName { get; set; } = string.Empty;
}
