using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class PropertyInitializer
    {
        public static async Task InitPropertyAsync<T>(IWritableProperty<T> prop, T defaultValue)
        {
            if (prop.Version == null)
            {
                prop.Value = defaultValue;
                prop.Version = 0;
                await prop.SendMessageAsync(new Ack<T>() { Status = 0, Value = defaultValue, Description = "init default value" });
            }
        }
    }
}
