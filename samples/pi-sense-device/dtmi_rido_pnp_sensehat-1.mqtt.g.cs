﻿//  <auto-generated/> 

using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

namespace dtmi_rido_pnp_sensehat.mqtt;

public class sensehat : Isensehat
{
    public IMqttClient Connection { get; set; }
    public string InitialState { get; set; }
    public IReadOnlyProperty<string> Property_piri { get; set; }
    public IReadOnlyProperty<string> Property_ipaddr { get; set; }
    public IReadOnlyProperty<string> Property_sdkInfo { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public IWritableProperty<bool> Property_combineTelemetry { get; set; }
    public ITelemetry<double> Telemetry_t1 { get; set; }
    public ITelemetry<double> Telemetry_t2 { get; set; }
    public ITelemetry<double> Telemetry_h { get; set; }
    public ITelemetry<double> Telemetry_p { get; set; }
    public ITelemetry<double> Telemetry_m { get; set; }

    public ITelemetry<AllTelemetries> AllTelemetries;

    public ICommand<string,string> Command_ChangeLCDColor { get; set; }

    

    internal sensehat(IMqttClient c) 
    {
        Connection = c;
        Property_piri = new ReadOnlyProperty<string>(c, "piri");
        Property_ipaddr = new ReadOnlyProperty<string>(c, "ipaddr");
        Property_sdkInfo = new ReadOnlyProperty<string>(c, "sdkInfo");
        Property_combineTelemetry = new WritableProperty<bool>(c, "combineTelemetry");
        Property_interval = new WritableProperty<int>(c, "interval");
        Telemetry_t1 = new Telemetry<double>(c, "t1");
        Telemetry_t2 = new Telemetry<double>(c, "t2");
        Telemetry_h = new Telemetry<double>(c, "h");
        Telemetry_p = new Telemetry<double>(c, "p");
        Command_ChangeLCDColor = new Command<string,string>(c, "ChangeLCDColor");
        AllTelemetries = new Telemetry<AllTelemetries>(c, String.Empty)
        {
            WrapMessage = false
        };
    }

    public async Task SendTelemetryAsync(AllTelemetries payload, CancellationToken t = default)
    {
        await AllTelemetries.SendMessageAsync(payload, t);
    }
}