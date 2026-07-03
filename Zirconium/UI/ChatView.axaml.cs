using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;
using Zirconium.Agents;

namespace Zirconium.UI;

public partial class ChatView : UserControl
{
    private Chat ViewModel => (Chat)DataContext!;

    private ScrollViewer? _scroll;
    private List<(ToolAgent Agent, int Indent)> _modelTree = new();
    private DateTime _popupClosedAt = DateTime.MinValue;

    public ChatView()
    {
        InitializeComponent();
        LoadAsciiArt();

        DataContext = new Chat();
        _scroll = this.FindControl<ScrollViewer>("ChatScroll");

        ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        ModelPopup.PlacementTarget = ModelSelectorButton;
        ModelPopup.Closed += (_, _) => _popupClosedAt = DateTime.Now;
        BuildModelTree();
        UpdateModelLabel();
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

    private void BuildModelTree()
    {
        var root = Orchestra.Instance.RootAgent();
        FlattenToolAgents(root, 0, _modelTree);
    }

    private void FlattenToolAgents(ToolAgent agent, int indent, List<(ToolAgent, int)> result)
    {
        result.Add((agent, indent));
        if (agent.Tools == null) return;
        foreach (var tool in agent.Tools)
            if (tool is ToolAgent ta)
                FlattenToolAgents(ta, indent + 1, result);
    }

    private void ModelSelectorClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if ((DateTime.Now - _popupClosedAt).TotalMilliseconds < 200) return;
        if (ModelPopup.IsOpen) { ModelPopup.IsOpen = false; return; }

        var stackPanel = new StackPanel();
        var currentSelected = Orchestra.Instance._Selected;

        foreach (var (agent, indent) in _modelTree)
        {
            var isSelected = ReferenceEquals(agent, currentSelected);
            var border = new Border
            {
                Background = isSelected ? new SolidColorBrush(Color.Parse("#1A1A1A")) : Brushes.Transparent,
                Padding = new Thickness(indent * 16 + 12, 8, 12, 8),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = agent
            };
            border.Child = new TextBlock
            {
                Text = agent.Name,
                Foreground = new SolidColorBrush(Color.Parse("#EAEAEA")),
                FontSize = 13
            };
            border.PointerReleased += ModelItem_PointerReleased;
            stackPanel.Children.Add(border);
        }

        ModelScroll.Content = stackPanel;

        const double popupWidth = 260;
        var popupHeight = _modelTree.Count * 36.0 + 2;
        //ModelPopup.HorizontalOffset = -popupWidth - 100;
        //ModelPopup.VerticalOffset = -(ModelSelectorButton.Bounds.Height / 2 + popupHeight);
        ModelPopup.HorizontalOffset = -(popupWidth / 2);
        ModelPopup.VerticalOffset = -(ModelSelectorButton.Bounds.Height + popupHeight);
        ModelPopup.IsOpen = true;
    }

    private void ModelItem_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border && border.Tag is ToolAgent agent)
        {
            Orchestra.Instance._Selected = agent;
            UpdateModelLabel();
            ModelPopup.IsOpen = false;
        }
    }

    private void UpdateModelLabel()
    {
        var selected = Orchestra.Instance._Selected;
        ModelLabel.Text = selected is ToolAgent ta ? $"{ta.Name}" : "Model: —";
    }
}