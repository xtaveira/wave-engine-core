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
                throw new ArgumentException("Tempo deve estar entre 1 e 120 segundos.");

            if (powerLevel < 1 || powerLevel > 10)
                throw new ArgumentException("Potência deve estar entre 1 e 10.");

            TimeInSeconds = timeInSeconds;
            PowerLevel = powerLevel;
        }

        public string StartHeating()
        {
            return $"Aquecimento iniciado: {TimeInSeconds}s a potência {PowerLevel}.";
        }
    }
}
