using RelAte.Api.Services;
using RelAte.Ingestion;

Console.WriteLine("=================================================================");
Console.WriteLine("RelAte Ingestion Worker (.NET 10)");
Console.WriteLine("Tracks 2A (Sivaram - Extractor) & 2B (Pradeep - Normalizer/Embed)");
Console.WriteLine("=================================================================");

var samplesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "golden_samples"));
Console.WriteLine($"\nScanning Golden Samples Directory:\n{samplesDir}\n");

if (!Directory.Exists(samplesDir))
{
    Directory.CreateDirectory(samplesDir);
}

// Find all supported resume documents
var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".txt" };
var files = Directory.GetFiles(samplesDir)
    .Where(f => supportedExtensions.Contains(Path.GetExtension(f)))
    .ToList();

Console.WriteLine($"Found {files.Count} resume file(s) for ingestion.");

// Initialize LLM provider & services (Mock or Gemini/Azure based on environment)
ILlmProvider llmProvider = new MockLlmProvider();
var normalizer = new SkillNormalizer(llmProvider);
var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING") ?? "";
var embedder = new VectorEmbedder(llmProvider, connectionString);

foreach (var file in files)
{
    Console.WriteLine($"\n--- Processing: {Path.GetFileName(file)} ---");
    try
    {
        // Step 1: Text & Contact Extraction (Track 2A)
        var extracted = ResumeExtractor.ProcessResume(file);
        Console.WriteLine($"[Extracted] Hash: {extracted.FileHash[..12]}... | Email: {extracted.Email ?? "N/A"} | LinkedIn: {extracted.LinkedInUrl ?? "N/A"}");

        // Step 2: Canonical Skill Normalization (Track 2B)
        var normalized = await normalizer.NormalizeSkillsAsync(extracted.RawText);
        Console.WriteLine($"[Normalized] Candidate: {normalized.FullName} | Role: {normalized.CurrentRole} | Exp: {normalized.TotalExperienceYears}y");
        Console.WriteLine($"[Skills] {string.Join(", ", normalized.Skills.Select(s => s.CanonicalSkill))}");

        // Step 3: Embed & Upsert into Azure SQL (Track 2B)
        var embedding = await embedder.GenerateEmbeddingAsync(normalized);
        await embedder.UpsertCandidateToAzureSqlAsync(extracted, normalized, embedding);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error] Failed processing '{Path.GetFileName(file)}': {ex.Message}");
    }
}

Console.WriteLine("\nIngestion pipeline execution complete.");
