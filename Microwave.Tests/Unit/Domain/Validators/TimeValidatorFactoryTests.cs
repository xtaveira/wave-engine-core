using Microwave.Domain.Validators;
using Microwave.Domain.Factories;

namespace Microwave.Tests.Unit.Domain.Validators;

public class TimeValidatorFactoryTests
{
    [Fact]
    public void TestTimeValidatorFactory_CreateManual()
    {
        var validator = TimeValidatorFactory.CreateManual();

        Assert.IsType<ManualTimeValidator>(validator);
    }

    [Fact]
    public void TestTimeValidatorFactory_CreatePredefined()
    {
        var validator = TimeValidatorFactory.CreatePredefined();

        Assert.IsType<PredefinedTimeValidator>(validator);
    }

    [Fact]
    public void TestTimeValidatorFactory_CreateByMode_Manual()
    {
        var validator = TimeValidatorFactory.Create(HeatingMode.Manual);

        Assert.IsType<ManualTimeValidator>(validator);
    }

    [Fact]
    public void TestTimeValidatorFactory_CreateByMode_Predefined()
    {
        var validator = TimeValidatorFactory.Create(HeatingMode.Predefined);

        Assert.IsType<PredefinedTimeValidator>(validator);
    }
}
