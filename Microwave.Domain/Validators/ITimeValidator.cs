namespace Microwave.Domain.Validators;

public interface ITimeValidator
{
    void Validate(int timeInSeconds);
    string GetValidationContext();
}
