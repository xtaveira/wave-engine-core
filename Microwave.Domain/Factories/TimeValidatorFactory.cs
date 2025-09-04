using Microwave.Domain.Validators;

namespace Microwave.Domain.Factories;

public enum HeatingMode
{
    Manual,
    Predefined,
    Custom
}

public static class TimeValidatorFactory
{
    public static ITimeValidator Create(HeatingMode mode)
    {
        return mode switch
        {
            HeatingMode.Manual => new ManualTimeValidator(),
            HeatingMode.Predefined => new PredefinedTimeValidator(),
            HeatingMode.Custom => new CustomTimeValidator(),
            _ => throw new ArgumentException($"Invalid heating mode: {mode}")
        };
    }

    public static ITimeValidator CreateManual() => Create(HeatingMode.Manual);
    public static ITimeValidator CreatePredefined() => Create(HeatingMode.Predefined);
    public static ITimeValidator CreateCustom() => Create(HeatingMode.Custom);
}
