using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace iothub_sample
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //using (var consoleListener = new ConsoleTraceListener())
            //{
            //    Trace.Listeners.Add(consoleListener);
            //}

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Device>();
                });
    }
}
