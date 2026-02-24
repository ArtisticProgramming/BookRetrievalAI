using Microsoft.SemanticKernel.ChatCompletion;

namespace BookRetrievalAI.Service.Core.Interfaces
{
    public interface IPromptBuilder
    {
        ChatHistory BuildPrompt(string context, string userQuery);
    }
}