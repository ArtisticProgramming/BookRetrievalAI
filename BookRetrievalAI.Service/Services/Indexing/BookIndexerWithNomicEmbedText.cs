using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using BookRetrievalAI.Service.Core.Interfaces;
using BookRetrievalAI.Service.Core.Models;
using BookRetrievalAI.Service.Services.Chunking;
using BookRetrievalAI.Service.Services.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookRetrievalAI.Service.Services.Indexing
{
    public class BookIndexerWithNomicEmbedText : IBookIndexer
    {
        private readonly QdrantClient _qdrantClient;
        private readonly ITextEmbeddingGenerationService _embeddingService;
        private readonly string _collectionName;
        private readonly IBookDataParser _dataParser;
        private readonly IBookChunker _bookChunker ;
        public BookIndexerWithNomicEmbedText(QdrantClient qdrantClient, 
                            ITextEmbeddingGenerationService embeddingService, 
                            string collectionName, 
                            IBookDataParser dataParser, 
                            IBookChunker bookChunker)
        {
            _qdrantClient = qdrantClient;
            _embeddingService = embeddingService;
            _collectionName = collectionName;
            _dataParser=dataParser;
            _bookChunker=bookChunker;
        }

        public async Task<bool> CollectionExistsAsync()
        {
            var collections = await _qdrantClient.ListCollectionsAsync();
            return collections.Any(c => c == _collectionName);
        }

        public async Task SetupCollectionAsync()
        {
            try
            {
                await _qdrantClient.CreateCollectionAsync(_collectionName, new VectorParams
                {
                    Size = 768,
                    Distance = Distance.Cosine
                });
                Console.WriteLine($"Collection '{_collectionName}' created.");
            }
            catch
            {
                Console.WriteLine($"Collection '{_collectionName}' already exists.");
            }
        }

        public async Task IndexBooksAsync(string filePath, int batchSize = 50)
        {
            var books = _dataParser.ParseBooksFile(filePath);

            if (books.Count == 0)
            {
                Console.WriteLine("No books to index!");
                return;
            }

            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine("Starting indexing process...");
            Console.WriteLine($"{'=',-60}\n");

            var points = new List<PointStruct>();
            ulong pointId = 1;
            int totalChunks = 0;
            int booksProcessed = 0;

            foreach (BookRecord book in books)
            {
                booksProcessed++;

                List<BookChunk> chunks = _bookChunker.ChunkBook(book);
                totalChunks += chunks.Count;

                foreach (var chunk in chunks)
                {
                    var embedding = await _embeddingService.GenerateEmbeddingAsync("search_document: " + chunk.FullContext);

                    //Qdrant point
                    points.Add(new PointStruct
                    {
                        Id = pointId++,
                        Vectors = embedding.ToArray(),
                        Payload =
                    {
                        ["book_id"] = chunk.BookId,
                        ["title"] = chunk.Title,
                        ["author"] = chunk.Author,
                        ["publication_date"] = chunk.PublicationDate,
                        ["genres"] = JsonSerializer.Serialize(chunk.GenreNames),
                        ["chunk_index"] = chunk.ChunkIndex,
                        ["total_chunks"] = chunk.TotalChunks,
                        ["chunk_text"] = chunk.ChunkText
                    }
                    });

                    // Batch upload for performance
                    if (points.Count >= batchSize)
                    {
                        await _qdrantClient.UpsertAsync(_collectionName, points);
                        Console.WriteLine($"  Indexed {pointId - 1} chunks from {booksProcessed} books...");
                        points.Clear();
                    }
                }
            }

            // Upload remaining points
            if (points.Count > 0)
            {
                await _qdrantClient.UpsertAsync(_collectionName, points);
            }

            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine("✓ Indexing complete!");
            Console.WriteLine($"  Books processed: {booksProcessed}");
            Console.WriteLine($"  Total chunks indexed: {totalChunks}");
            Console.WriteLine($"  Average chunks per book: {(double)totalChunks / booksProcessed:F1}");
            Console.WriteLine($"{'=',-60}\n");
        }

        public async Task<List<SearchResult>> SearchAsync(string query, int limit = 5)
        {
            Console.WriteLine($"\nSearching for: '{query}'");

            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync("search_query: " + query);

            var results = await _qdrantClient.SearchAsync(
                _collectionName,
                queryEmbedding,
                limit: (ulong)limit
            );

            return results.Select(r => new SearchResult
            {
                Title = r.Payload["title"].StringValue,
                Author = r.Payload["author"].StringValue,
                ChunkText = r.Payload["chunk_text"].StringValue,
                ChunkIndex = (int)r.Payload["chunk_index"].IntegerValue,
                TotalChunks = (int)r.Payload["total_chunks"].IntegerValue,
                Score = r.Score
            }).ToList();
        }

    }

 
}
