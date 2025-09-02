using Microwave.Domain;

namespace Microwave.Tests;

public class UnitTest1
{
    [Fact]
    public void TestMicrowaveOvenCreationAndHeating()
    {
        var microwave = new MicrowaveOven(30, 5);

        var result = microwave.StartHeating();

        Assert.Equal("Aquecimento iniciado: 30s a potência 5.", result);
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
}
