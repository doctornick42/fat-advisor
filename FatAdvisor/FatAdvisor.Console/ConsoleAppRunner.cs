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
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are a nutrition and training assistant." +
                "Always consider user’s FatSecret food log.");

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
