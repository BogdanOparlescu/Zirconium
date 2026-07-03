using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;

namespace Zirconium.UI;

public partial class ChatView : UserControl
{
    private Chat ViewModel => (Chat)DataContext!;

    private ScrollViewer? _scroll;

    public ChatView()
    {
        InitializeComponent();
        LoadAsciiArt();

        DataContext = new Chat();
        _scroll = this.FindControl<ScrollViewer>("ChatScroll");

        ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
    }

    //private void InitializeComponent()
    //{
    //    AvaloniaXamlLoader.Load(this);
    //}

    private void LoadAsciiArt()
    {
        var assemblyName = GetType().Assembly.GetName().Name;
        using var stream = AssetLoader.Open(new Uri($"avares://{assemblyName}/UI/octo.txt"));

        using var reader = new StreamReader(stream);
        _OctoAsciiArt.Text = reader.ReadToEnd();
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        Dispatcher.UIThread.Post(ScrollToBottom);

    private async void SendClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await ViewModel.Process();
        ScrollToBottom();
    }

    private void ScrollToBottom() => _scroll?.ScrollToEnd();
}