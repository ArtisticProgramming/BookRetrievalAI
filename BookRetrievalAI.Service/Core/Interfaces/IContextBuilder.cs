using BookRetrievalAI.Service.Core.Models;

namespace BookRetrievalAI.Service.Core.Interfaces
{
    public interface IContextBuilder
    {
        string BuildContext(List<SearchResult> searchResults);
    }
}