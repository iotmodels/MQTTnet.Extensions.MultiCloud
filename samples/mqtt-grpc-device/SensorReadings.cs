using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mqtt_grpc_device
{
    internal class SensorReadings
    {
        static readonly Random random = new();
        static internal double GenerateSensorReading(double currentValue, double min, double max)
        {
            double percentage = 15;
            double value = currentValue * (1 + (percentage / 100 * (2 * random.NextDouble() - 1)));
            value = Math.Max(value, min);
            value = Math.Min(value, max);
            return value;
        }
    }
}
