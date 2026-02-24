using BookRetrievalAI.Service.Core.Interfaces;
using BookRetrievalAI.Service.Core.Models;
using System.Text;

namespace BookRetrievalAI.Service.Services.Chat
{
    public class ContextBuilder : IContextBuilder
    {
        public string BuildContext(List<SearchResult> searchResults)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < searchResults.Count; i++)
            {
                sb.AppendLine($"[Book {i + 1}]");
                sb.AppendLine($"Title: {searchResults[i].Title}");
                sb.AppendLine($"Author: {searchResults[i].Author}");
                sb.AppendLine($"Summary: {searchResults[i].ChunkText}");
                sb.AppendLine($"Relevance Score: {searchResults[i].Score:F4}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}