using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;

// Build config to read from secrets
var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>()   // <Program> means it will look for UserSecretsId in Program.csproj
    .AddEnvironmentVariables();  // optional: fallback to env vars

var config = builder.Build();

ChatHistory chatHistory = [];
chatHistory.AddUserMessage("Hey, please introduce yourself");

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
//kernelBuilder.Plugins.AddFromType<BookTravelPlugin>("BookTravel");

var kernel = kernelBuilder.Build();

var settings = new AzureOpenAIPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, settings, kernel);

Console.WriteLine(response.Content);
chatHistory.AddMessage(response!.Role, response!.Content!);

//Plugin example
//public class BookTravelPlugin
//{
//    [KernelFunction("book_flight")]
//    [Description("Book travel given location and date")]
//    public async Task<string> BookFlight(DateTime date, string location)
//    {
//        return await Task.FromResult($"Travel was booked to {location} on {date}");
//    }
//}