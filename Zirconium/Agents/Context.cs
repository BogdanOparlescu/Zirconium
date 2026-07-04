using System.Text;

namespace Zirconium.Agents;

public enum MemoryType
{
    Instant,
    Continuous,
    Persistent
}

public enum ReasoningType
{
    None,
    Assist,
    Reasoning
}

public class Context
{
    public MemoryType MemoryType { get; }
    private readonly StringBuilder _content = new();

    public Context(MemoryType memoryType = MemoryType.Instant) => MemoryType = memoryType;

    public string Get() => MemoryType == MemoryType.Instant ? string.Empty : _content.ToString();

    public void Add(string content)
    {
        if (MemoryType == MemoryType.Instant) 
            return;
        _content.AppendLine(content);
    }

    public void Clear() => _content.Clear();
}

public static class ContextHelper
{
    public static string CombineContext(this Context context, string prompt)
    {
        string contextPrefix = context.Get();
        if (string.IsNullOrEmpty(contextPrefix))
            return prompt;
        return $"{contextPrefix}\n{prompt}";
    }
}