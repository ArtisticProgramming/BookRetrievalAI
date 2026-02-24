using Microsoft.SemanticKernel.ChatCompletion;
using BookRetrievalAI.Service.Core.Interfaces;
using System.Text;

namespace BookRetrievalAI.Service.Services.Chat
{
    public class RagChatService : IRagChatService
    {
        private readonly IBookIndexer _indexer;
        private readonly IChatCompletionService _chatService;
        private readonly IContextBuilder _contextBuilder;
        private readonly IPromptBuilder _promptBuilder;

        public RagChatService(
            IBookIndexer indexer,
            IChatCompletionService chatService,
            IContextBuilder contextBuilder,
            IPromptBuilder promptBuilder)
        {
            _indexer = indexer;
            _chatService = chatService;
            _contextBuilder = contextBuilder;
            _promptBuilder = promptBuilder;
        }

        public async Task<string> AskAsync(string userQuery, int retrievalLimit = 10)
        {
            var searchResults = await _indexer.SearchAsync(userQuery, retrievalLimit);

            if (searchResults.Count == 0)
            {
                return "I couldn't find any relevant books in my database for your query.";
            }

            var context = _contextBuilder.BuildContext(searchResults);

            var chatHistory = _promptBuilder.BuildPrompt(context, userQuery);

            var response = await _chatService.GetChatMessageContentAsync(chatHistory);

            //await foreach (var content in _chatService.GetStreamingChatMessageContentsAsync(chatHistory))
            //{
            //    Console.Write(content.Content);
            //    response+=content.Content;
            //}

            return response.Content ?? "I apologize, but I couldn't generate a response.";
        }
    }
}