using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    internal static class Json
    {
        public static string Stringify(object o) => JsonSerializer.Serialize(o,
             new JsonSerializerOptions()
             {
                 Converters =
                {
                    new JsonStringEnumConverter()
                }
             });
        public static T FromString<T>(string s) => JsonSerializer.Deserialize<T>(s,
            new JsonSerializerOptions()
            {
                Converters =
                    {
                        new JsonStringEnumConverter()
                    }
            })!;
    }
}
