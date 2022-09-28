﻿//  <auto-generated/> 

using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;


namespace dtmi_rido_pnp_sensehat.hub;

public class sensehat : HubMqttClient, Isensehat
{
    public IReadOnlyProperty<string> Property_piri { get; set; }
    public IReadOnlyProperty<string> Property_ipaddr { get; set; }
    public IReadOnlyProperty<string> Property_sdkInfo { get; set; }
    public IWritableProperty<bool> Property_combineTelemetry { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public ITelemetry<double> Telemetry_t1 { get; set; }
    public ITelemetry<double> Telemetry_t2 { get; set; }
    public ITelemetry<double> Telemetry_h { get; set; }
    public ITelemetry<double> Telemetry_p { get; set; }
    public ITelemetry<double> Telemetry_m { get; set; }

    public ITelemetry<AllTelemetries> AllTelemetries;

    public ICommand<string, string> Command_ChangeLCDColor { get; set; }

    internal sensehat(IMqttClient c) : base(c)
    {
        Property_piri = new HubReadOnlyPropertyUTFJson<string>(c, "piri");
        Property_ipaddr = new HubReadOnlyPropertyUTFJson<string>(c, "ipaddr");
        Property_sdkInfo = new HubReadOnlyPropertyUTFJson<string>(c, "sdkInfo");
        Property_combineTelemetry = new HubWritablePropertyUTFJson<bool>(c, "combineTelemetry");
        Property_interval = new HubWritablePropertyUTFJson<int>(c, "interval");
        Telemetry_t1 = new HubTelemetryUTF8Json<double>(c, "t1");
        Telemetry_t2 = new HubTelemetryUTF8Json<double>(c, "t2");
        Telemetry_h = new HubTelemetryUTF8Json<double>(c, "h");
        Telemetry_p = new HubTelemetryUTF8Json<double>(c, "p");
        Command_ChangeLCDColor = new HubCommandUTF8Json<string, string>(c, "ChangeLCDColor");
        AllTelemetries = new HubTelemetryUTF8Json<AllTelemetries>(c, String.Empty)
        {
            wrapMessage = false
        };
    }
    public async Task SendTelemetryAsync(AllTelemetries payload, CancellationToken t = default)
    {
        await AllTelemetries.SendMessageAsync(payload, t);
    }
}