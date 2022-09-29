using System.Diagnostics;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    [DebuggerStepThrough()]
    internal static class RidCounter
    {
        private static int counter = 0;
        internal static int Current => counter;
        internal static int NextValue() => Interlocked.Increment(ref counter);
        internal static void Reset() => counter = 0;
    }
}
