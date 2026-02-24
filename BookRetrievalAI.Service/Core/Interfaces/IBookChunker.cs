using BookRetrievalAI.Service.Core.Models;

namespace BookRetrievalAI.Service.Core.Interfaces
{
    public interface IBookChunker
    {
        List<BookChunk> ChunkBook(BookRecord book);
    }
}
