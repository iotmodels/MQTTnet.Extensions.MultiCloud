using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;

namespace mqtt_commands
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;
        public Device(ILogger<Device> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IMqttClient connection = await BrokerClientFactory.CreateFromConnectionSettingsAsync(_configuration.GetConnectionString("Broker")!, false, stoppingToken);
            _logger.LogWarning("Connected to {cs}", BrokerClientFactory.ComputedSettings);
            GenericCommand cmd = new GenericCommand(connection);
            cmd.OnCmdDelegate += req =>
            {
                _logger.LogInformation($"Received command {req.CommandName} with payload {req.CommandPayload}");
                return new GenericCommandResponse() { Status = 200, ReponsePayload = $"{req.CommandPayload} {req.CommandPayload}" };
            };

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }
}