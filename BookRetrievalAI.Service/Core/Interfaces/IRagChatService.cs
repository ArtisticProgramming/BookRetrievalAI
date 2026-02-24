namespace BookRetrievalAI.Service.Core.Interfaces
{
     public interface IRagChatService
    {
          Task<string> AskAsync(string userQuery, int retrievalLimit = 5);
    }
}