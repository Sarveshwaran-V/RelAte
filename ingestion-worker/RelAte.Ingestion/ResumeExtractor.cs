using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace RelAte.Ingestion;

public record ExtractedResume(
    string FilePath,
    string FileHash,
    string RawText,
    string? Email,
    string? Phone,
    string? LinkedInUrl
);

/// <summary>
/// Resume text & metadata extractor for Sivaram (Track 2A)
/// Supports PDF (PdfPig), DOCX (OpenXml), and TXT documents.
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

    public static string ExtractText(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => ExtractPdfText(filePath),
            ".docx" => ExtractDocxText(filePath),
            ".txt" => File.ReadAllText(filePath),
            _ => throw new NotSupportedException($"Unsupported file format: {ext}")
        };
    }

    public static string ExtractPdfText(string filePath)
    {
        var sb = new StringBuilder();
        using var document = PdfDocument.Open(filePath);
        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString().Trim();
    }

    public static string ExtractDocxText(string filePath)
    {
        var sb = new StringBuilder();
        using var wordDoc = WordprocessingDocument.Open(filePath, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body != null)
        {
            foreach (var para in body.Elements<Paragraph>())
            {
                if (!string.IsNullOrWhiteSpace(para.InnerText))
                {
                    sb.AppendLine(para.InnerText);
                }
            }
        }
        return sb.ToString().Trim();
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

    public static ExtractedResume ProcessResume(string filePath)
    {
        var rawText = ExtractText(filePath);
        var hash = ComputeFileHash(filePath);

        return new ExtractedResume(
            FilePath: filePath,
            FileHash: hash,
            RawText: rawText,
            Email: ExtractEmail(rawText),
            Phone: ExtractPhone(rawText),
            LinkedInUrl: ExtractLinkedInUrl(rawText)
        );
    }
}
