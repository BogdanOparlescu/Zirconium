namespace Zirconium;

static class ApiKeys
{
    public static string? Groq() => Environment.GetEnvironmentVariable("GROQ_API_KEY");
}