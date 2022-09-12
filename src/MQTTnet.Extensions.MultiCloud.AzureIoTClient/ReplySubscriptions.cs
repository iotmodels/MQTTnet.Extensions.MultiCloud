using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    internal static class ReplySubscriptions
    {
        private static List<string> subscriptions = new List<string>();

        internal static void Add(string topic)
        {
            if (!subscriptions.Contains(topic))
            {
                subscriptions.Add(topic);
            }
        }

        internal static List<string> Get()
        {
            return subscriptions;
        }
    }
}
