using BookRetrievalAI.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Threading.Tasks;
public class ChatMessage
{
    public string Role { get; set; } // "user" or "assistant"
    public string Content { get; set; }
}
public class ChatModel : PageModel
{
    private readonly BookRagServiceWithOllamaLocalModels _bookRagServiceWithOllamaLocalModels;
    private readonly BookRagService _bookRagService;
    private readonly IConfiguration _config;

    public ChatModel(BookRagServiceWithOllamaLocalModels bookRagServiceWithOllamaLocalModels, BookRagService bookRagService, IConfiguration config)
    {
        _bookRagServiceWithOllamaLocalModels=bookRagServiceWithOllamaLocalModels;
        _bookRagService=bookRagService;
        _config=config;

    }

    [BindProperty]
    public string UserInput { get; set; }

    public List<ChatMessage> Messages { get; set; } = new();

    public void OnGet()
    {
        LoadMessages();
    }

    public async Task<IActionResult> OnPost()
    {
        LoadMessages();

        Messages.Add(new ChatMessage
        {
            Role = "user",
            Content = UserInput
        });

        string response = await CallRagServiceAsync(UserInput);

        Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = response
        });

        SaveMessages();

        // 🔥 THIS LINE CLEARS TEXTBOX
        UserInput = string.Empty;

        ModelState.Clear(); // important!

        return Page();
    }
    private void LoadMessages()
    {
        var sessionData = HttpContext.Session.GetString("ChatHistory");
        if (sessionData != null)
        {
            Messages = JsonSerializer.Deserialize<List<ChatMessage>>(sessionData);
        }
    }

    private void SaveMessages()
    {
        HttpContext.Session.SetString(
            "ChatHistory",
            JsonSerializer.Serialize(Messages));
    }

    //private async Task<string> CallRagService(string question)
    //{
    //    return await _bookRagService.AskAsync(UserInput);
    //}


    private async Task<string> CallRagServiceAsync(string question)
    {
        bool azureEnabled = _config.GetValue<bool>("AzureOpenAI:Enabled");
        bool ollamaEnabled = _config.GetValue<bool>("Ollama:Enabled");

        if (azureEnabled && _bookRagService != null)
        {
            return await _bookRagService.AskAsync(question);
        }

        if (ollamaEnabled && _bookRagServiceWithOllamaLocalModels != null)
        {
            return await _bookRagServiceWithOllamaLocalModels.AskAsync(question);
        }

        throw new Exception("No AI provider is enabled or services are not configured.");
    }
    public IActionResult OnPostReset()
    {
        // Clear the chat session
        HttpContext.Session.Remove("ChatHistory");

        // Clear the bound property too
        UserInput = string.Empty;
        Messages.Clear();

        // Optionally clear ModelState
        ModelState.Clear();

        // Reload the page
        return Page();
    }
}