using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class HubCommandUTF8Json<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
    {
        public HubCommandUTF8Json(IMqttClient client, string name)
        : base(client, name, new UTF8JsonSerializer())
        {
            TopicTemplate = "$iothub/methods/POST/{name}/#";
            ResponseTopic = "$iothub/methods/res/200/?$rid={rid}";
        }
    }
}
