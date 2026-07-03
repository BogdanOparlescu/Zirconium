using Avalonia.Data.Converters;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

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

        var userText = Prompt.Trim();
        Prompt = "";

        UserMessage(userText);
        AssistantMessage("GenerateLorem()");
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