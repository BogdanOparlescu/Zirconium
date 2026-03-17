namespace Zirconium;

static class ApiKeys
{
    public static string? Groq() => Environment.GetEnvironmentVariable("GROQ_API_KEY");
    public static string? Cerebras() => Environment.GetEnvironmentVariable("CEREBRAS_API_KEY");
}