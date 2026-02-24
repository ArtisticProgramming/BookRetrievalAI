using BookRetrievalAI.Service.Core.Interfaces;
using BookRetrievalAI.Service.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Services.Chunking
{
    public class BookChunker : IBookChunker
    {
        private const int SentencesPerChunk = 4;  
        private const int OverlapSentences = 1; 

        public List<BookChunk> ChunkBook(BookRecord book)
        {
            var chunks = new List<BookChunk>();

            var sentences = SplitIntoSentences(book.Summary);

            if (sentences.Count == 0)
            {
                Console.WriteLine($"Warning: No sentences found for book '{book.Title}'");
                return chunks;
            }

            var genreNames = book.Genres.Values.ToList();
            int chunkIndex = 0;

            for (int i = 0; i < sentences.Count; i += SentencesPerChunk - OverlapSentences)
            {
                var chunkSentences = sentences
                    .Skip(i)
                    .Take(SentencesPerChunk)
                    .ToList();

                if (chunkSentences.Count == 0) 
                    break;

                var chunkText = string.Join(" ", chunkSentences);

                var context = BuildContextForEmbedding(book, chunkText, genreNames);

                chunks.Add(new BookChunk
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    PublicationDate = book.PublicationDate,
                    GenreNames = genreNames,
                    ChunkIndex = chunkIndex,
                    TotalChunks = -1,
                    ChunkText = chunkText,
                    FullContext = context
                });

                chunkIndex++;
            }

            foreach (var chunk in chunks)
            {
                chunk.TotalChunks = chunks.Count;
            }

            return chunks;
        }

        private static string BuildContextForEmbedding(BookRecord book, string chunkText, List<string> genres)
        {
            return $"Book: {book.Title}\n" +
                   $"Author: {book.Author}\n" +
                   $"Published: {book.PublicationDate}\n" +
                   $"Genres: {string.Join(", ", genres)}\n\n" +
                   $"{chunkText}";
        }

        private static List<string> SplitIntoSentences(string text)
        {
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            return sentences;
        }
    }
}
