using System;
using Microwave.Domain.Validators;
using Microwave.Domain.Factories;

namespace Microwave.Domain
{
    public class MicrowaveOven
    {
        public int TimeInSeconds { get; private set; }
        public int PowerLevel { get; private set; }

        public MicrowaveOven(int timeInSeconds, int powerLevel)
            : this(timeInSeconds, powerLevel, TimeValidatorFactory.CreateManual())
        {
        }

        public MicrowaveOven(int timeInSeconds, int powerLevel, ITimeValidator timeValidator)
        {
            timeValidator.Validate(timeInSeconds);

            if (powerLevel < 1 || powerLevel > 10)
                throw new ArgumentException("Power level must be between 1 and 10.");

            TimeInSeconds = timeInSeconds;
            PowerLevel = powerLevel;
        }

        public static MicrowaveOven CreateManual(int timeInSeconds, int powerLevel)
        {
            return new MicrowaveOven(timeInSeconds, powerLevel, TimeValidatorFactory.CreateManual());
        }

        public static MicrowaveOven CreatePredefined(int timeInSeconds, int powerLevel)
        {
            return new MicrowaveOven(timeInSeconds, powerLevel, TimeValidatorFactory.CreatePredefined());
        }

        public void StartHeating()
        {
        }
    }
}
