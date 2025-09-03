using Microwave.Domain.Validators;

namespace Microwave.Tests.Unit.Domain.Validators;

public class PredefinedTimeValidatorTests
{
    [Fact]
    public void TestPredefinedTimeValidator_ValidTime()
    {
        var validator = new PredefinedTimeValidator();

        var exception = Record.Exception(() => validator.Validate(300));
        Assert.Null(exception);
    }

    [Fact]
    public void TestPredefinedTimeValidator_TimeAtMinimum()
    {
        var validator = new PredefinedTimeValidator();

        var exception = Record.Exception(() => validator.Validate(1));
        Assert.Null(exception);
    }

    [Fact]
    public void TestPredefinedTimeValidator_TimeAtMaximum()
    {
        var validator = new PredefinedTimeValidator();

        var exception = Record.Exception(() => validator.Validate(1800));
        Assert.Null(exception);
    }

    [Fact]
    public void TestPredefinedTimeValidator_HighTimeAllowed()
    {
        var validator = new PredefinedTimeValidator();

        var exception = Record.Exception(() => validator.Validate(600));
        Assert.Null(exception);
    }

    [Fact]
    public void TestPredefinedTimeValidator_TimeTooLow()
    {
        var validator = new PredefinedTimeValidator();

        var exception = Assert.Throws<ArgumentException>(() => validator.Validate(0));
        Assert.Contains("must be between 1 and 1800", exception.Message);
    }

    [Fact]
    public void TestPredefinedTimeValidator_TimeTooHigh()
    {
        var validator = new PredefinedTimeValidator();

        var exception = Assert.Throws<ArgumentException>(() => validator.Validate(1801));
        Assert.Contains("must be between 1 and 1800", exception.Message);
    }

    [Fact]
    public void TestPredefinedTimeValidator_GetValidationContext()
    {
        var validator = new PredefinedTimeValidator();

        var context = validator.GetValidationContext();

        Assert.Contains("1-1800", context);
    }
}
