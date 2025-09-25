using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Core.Storage;
using OpenAI.Chat;
using Spectre.Console;

namespace DevExLead.Modules.AI.Handlers
{
    internal class ChatHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var prompt = ParameterHelper.ReadStringParameter(options, "prompt");
            var openAiKey = UserStorageManager.GetDecryptedValue("OpenAI:Key");
            ChatClient client = new(model: "gpt-4o", openAiKey);

            // Create chat messages
            var messages = new List<ChatMessage>
                                            {
                                                ChatMessage.CreateUserMessage(prompt)
                                            };

            // Set the temperature to 0 for deterministic responses
            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0f
            };

            // Complete the chat with the specified options
            ChatCompletion completion = client.CompleteChat(messages, chatCompletionOptions);

            // Return the assistant's response
            AnsiConsole.WriteLine(completion.Content[0].Text);
        }
    }
}

