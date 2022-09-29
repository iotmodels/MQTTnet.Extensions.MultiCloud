using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace payload_size.json;

public class Properties
{
    [JsonPropertyName("sdkInfo")]
    public string? SdkInfo { get; set; }
    [JsonPropertyName("started")]
    public DateTime Started { get; set; }
}
