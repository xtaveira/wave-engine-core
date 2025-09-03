namespace Microwave.Domain.Validators;

public class PredefinedTimeValidator : ITimeValidator
{
    private const int MIN_TIME = 1;
    private const int MAX_TIME = 1800;

    public void Validate(int timeInSeconds)
    {
        if (timeInSeconds < MIN_TIME || timeInSeconds > MAX_TIME)
            throw new ArgumentException($"Predefined program: Time must be between {MIN_TIME} and {MAX_TIME} seconds.");
    }

    public string GetValidationContext()
    {
        return $"Predefined programs allow {MIN_TIME}-{MAX_TIME} seconds";
    }
}
