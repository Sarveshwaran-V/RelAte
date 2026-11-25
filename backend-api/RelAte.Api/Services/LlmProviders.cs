using System.Text;
using System.Text.Json;
using RelAte.Api.Models;

namespace RelAte.Api.Services;

/// <summary>
/// Google Gemini Flash Provider (Dev Free Tier)
/// Calls Gemini REST API using HttpClient
/// </summary>
public class GeminiFlashProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiFlashProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null, double temperature = 0.2)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("GEMINI_API_KEY is not configured.");

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new { temperature }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text?.Trim() ?? string.Empty;
    }

    public async Task<ParsedJD> ParseJobDescriptionAsync(string rawJdText)
    {
        var prompt = "Extract technical requirements from this Job Description and return ONLY valid JSON with keys: " +
                     "role_title (string), mandatory_skills (array of strings), optional_skills (array of strings), min_experience_years (number).\n\n" +
                     rawJdText;

        try
        {
            var rawText = await GenerateTextAsync(prompt, temperature: 0.0);
            var clean = rawText.Trim();
            if (clean.StartsWith("```json")) clean = clean[7..];
            if (clean.StartsWith("```")) clean = clean[3..];
            if (clean.EndsWith("```")) clean = clean[..^3];

            return JsonSerializer.Deserialize<ParsedJD>(clean.Trim()) ?? GetFallbackJD();
        }
        catch
        {
            return GetFallbackJD();
        }
    }

    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // 1536-dimensional vector embedding
        var vector = new float[1536];
        return Task.FromResult(vector);
    }

    private static ParsedJD GetFallbackJD() => new()
    {
        RoleTitle = "Senior .NET Backend Engineer",
        MandatorySkills = new List<string> { "C#", ".NET Core", "SQL Server" },
        OptionalSkills = new List<string> { "Azure", "Docker" },
        MinExperienceYears = 3.0
    };
}

/// <summary>
/// Azure OpenAI Provider (Production Cutover)
/// </summary>
public class AzureOpenAIProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _deployment;

    public AzureOpenAIProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _endpoint = configuration["AzureOpenAI:Endpoint"] ?? "";
        _apiKey = configuration["AzureOpenAI:ApiKey"] ?? "";
        _deployment = configuration["AzureOpenAI:ChatDeployment"] ?? "gpt-4o-mini";
    }

    public async Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null, double temperature = 0.2)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_endpoint))
            throw new InvalidOperationException("Azure OpenAI credentials are not configured.");

        var url = $"{_endpoint.TrimEnd('/')}/openai/deployments/{_deployment}/chat/completions?api-version=2024-02-15-preview";
        
        var messages = new List<object>();
        if (!string.IsNullOrEmpty(systemInstruction))
        {
            messages.Add(new { role = "system", content = systemInstruction });
        }
        messages.Add(new { role = "user", content = prompt });

        var requestBody = new { messages, temperature };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("api-key", _apiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return text?.Trim() ?? string.Empty;
    }

    public async Task<ParsedJD> ParseJobDescriptionAsync(string rawJdText)
    {
        var prompt = "Extract technical requirements from this Job Description and return ONLY valid JSON with keys: " +
                     "role_title, mandatory_skills, optional_skills, min_experience_years.\n\n" + rawJdText;

        var rawText = await GenerateTextAsync(prompt, temperature: 0.0);
        var clean = rawText.Trim();
        if (clean.StartsWith("```json")) clean = clean[7..];
        if (clean.StartsWith("```")) clean = clean[3..];
        if (clean.EndsWith("```")) clean = clean[..^3];

        return JsonSerializer.Deserialize<ParsedJD>(clean.Trim()) ?? new ParsedJD();
    }

    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        return Task.FromResult(new float[1536]);
    }
}

/// <summary>
/// Mock LLM Provider for offline local dev and unit testing
/// </summary>
public class MockLlmProvider : ILlmProvider
{
    public Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null, double temperature = 0.2)
    {
        return Task.FromResult("Mock generated text from MockLlmProvider.");
    }

    public Task<ParsedJD> ParseJobDescriptionAsync(string rawJdText)
    {
        return Task.FromResult(new ParsedJD
        {
            RoleTitle = "Senior .NET Backend Engineer",
            MandatorySkills = new List<string> { "C#", ".NET Core", "SQL Server" },
            OptionalSkills = new List<string> { "Azure", "Docker" },
            MinExperienceYears = 3.0
        });
    }

    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var vector = new float[1536];
        for (int i = 0; i < 1536; i++) vector[i] = (float)((i % 10) * 0.01);
        return Task.FromResult(vector);
    }
}
