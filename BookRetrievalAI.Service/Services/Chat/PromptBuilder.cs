using Microsoft.SemanticKernel.ChatCompletion;
using BookRetrievalAI.Service.Core.Interfaces;

namespace BookRetrievalAI.Service.Services.Chat
{
    public class PromptBuilder : IPromptBuilder
    {
        private const string SystemPrompt =
            "You are a helpful book recommendation assistant. " +
            "Use the provided book information to answer the user's question. " +
            "Be conversational, accurate, and helpful. " +
            "If the information doesn't fully answer the question, acknowledge this politely. " +
            "Always cite specific book titles when making recommendations.";

        public ChatHistory BuildPrompt(string context, string userQuery)
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(SystemPrompt);

            chatHistory.AddUserMessage($@"
Context from book database:
{context}

User question: {userQuery}

Please provide a helpful answer based on the context above.");

            return chatHistory;
        }
    }
}