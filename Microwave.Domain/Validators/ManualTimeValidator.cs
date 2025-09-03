namespace Microwave.Domain.Validators;

public class ManualTimeValidator : ITimeValidator
{
    private const int MIN_TIME = 1;
    private const int MAX_TIME = 120;

    public void Validate(int timeInSeconds)
    {
        if (timeInSeconds < MIN_TIME || timeInSeconds > MAX_TIME)
            throw new ArgumentException($"Manual heating: Time must be between {MIN_TIME} and {MAX_TIME} seconds.");
    }

    public string GetValidationContext()
    {
        return $"Manual heating allows {MIN_TIME}-{MAX_TIME} seconds";
    }
}
