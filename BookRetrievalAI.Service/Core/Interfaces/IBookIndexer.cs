using BookRetrievalAI.Service.Core.Models;
using BookRetrievalAI.Service.Services.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Core.Interfaces
{
    public interface IBookIndexer
    {
        Task<bool> CollectionExistsAsync();

        Task SetupCollectionAsync();
        Task IndexBooksAsync(string filePath, int batchSize = 50);
        Task<List<SearchResult>> SearchAsync(string query, int limit = 5);
    }
}
