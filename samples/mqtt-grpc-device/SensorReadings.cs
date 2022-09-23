using System;

namespace mqtt_grpc_device
{
    internal class SensorReadings
    {
        private static readonly Random random = new();
        internal static double GenerateSensorReading(double currentValue, double min, double max)
        {
            double percentage = 15;
            double value = currentValue * (1 + (percentage / 100 * (2 * random.NextDouble() - 1)));
            value = Math.Max(value, min);
            value = Math.Min(value, max);
            return value;
        }
    }
}
