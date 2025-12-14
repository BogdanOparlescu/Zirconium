using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Zirconium;

public class Groq
{
    private string key = string.Empty;

    public Groq(string key)
    {
        this.key = key;
    }

    public async Task<string> Ask(
    string prompt,
    string model,
    uint max_output_tokens = 8192,
    string reasoning_effort = "medium",
    bool enableBrowserSearch = false,
    bool enableCodeInterpreter = false
    )
    {
        var requestDict = new Dictionary<string, object?>
        {
            ["messages"] = new[]
            {
            new { role = "user", content = prompt }
        },
            ["model"] = model,
            ["temperature"] = 1,
            ["max_completion_tokens"] = max_output_tokens,
            ["top_p"] = 1,
            ["stream"] = false,
            ["reasoning_effort"] = reasoning_effort,
            ["stop"] = null
        };

        var tools = new List<object>();
        if (enableBrowserSearch)
            tools.Add(new { type = "browser_search" });
        if (enableCodeInterpreter)
            tools.Add(new { type = "code_interpreter" });
        if (tools.Count > 0)
            requestDict["tools"] = tools;

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");

        var json = JsonSerializer.Serialize(requestDict);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await http.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            content
        );

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"Error: {response.StatusCode}\n{responseText}";

        var jsonDoc = JsonDocument.Parse(responseText);

        if (jsonDoc.RootElement.TryGetProperty("choices", out var choices))
        {
            return choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}