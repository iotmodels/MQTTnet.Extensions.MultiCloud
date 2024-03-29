﻿//  <auto-generated/> 

using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;


namespace dtmi_rido_memmon.mqtt;

public class _memmon : Imemmon
{
    public IMqttClient Connection { get; set; }
    public string InitialState { get; set; }
    public IReadOnlyProperty<DateTime> Property_started { get; set; }
    public IReadOnlyProperty<int> Property_timesRestarted { get; set; }
    public IWritableProperty<bool> Property_enabled { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public ITelemetry<double> Telemetry_workingSet { get; set; }
    public ITelemetry<double> Telemetry_managedMemory { get; set; }
    public ICommand<DiagnosticsMode, Dictionary<string, string>> Command_getRuntimeStats { get; set; }
    public ICommand<int, bool> Command_isPrime { get; set; }
    public ICommand<int> Command_malloc { get; set; }
    public ICommand Command_free { get; set; }

    internal _memmon(IMqttClient c) 
    {
        Connection = c;
        Property_started = new ReadOnlyProperty<DateTime>(c, "started");
        Property_timesRestarted = new ReadOnlyProperty<int>(c, "timesRestarted");
        Property_interval = new WritableProperty<int>(c, "interval");
        Property_enabled = new WritableProperty<bool>(c, "enabled");
        Telemetry_workingSet = new Telemetry<double>(c, "workingSet");
        Telemetry_managedMemory = new Telemetry<double>(c, "managedMemory");
        Command_getRuntimeStats = new Command<DiagnosticsMode, Dictionary<string, string>>(c, "getRuntimeStats");
        Command_isPrime = new Command<int, bool>(c, "isPrime");
        Command_malloc = new Command<int>(c, "malloc");
        Command_free = new Command(c, "free");
    }
}