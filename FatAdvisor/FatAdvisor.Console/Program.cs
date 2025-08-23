using Microsoft.Extensions.Configuration;
using FatAdvisor.FatSecretApi;
using FatAdvisor.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace FatAdvisor.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddUserSecrets<Program>();
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddHttpClient("FatSecretApiClient", client =>
                        {
                            client.BaseAddress = new Uri("https://www.fatsecret.com/");
                            client.Timeout = TimeSpan.FromSeconds(300);
                        });
                    })
                    .ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        // Register your modules here
                        builder.RegisterModule(new FatSecretAiModule(context.Configuration));
                        builder.RegisterModule(new FatSecretApiModule(context.Configuration));

                        builder.RegisterType<ConsoleAppRunner>().AsSelf();
                    })
                    .Build();

            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<ConsoleAppRunner>();
            await app.RunAsync();
        }
    }
}