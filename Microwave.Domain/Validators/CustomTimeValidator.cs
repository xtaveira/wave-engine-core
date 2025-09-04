namespace Microwave.Domain.Validators;

public class CustomTimeValidator : ITimeValidator
{
    private const int MIN_TIME = 1;
    private const int MAX_TIME = 7200;

    public void Validate(int timeInSeconds)
    {
        if (timeInSeconds < MIN_TIME || timeInSeconds > MAX_TIME)
            throw new ArgumentException($"Custom program: Time must be between {MIN_TIME} and {MAX_TIME} seconds.");
    }

    public string GetValidationContext()
    {
        return $"Custom programs allow {MIN_TIME}-{MAX_TIME} seconds";
    }
}
