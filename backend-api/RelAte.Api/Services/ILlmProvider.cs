using RelAte.Api.Models;

namespace RelAte.Api.Services;

public interface ILlmProvider
{
    Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null, double temperature = 0.2);
    Task<ParsedJD> ParseJobDescriptionAsync(string rawJdText);
    Task<float[]> GenerateEmbeddingAsync(string text);
}
