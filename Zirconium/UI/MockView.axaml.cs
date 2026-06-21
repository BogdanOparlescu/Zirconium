using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Zirconium.UI;

public partial class MockView : UserControl
{
    private Chat ViewModel => (Chat)DataContext!;

    private ScrollViewer? _scroll;

    public MockView()
    {
        InitializeComponent();

        DataContext = new Chat();
        _scroll = this.FindControl<ScrollViewer>("ChatScroll");

        ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(ScrollToBottom);
    }

    private async void SendClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await ViewModel.SendAsync();
        await Task.Delay(1);
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        _scroll?.ScrollToEnd();
    }
}