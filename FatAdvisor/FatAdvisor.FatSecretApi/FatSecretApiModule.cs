using Autofac;
using FatAdvisor.Ai;
using FatAdvisor.FatSecretAi;
using Microsoft.Extensions.Configuration;

namespace FatAdvisor.FatSecretApi
{
    public class FatSecretApiModule : Module
    {
        private readonly IConfiguration _config;

        public FatSecretApiModule(IConfiguration config) => _config = config;

        protected override void Load(ContainerBuilder builder)
        {
            var consumerKey = _config["FatSecret:ConsumerKey"];
            var consumerSecret = _config["FatSecret:ConsumerSecret"];

            builder.RegisterType<ProfileTokenStorage>()
                   .As<IProfileTokenStorage>()
                   .SingleInstance();

            builder.Register(ctx =>
            {
                var httpClientFactory = ctx.Resolve<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("FatSecretApiClient");

                return new FatSecretOAuth1(
                    http: httpClient,
                    consumerKey: consumerKey,
                    consumerSecret: consumerSecret);
            })
            .As<IFatSecretOAuth>()
            .SingleInstance();

            builder.Register(ctx => 
            {
                var httpClientFactory = ctx.Resolve<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("FatSecretApiClient");

                return new FatSecretApiClient(
                    httpClient,
                    consumerKey: consumerKey,
                    consumerSecret: consumerSecret,
                    accessToken: null,
                    accessTokenSecret: null);
            })
            .As<IFatSecretApiClient>()
            .SingleInstance();

            builder.RegisterType<ProfileTokenStorage>().As<IProfileTokenStorage>().SingleInstance();
        }
    }
}
