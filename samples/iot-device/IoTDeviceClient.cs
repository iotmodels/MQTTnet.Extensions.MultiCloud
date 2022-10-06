using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iot_device
{
    internal class IoTDeviceClient
    {
        //public ITelemetry<Telemetries> Temperature;
        public ITelemetry<double> Temperature;
        //public IReadOnlyProperty<string> SdkInfo;
        //public IROProperty<Properties> SdkInfo;
        //public ICommand<int, string> EchoRepeater;
        //public ICommand<echoRequest, echoResponse> EchoRepeater;
        //public IWritableProperty<int> Interval;

        public IoTDeviceClient(IMqttClient c)
        {

        }
    }
}
