using Microwave.Domain;
using Microwave.Domain.Validators;
using Microwave.Domain.Factories;
using Microwave.Tests.Shared;

namespace Microwave.Tests.Unit.Domain;

public class MicrowaveOvenTests
{
    [Fact]
    public void TestMicrowaveOvenCreationAndHeating()
    {
        var microwave = new MicrowaveOven(30, 5);

        microwave.StartHeating();

        Assert.Equal(30, microwave.TimeInSeconds);
        Assert.Equal(5, microwave.PowerLevel);
    }

    [Fact]
    public void TestMicrowaveOvenInvalidTime()
    {
        Assert.Throws<ArgumentException>(() => new MicrowaveOven(150, 5));
    }

    [Fact]
    public void TestMicrowaveOvenInvalidPower()
    {
        Assert.Throws<ArgumentException>(() => new MicrowaveOven(30, 15));
    }

    [Fact]
    public void TestFactoryMethods_CreateManual()
    {
        var microwave = MicrowaveOven.CreateManual(60, 5);

        Assert.Equal(60, microwave.TimeInSeconds);
        Assert.Equal(5, microwave.PowerLevel);
    }

    [Fact]
    public void TestFactoryMethods_CreatePredefined()
    {
        var microwave = MicrowaveOven.CreatePredefined(300, 7);

        Assert.Equal(300, microwave.TimeInSeconds);
        Assert.Equal(7, microwave.PowerLevel);
    }

    [Fact]
    public void TestFactoryMethods_ManualTimeValidation()
    {
        Assert.Throws<ArgumentException>(() => MicrowaveOven.CreateManual(150, 5));
    }

    [Fact]
    public void TestFactoryMethods_PredefinedTimeValidation()
    {
        var microwave = MicrowaveOven.CreatePredefined(300, 5);
        Assert.Equal(300, microwave.TimeInSeconds);
    }

    [Fact]
    public void TestMicrowaveOvenWithCustomValidator()
    {
        var validator = new ManualTimeValidator();
        var microwave = new MicrowaveOven(60, 8, validator);

        Assert.Equal(60, microwave.TimeInSeconds);
        Assert.Equal(8, microwave.PowerLevel);
    }

    [Fact]
    public void TestMicrowaveOvenValidationContext()
    {
        var validator = new PredefinedTimeValidator();

        var microwave = new MicrowaveOven(600, 5, validator);
        Assert.Equal(600, microwave.TimeInSeconds);

        Assert.Throws<ArgumentException>(() => new MicrowaveOven(2000, 5, validator));
    }
}
