using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace aws_sample
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
            ConnectionSettings cs =  new (_configuration.GetConnectionString("cs"));
            var client = await AwsClientFactory.CreateFromConnectionSettingsAsync(cs, stoppingToken);
            Console.WriteLine(client.IsConnected);
            Console.WriteLine(AwsClientFactory.ComputedSettings);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}