using System;

namespace Microwave.Domain
{
    public class MicrowaveOven
    {
        public int TimeInSeconds { get; private set; }
        public int PowerLevel { get; private set; }

        public MicrowaveOven(int timeInSeconds, int powerLevel)
        {
            if (timeInSeconds < 1 || timeInSeconds > 120)
                throw new ArgumentException("Time must be between 1 and 120 seconds.");

            if (powerLevel < 1 || powerLevel > 10)
                throw new ArgumentException("Power level must be between 1 and 10.");

            TimeInSeconds = timeInSeconds;
            PowerLevel = powerLevel;
        }

        public void StartHeating()
        {
        }
    }
}
