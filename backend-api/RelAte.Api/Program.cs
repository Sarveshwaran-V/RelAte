using RelAte.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddHttpClient();

// Register Scorer
builder.Services.AddSingleton<IDeterministicScoringEngine, DeterministicScoringEngine>();

// Register LLM Provider based on configuration
var providerType = builder.Configuration["LLM:Provider"] ?? Environment.GetEnvironmentVariable("LLM_PROVIDER") ?? "gemini";
if (providerType.Equals("azure_openai", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<ILlmProvider, AzureOpenAIProvider>();
}
else if (providerType.Equals("gemini", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<ILlmProvider, GeminiFlashProvider>();
}
else
{
    builder.Services.AddScoped<ILlmProvider, MockLlmProvider>();
}

// Configure CORS for Angular (http://localhost:4200) and dev servers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
