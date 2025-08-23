using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace FatAdvisor.Ai
{
    public class FatSecretAiModule : Module
    {
        private readonly IConfiguration _config;

        public FatSecretAiModule(IConfiguration config) => _config = config;

        protected override void Load(ContainerBuilder builder)
        {
            var apiKey = _config["GitHubModels:ApiKey"];
            var endpoint = _config["GitHubModels:Endpoint"];
            var modelId = "gpt-4o-mini";

            builder.Register(ctx =>
            {
                var loggerFactory = ctx.Resolve<ILoggerFactory>();

                var kernelBuilder = Kernel.CreateBuilder();

                kernelBuilder.Services.AddSingleton(loggerFactory);

                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey,
                    endpoint: new Uri(endpoint));

                // Register plugins dynamically
                var fatSecretPlugin = ctx.Resolve<FatSecretProfileDataPlugin>();
                kernelBuilder.Plugins.AddFromObject(fatSecretPlugin);

                return kernelBuilder.Build();
            }).As<Kernel>()
              .SingleInstance();

            builder.RegisterType<FatSecretProfileDataPlugin>().AsSelf().SingleInstance();
        }
    }
}
