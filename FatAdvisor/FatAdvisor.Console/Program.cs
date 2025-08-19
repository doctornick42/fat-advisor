using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using FatAdvisor.FatSecretApi;
using FatAdvisor.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Build config to read from secrets
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>()   // <Program> means it will look for UserSecretsId in Program.csproj
            .AddEnvironmentVariables();  // optional: fallback to env vars

        var config = builder.Build();

        var profileTokenStorage = new ProfileTokenStorage();
        
        var apiClient = new FatSecretApiClient(
            consumerKey: config["FatSecret:ConsumerKey"],
            consumerSecret: config["FatSecret:ConsumerSecret"],
            accessToken: null,
            accessTokenSecret: null);

        var oauth = new FatSecretOAuth1(
            http: new HttpClient(),
            consumerKey: config["FatSecret:ConsumerKey"],
            consumerSecret: config["FatSecret:ConsumerSecret"]);

        var dataPlugin = new FatSecretProfileDataPlugin(apiClient, oauth, profileTokenStorage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage("Hey, please analyze my food for today and give me recommendation what to eat today for a dinner?");
        chatHistory.AddUserMessage("Consider that I had a cardio/core training session today in gym");

        var apiKey = config["GitHubModels:ApiKey"];
        var endpoint = config["GitHubModels:Endpoint"];
        var modelId = "gpt-4o-mini";

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: modelId,
            apiKey: apiKey,
            endpoint: new Uri(endpoint)
        );

        //Plugin registration example
        kernelBuilder.Plugins.AddFromObject(dataPlugin);

        kernelBuilder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        var kernel = kernelBuilder.Build();

        var settings = new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, settings, kernel);

        Console.WriteLine(response.Content);

        foreach (var kvp in response.Metadata)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        chatHistory.AddMessage(response!.Role, response!.Content!);
    }
}