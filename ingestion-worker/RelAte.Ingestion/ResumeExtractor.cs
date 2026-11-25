using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace RelAte.Ingestion;

/// <summary>
/// Resume text & metadata extractor for Sivaram (Track 2A)
/// </summary>
public static class ResumeExtractor
{
    private static readonly Regex EmailRegex = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"(?:\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}", RegexOptions.Compiled);
    private static readonly Regex LinkedInRegex = new(@"https?://(?:www\.)?linkedin\.com/in/[\w\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static string? ExtractEmail(string text)
    {
        var match = EmailRegex.Match(text);
        return match.Success ? match.Value : null;
    }

    public static string? ExtractPhone(string text)
    {
        var match = PhoneRegex.Match(text);
        return match.Success ? match.Value : null;
    }

    public static string? ExtractLinkedInUrl(string text)
    {
        var match = LinkedInRegex.Match(text);
        return match.Success ? match.Value : null;
    }
}
