using Microwave.Domain.Validators;

namespace Microwave.Domain.Factories;

public enum HeatingMode
{
    Manual,
    Predefined
}

public static class TimeValidatorFactory
{
    public static ITimeValidator Create(HeatingMode mode)
    {
        return mode switch
        {
            HeatingMode.Manual => new ManualTimeValidator(),
            HeatingMode.Predefined => new PredefinedTimeValidator(),
            _ => throw new ArgumentException($"Invalid heating mode: {mode}")
        };
    }

    public static ITimeValidator CreateManual() => Create(HeatingMode.Manual);
    public static ITimeValidator CreatePredefined() => Create(HeatingMode.Predefined);
}
