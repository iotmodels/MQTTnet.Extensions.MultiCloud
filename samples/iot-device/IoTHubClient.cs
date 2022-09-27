using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iot_device
{
    internal class IoTHubClient
    {
        RequestResponseBinder rr;
        public IoTHubClient(IMqttClient c)
        {
            rr = new RequestResponseBinder(c);
        }

        internal async Task<string> GetTwinAsync(CancellationToken cancellationToken = default) 
            => await rr.GetTwinAsync(cancellationToken);
        
    }
}
