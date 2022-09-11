using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Clients.Connections
{
    public static class Json
    {
        public static string Stringify(object o) => JsonSerializer.Serialize(o,
             new JsonSerializerOptions()
             {
                 Converters =
                {
                    new JsonStringEnumConverter()
                }
             });
    }
}
