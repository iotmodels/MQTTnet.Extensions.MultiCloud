﻿//  <auto-generated/> 


using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;

namespace dtmi_rido_pnp_memmon;

public interface Imemmon 
{
    public const string ModelId = "dtmi:rido:pnp:memmon;1";
    public IMqttClient Connection { get; }
    public string InitialState { get; }

    public IReadOnlyProperty<DateTime> Property_started { get; set; }
    public IWritableProperty<bool> Property_enabled { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public ITelemetry<double> Telemetry_workingSet { get; set; }
    public ICommand<DiagnosticsMode, Dictionary<string, string>> Command_getRuntimeStats { get; set; }
}

public enum DiagnosticsMode
{
    minimal = 0,
    complete = 1,
    full = 2
}
