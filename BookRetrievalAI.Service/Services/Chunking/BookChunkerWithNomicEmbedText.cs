using BookRetrievalAI.Service.Core.Interfaces;
using BookRetrievalAI.Service.Core.Models;
using System.Text.RegularExpressions;

public class BookChunkerWithNomicEmbedText : IBookChunker
{
    private const int SentencesPerChunk = 10;
    private const int OverlapSentences = 3;

    public List<BookChunk> ChunkBook(BookRecord book)
    {
        var chunks = new List<BookChunk>();
        var sentences = SplitIntoSentences(book.Summary);

        if (sentences.Count == 0)
        {
            Console.WriteLine($"Warning: No sentences found for book '{book.Title}'");
            return chunks;
        }

        if (sentences.Count <= SentencesPerChunk)
        {
            var fullText = string.Join(" ", sentences);
            chunks.Add(CreateChunk(book, fullText, 0));
            chunks[0].TotalChunks = 1;
            return chunks;
        }

        int chunkIndex = 0;
        int step = SentencesPerChunk - OverlapSentences;

        for (int i = 0; i < sentences.Count; i += step)
        {
            var chunkSentences = sentences.Skip(i).Take(SentencesPerChunk).ToList();
            if (chunkSentences.Count < 2) 
                break;

            var chunkText = string.Join(" ", chunkSentences);
            chunks.Add(CreateChunk(book, chunkText, chunkIndex++));
        }

        foreach (var chunk in chunks)
            chunk.TotalChunks = chunks.Count;

        return chunks;
    }

    private static BookChunk CreateChunk(BookRecord book, string chunkText, int chunkIndex)
    {
        var genreNames = book.Genres.Values.ToList();
        return new BookChunk
        {
            BookId = book.Id,
            Title = book.Title,
            Author = book.Author,
            PublicationDate = book.PublicationDate,
            GenreNames = genreNames,
            ChunkIndex = chunkIndex,
            TotalChunks = -1,
            ChunkText = chunkText,
            FullContext = BuildContextForEmbedding(book, chunkText, genreNames)
        };
    }

    private static string BuildContextForEmbedding(BookRecord book, string chunkText, List<string> genres)
    {
        return $"{book.Title} by {book.Author}, published {book.PublicationDate}, " +
               $"a {string.Join(", ", genres)} book. {chunkText}";
    }

    private static List<string> SplitIntoSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        return Regex.Split(text, @"(?<=[.!?])\s+")
            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length > 10)
            .Select(s => s.Trim())
            .ToList();
    }
}