using BookRetrievalAI.Service.Core.Interfaces;
using BookRetrievalAI.Service.Services.Chat;
using BookRetrievalAI.Service.Services.Chunking;
using BookRetrievalAI.Service.Services.Indexing;
using BookRetrievalAI.Service.Services.Parsing;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;

namespace BookRetrievalAI.Service.Services
{
    public class BookRagServiceWithOllamaLocalModels
    {
        private readonly IRagChatService _ragChatService;
        private readonly IBookIndexer _indexer;
        private readonly ILogger<BookRagServiceWithOllamaLocalModels> _logger;

        public BookRagServiceWithOllamaLocalModels(
            bool isEnabled,
            string ollamaEndpoint,       
            string chatModel,            
            string embeddingModel,       
            Uri qdrantEndpoint,
            string collectionName,
            string dataFilePath,
            ILogger<BookRagServiceWithOllamaLocalModels> logger)
        {
            _logger = logger;

            try
            {
                if (!isEnabled)
                {
                    logger.LogWarning("Azure is enabled. Skipping Ollama local model service registration.");
                    return;
                }

                _logger.LogInformation("Initializing BookRagService with local Ollama models");

                // Build Kernel with Ollama
                _logger.LogInformation("Building Kernel with Ollama at {Endpoint}", ollamaEndpoint);
                var ollamaUri = new Uri(ollamaEndpoint);

                var builder = Kernel.CreateBuilder();
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(10) 
                };

                builder.AddOllamaChatCompletion(
                    modelId: chatModel,
                    endpoint: ollamaUri);

                builder.AddOllamaTextEmbeddingGeneration(
                    modelId: embeddingModel,
                    endpoint: ollamaUri);

                var kernel = builder.Build();

                _logger.LogInformation("Resolving Kernel services");
                ITextEmbeddingGenerationService embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();

                _logger.LogInformation("Initializing parser and chunker");
                IBookDataParser dataParser = new BookDataParser();
                IBookChunker bookChunker = new BookChunkerWithNomicEmbedText();

                _logger.LogInformation("Connecting to Qdrant at {Endpoint}", qdrantEndpoint);
                var qdrantClient = new QdrantClient(qdrantEndpoint);

                _logger.LogInformation("Creating BookIndexer for collection {Collection}", collectionName);
                _indexer = new BookIndexerWithNomicEmbedText(
                    qdrantClient,
                    embeddingService,
                    collectionName,
                    dataParser,
                    bookChunker);

                _logger.LogInformation("Building chat orchestration pipeline");
                IContextBuilder contextBuilder = new ContextBuilder();
                IPromptBuilder promptBuilder = new PromptBuilder();

                _ragChatService = new RagChatService(
                    _indexer,
                    chatService,
                    contextBuilder,
                    promptBuilder);

                _logger.LogInformation("Starting book dataset indexing pipeline");
                Task.Run(async () =>
                {
                    if (!await _indexer.CollectionExistsAsync())
                    {
                        await _indexer.SetupCollectionAsync();
                        _logger.LogInformation("Collection does not exist. Starting indexing from {File}", dataFilePath);
                        await _indexer.IndexBooksAsync(dataFilePath);
                        _logger.LogInformation("Indexing completed successfully");
                    }
                    else
                    {
                        _logger.LogInformation("Collection already exists. Skipping indexing");
                    }
                }).Wait();

                _logger.LogInformation("BookRagService initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing BookRagService");
                throw;
            }
        }

        public async Task<string> AskAsync(string question, int retrievalLimit = 5)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                _logger.LogWarning("User sent empty question");
                throw new ArgumentException("Question cannot be empty", nameof(question));
            }

            _logger.LogInformation("Processing question: {Question}", question);

            try
            {
                var result = await _ragChatService.AskAsync(question, retrievalLimit);
                _logger.LogInformation(
                    "Question processed successfully. RetrievalLimit={Limit}",
                    retrievalLimit);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing question");
                throw;
            }
        }
    }
}