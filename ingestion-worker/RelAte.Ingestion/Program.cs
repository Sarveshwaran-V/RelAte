using RelAte.Ingestion;

Console.WriteLine("=================================================");
Console.WriteLine("RelAte Ingestion Worker (.NET 10)");
Console.WriteLine("Tracks 2A (Sivaram) & 2B (Pradeep)");
Console.WriteLine("=================================================");

var samplesDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "golden_samples");
Console.WriteLine($"Watching golden sample directory: {samplesDir}");

if (!Directory.Exists(samplesDir))
{
    Directory.CreateDirectory(samplesDir);
}

var files = Directory.GetFiles(samplesDir);
Console.WriteLine($"Found {files.Length} sample files ready for ingestion.");

foreach (var file in files)
{
    Console.WriteLine($"Processing: {Path.GetFileName(file)} [Hash: {ResumeExtractor.ComputeFileHash(file)}]");
}

Console.WriteLine("Worker ready for batch pipeline execution.");
