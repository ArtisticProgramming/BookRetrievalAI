using BookRetrievalAI.Service.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddRazorPages();
builder.Services.AddSession();


builder.Services.AddSingleton<BookRagService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<BookRagService>>();
    string _datasetPath = GetDatasetPath(config["DatasetFilePath"]!, sp, config);

    EnsureAnyAIEnabled(config);
   
    return new BookRagService(
        isEnabled: config.GetValue<bool>("AzureOpenAI:Enabled"),
        azureEndpoint: config["AzureOpenAI:Endpoint"]!,
        azureApiKey: config["AzureOpenAI:ApiKey"]!,
        chatDeployment: config["AzureOpenAI:ChatDeployment"]!,
        embeddingDeployment: config["AzureOpenAI:EmbeddingDeployment"]!,
        qdrantEndpoint: new Uri(config["QdrantEndpoint"]!),
        collectionName: config["AzureOpenAI:collectionName"]!,
        dataFilePath: _datasetPath,
        logger);
});

builder.Services.ConfigureHttpClientDefaults(c =>
{
    c.ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(10));
});

builder.Services.AddSingleton<BookRagServiceWithOllamaLocalModels>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<BookRagServiceWithOllamaLocalModels>>();
    string _datasetPath = GetDatasetPath(config["DatasetFilePath"]!, sp, config);

    EnsureAnyAIEnabled(config);

 

    return new BookRagServiceWithOllamaLocalModels(
        isEnabled: config.GetValue<bool>("Ollama:Enabled"),
        ollamaEndpoint: config["Ollama:Endpoint"]!,
        chatModel: config["Ollama:ChatModel"]!,
        embeddingModel: config["Ollama:EmbeddingModel"]!,
        qdrantEndpoint: new Uri(config["QdrantEndpoint"]!),
        collectionName: config["Ollama:collectionName"]!,
        dataFilePath: _datasetPath,
        logger: logger
    );
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/", context =>
{
    context.Response.Redirect("/chat");
    return Task.CompletedTask;
});

app.UseSession();


app.Run();

static string GetDatasetPath(string relativePath,IServiceProvider sp, IConfiguration config)
{
    var env = sp.GetRequiredService<IHostEnvironment>();

    var _datasetPath = Path.Combine(
        env.ContentRootPath,
        relativePath!
    );
    return _datasetPath;
}

static void EnsureAnyAIEnabled(IConfiguration config)
{
    bool azureEnabled = config.GetValue<bool>("AzureOpenAI:Enabled");
    bool ollamaEnabled = config.GetValue<bool>("Ollama:Enabled");

    if (!azureEnabled && !ollamaEnabled)
        throw new Exception("No AI provider is enabled in configuration");
}