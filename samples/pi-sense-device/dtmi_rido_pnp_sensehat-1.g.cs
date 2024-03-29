﻿//  <auto-generated/> 


using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;

namespace dtmi_rido_pnp_sensehat;

public interface Isensehat 
{
    public const string ModelId = "dtmi:rido:pnp:sensehat;1";
    public IMqttClient Connection { get; }
    public string InitialState { get; set; }
    
    public IReadOnlyProperty<string> Property_piri { get; set; }
    public IReadOnlyProperty<string> Property_ipaddr { get; set; }
    public IReadOnlyProperty<string> Property_sdkInfo { get; set; }
    
    public ICommand<string, string> Command_ChangeLCDColor { get; set; }
    
    public IWritableProperty<int> Property_interval { get; set; }
    public IWritableProperty<bool> Property_combineTelemetry { get; set; }
    
    public ITelemetry<double> Telemetry_t1 { get; set; }
    public ITelemetry<double> Telemetry_t2 { get; set; }
    public ITelemetry<double> Telemetry_h { get; set; }
    public ITelemetry<double> Telemetry_p { get; set; }
    public ITelemetry<double> Telemetry_m { get; set; }

    public Task SendTelemetryAsync(AllTelemetries payload, CancellationToken t = default);
}


public class AllTelemetries
{
    public double t1 { get; set; }
    public double t2 { get; set; }
    public double h { get; set; }
    public double p { get; set; }
    public double m { get; set; }
}

