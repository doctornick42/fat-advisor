using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FatAdvisor.Console
{
    public class ConsoleAppRunner
    {
        private readonly Kernel _kernel;

        public ConsoleAppRunner(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task RunAsync()
        {
            var yesterday = DateTime.Now.AddDays(-1);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are a nutrition and training assistant. " +
                "When answering the user, always consider user's FatSecret profile data including: " +
                $"FatSecret food log for the last week (check it for every day in the range of last 3 days until {yesterday}. " +
                "If available, consider user's weight diary (for the last month) to understand what " +
                "amount of food should be taken. In your response display weight diary and consumed food diary.");

            System.Console.WriteLine("Please enter your question:");
            var userMessage = System.Console.ReadLine();

            chatHistory.AddUserMessage(userMessage);

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

            var settings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, settings, _kernel);

            System.Console.WriteLine(response.Content);

            foreach (var kvp in response.Metadata)
                System.Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
}
