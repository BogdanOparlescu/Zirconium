using Avalonia.Data.Converters;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zirconium.UI;

public class Chat : INotifyPropertyChanged
{
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private string _prompt = "";
    public string Prompt
    {
        get => _prompt;
        set
        {
            if (_prompt == value)
                return;

            _prompt = value;
            OnPropertyChanged();
        }
    }
    
    public void UserMessage(string content) => Messages.Add(new ChatMessage { Content = content , IsUser  = true});
    public void AssistantMessage(string content) => Messages.Add(new ChatMessage { Content = content, IsUser = false });
    
    public async Task Process()
    {
        if (string.IsNullOrWhiteSpace(Prompt))
            return;

        if (Prompt[0] == '/')
        {
            ProcessCommand(Prompt);
            Prompt = "";
            return;
        }

        var userText = Prompt.Trim();
        Prompt = "";

        UserMessage(userText);
        var x = await Orchestra.Instance.Process(userText);
        AssistantMessage(x);
    }

    public static List<(string Name, string Description, Action<Chat> Handler)> ChatCommands = new();

    public void ProcessCommand(string command)
    {
        foreach (var (name, _, handler) in ChatCommands)
            if (name == command)
            {
                handler(this);
                return;
            }
    }

    public void InitChatCommands()
    {
        ChatCommands.Add(("/agent", "Displays selected agent information", chat => chat.DisplayAgentInformation()));
        ChatCommands.Add(("/clear", "Clear chat history and selected agent memory", chat => chat.ClearHistory()));
        ChatCommands.Add(("/prompt", "Display the selected agent system prompt", chat => chat.DisplaySystemInstructions()));
        ChatCommands.Add(("/help", "Show available commands", chat =>
        {
            var sb = new StringBuilder("Available commands:\n");
            foreach (var (name, description, _) in ChatCommands)
                sb.AppendLine($"  {name,-10} - {description}");
            chat.AssistantMessage(sb.ToString());
        }));
    }

    public void ClearHistory()
    {
        Prompt = "";
        Messages.Clear();
        //Orchestra.Instance._Selected!.ClearMemory();
    }

    public void DisplaySystemInstructions() =>
        AssistantMessage(Orchestra.Instance._Selected!.SystemPrompt);

    public void DisplayAgentInformation()
    {
        var agent = Orchestra.Instance._Selected!;
        var sb = new StringBuilder();
        sb.AppendLine(agent.Name);
        sb.AppendLine(agent.Description);
        sb.AppendLine(agent.Agent.ToString());
        sb.AppendLine($"[ {string.Join(" ", agent.Tools.Select(t => t.Name)) } ]");
        AssistantMessage(sb.ToString());
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}



/*--These are bindings directly into the UI, do not modify them!*/
public sealed class AlignConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isUser = value is bool b && b;
        return isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}