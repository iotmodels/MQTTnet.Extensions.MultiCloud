using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace managed_client
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;

        public Device(ILogger<Device> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionSettings cs = new(_configuration.GetConnectionString("cs")!);
            var managedClient = await HubDpsFactory.CreateManagedClientFromConnectionSettingsAsync(cs, 5, stoppingToken);

            managedClient.ConnectedAsync += async c =>
            {
                _logger.LogInformation("Connected: {c}", c.ConnectResult.ResultCode);
                await Task.Yield();
            };
            
            var telemetry = new TelemetryEx<object>(managedClient, cs.DeviceId!);

            int counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                await telemetry.SendMessageAsync(new { counter });
                _logger.LogInformation("PendingMessages: {m}, counter {c}", managedClient.PendingApplicationMessagesCount, counter);
                await Task.Delay(1000, stoppingToken);
                counter++;
            }
        }
    }
}