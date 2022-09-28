﻿//  <auto-generated/> 

using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

namespace dtmi_com_example_devicetemplate.mqtt;

public class devicetemplate : Idevicetemplate
{
    public IMqttClient Connection { get; set; }
    public string InitialState { get; set; }
    public IReadOnlyProperty<string> Property_sdkInfo { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public ITelemetry<double> Telemetry_temp { get; set; }
    public ICommand<string, string> Command_echo { get; set; }
    public devicetemplate(IMqttClient c) 
    {
        Connection = c;
        Property_sdkInfo = new ReadOnlyPropertyUTFJson<string>(c, "sdkInfo");
        Property_interval = new WritablePropertyUTFJson<int>(c, "interval");
        Telemetry_temp = new TelemetryUTF8Json<double>(c, "temp");
        Command_echo = new CommandUTF8Json<string, string>(c, "echo");
    }
}
