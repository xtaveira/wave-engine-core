using Microwave.Domain.Validators;

namespace Microwave.Tests.Unit.Domain.Validators;

public class ManualTimeValidatorTests
{
    [Fact]
    public void TestManualTimeValidator_ValidTime()
    {
        var validator = new ManualTimeValidator();

        var exception = Record.Exception(() => validator.Validate(60));
        Assert.Null(exception);
    }

    [Fact]
    public void TestManualTimeValidator_TimeAtMinimum()
    {
        var validator = new ManualTimeValidator();

        var exception = Record.Exception(() => validator.Validate(1));
        Assert.Null(exception);
    }

    [Fact]
    public void TestManualTimeValidator_TimeAtMaximum()
    {
        var validator = new ManualTimeValidator();

        var exception = Record.Exception(() => validator.Validate(120));
        Assert.Null(exception);
    }

    [Fact]
    public void TestManualTimeValidator_TimeTooLow()
    {
        var validator = new ManualTimeValidator();

        var exception = Assert.Throws<ArgumentException>(() => validator.Validate(0));
        Assert.Contains("must be between 1 and 120", exception.Message);
    }

    [Fact]
    public void TestManualTimeValidator_TimeTooHigh()
    {
        var validator = new ManualTimeValidator();

        var exception = Assert.Throws<ArgumentException>(() => validator.Validate(121));
        Assert.Contains("must be between 1 and 120", exception.Message);
    }

    [Fact]
    public void TestManualTimeValidator_GetValidationContext()
    {
        var validator = new ManualTimeValidator();

        var context = validator.GetValidationContext();

        Assert.Contains("1-120", context);
    }
}
