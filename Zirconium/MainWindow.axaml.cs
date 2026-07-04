using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections;
using System.Diagnostics;
using System.Text;
using Zirconium.Agents;
using Zirconium.Tools;
using Zirconium.Tools.CodeScanners;
using Zirconium.Tools.Recon;
using Zirconium.UI;

namespace Zirconium
{
    /// <summary>
    /// Interaction logic for MainWindow.axaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            if (Instance == null)
                Instance = this;
            InitializeToolingArsenal();
            InitializeConfigTable();
            InitializaOrchestrationCanvas();
        }
        public void CallStackUpdate(string text)
        {
            _ChatViewControl._CallStack.Text = text;
        }

        public void InitializeToolingArsenal() => _ToolGrid.ItemsSource = UIBinder.ToolingItems();
        public void InitializeConfigTable() => _ConfigGrid.ItemsSource = UIBinder.ConfigItems();
        public void InitializaOrchestrationCanvas() 
        {
            var transformGroup = (TransformGroup)_CanvasHost.RenderTransform!;
            _canvasScale = (ScaleTransform)transformGroup.Children[0];
            _canvasTranslate = (TranslateTransform)transformGroup.Children[1];
            UIOrchestraDiagram.Init(_OrchestrationCanvas, _CanvasHost, _canvasScale, _canvasTranslate);
        }

        private void SidebarMainChat(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = true;
            _ToolView.IsVisible = false;
            _OrchestrationCanvas.IsVisible = false;
            _RecordLog.IsVisible = false;
            _ConfigView.IsVisible = false;
        }
        private void SidebarInstalledTools(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = false;
            _ToolView.IsVisible = true;
            _OrchestrationCanvas.IsVisible = false;
            _RecordLog.IsVisible = false;
            _ConfigView.IsVisible = false;
        }
        private void SidebarOrchestrationSchema(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = false;
            _ToolView.IsVisible = false;
            _OrchestrationCanvas.IsVisible = true;
            _RecordLog.IsVisible = false;
            _ConfigView.IsVisible = false;
            if(Orchestra.Instance._Selected != null)
                UIOrchestraDiagram.Instance.DrawOrchestration(Orchestra.Instance._Selected!);
        }
        private void SidebarCallStack(object? sender, RoutedEventArgs e) =>
            _ChatViewControl.ToggleCallStack();
        private void SidebarRecordLog(object? sender, RoutedEventArgs e)
        {
            _ActionLogGrid.ItemsSource = UIBinder.ActionDB();
            _MainChat.IsVisible = false;
            _ToolView.IsVisible = false;
            _OrchestrationCanvas.IsVisible = false;
            _RecordLog.IsVisible = true;
            _ConfigView.IsVisible = false;
        }
        private void SidebarConfigSettings(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = false;
            _ToolView.IsVisible = false;
            _OrchestrationCanvas.IsVisible = false;
            _RecordLog.IsVisible = false;
            _ConfigView.IsVisible = true;
        }
            
        private void SidebarCollapse(object? sender, RoutedEventArgs e) => _SideBar.IsVisible = false;

        private void SideBarEnable(object? sender, RoutedEventArgs e) => _SideBar.IsVisible = true;

        /* Orchestration Canvas Helper */
        private ScaleTransform? _canvasScale;
        private TranslateTransform? _canvasTranslate;
        private void Canvas_PointerPressed(object sender, PointerPressedEventArgs e)
            => UIOrchestraDiagram.Instance.PointerPressed(this as Visual, e);

        private void Canvas_PointerMoved(object sender, PointerEventArgs e)
            => UIOrchestraDiagram.Instance.PointerMoved(e);

        private void Canvas_PointerReleased(object sender, PointerReleasedEventArgs e)
            => UIOrchestraDiagram.Instance.PointerReleased(e);

        private void Canvas_PointerWheelChanged(object sender, PointerWheelEventArgs e)
            => UIOrchestraDiagram.Instance.PointerWheelChanged(e);
    }
}