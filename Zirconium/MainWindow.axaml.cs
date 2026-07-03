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
            InitializeConfigTable();
            InitializaOrchestrationCanvas();
        }
        public void CallStackUpdate(string text)
        {
            _CallStackDisplay.Text = text;
        }

        public void InitializeConfigTable() => _ConfigGrid.ItemsSource = UIBinder.ConfigItems();
        public void InitializaOrchestrationCanvas() 
        {
            var transformGroup = (TransformGroup)_CanvasHost.RenderTransform!;
            _canvasScale = (ScaleTransform)transformGroup.Children[0];
            _canvasTranslate = (TranslateTransform)transformGroup.Children[1];
            UIOrchestraDiagram.Init(_OrchestrationCanvas, _CanvasHost, _canvasScale, _canvasTranslate);
        }

        private async void Button_Click(object? sender, RoutedEventArgs e)
        {
            //_Debug.Text = $"{ApiKeys.Groq()} {ApiKeys.Cerebras()}";
            //_Debug.Text = await new Agents.GroqAgent("openai/gpt-oss-120b").Ask(_Debug.Text);

            //Bandit bandit = new Bandit();
            //bandit.Scan("C:\\Users\\User\\Desktop\\z_tests\\vulpy-master");
            //_Debug.Text = bandit.scan_out;

            //Trufflehog trufflehog = new Trufflehog();
            //trufflehog.Scan("C:\\Users\\User\\Desktop\\z_tests\\vulpy-master");
            //_Debug.Text = trufflehog.Version();

            //Trivy trivy = new Trivy();
            //trivy.Scan("C:\\Users\\User\\Desktop\\z_tests\\vulpy-master");
            //_Debug.Text = trivy.Version() + trivy.VerifyInstall();

            //Nmap nmap = new Nmap();
            //nmap.Scan("192.168.0.24", "--min-rate 60000");
            //_Debug.Text = nmap.scan_out;

            //Subfinder subfinder = new Subfinder();
            //_Debug.Text = subfinder.VerifyInstall().ToString(); // + "\n\n" + subfinder.Version();

            //ReconAgent ReconAgent = new ReconAgent();
            //string response = await ReconAgent.Ask(_Debug.Text);
            //_Debug.Text = response;

            //new GroqAgent("llama-3.1-8b-instant", 1024)
            // If only DEAD people understand hexadecimal, how many people understand hexadecimal? [Provide only the most concise answer, do not waste tokens]
            // A -> B -> C -> A
            // artificial analysis 
            // ChatGPT - 30/40 t/s
            // Groq 
            ////Tool B = new ToolAgent("Recon Agent", "Calls passive and active recon tools", new GroqAgent("openai/gpt-oss-safeguard-20b", 1024), new List<Tool>() { reconA });
            ToolAgent reconA = new ToolAgent("Recon Agent", "Calls passive and active recon tools",
                                new GroqAgent("openai/gpt-oss-20b", 1024, "high", 0.8),
                                new List<Tool>() { new Nmap(), new Trivy() });
            ToolAgent main = new ToolAgent("Vulnerability Detection Agent", "Orchestrates vulnerability findings",
                                new GroqAgent("openai/gpt-oss-120b", 2048, "high"),
                                new List<Tool>() { reconA });

            //new CerebrasAgent("zai-glm-4.7", 2048, "default"),


            string outRecon = await main.Ask(_Debug.Text!);
            _Debug.Text = outRecon;
            //_Debug.Text = reconA.SystemPrompt;
            //ToolDatabase.CallTool("{\n  \"tool_name\": \"nmap\",\n  \"target\": \"192.168.0.24\",\n  \"arguments\": \"-O --minrate 20000\"\n}", reconA.Tools);
            // Agent a1 = {nmap}
            // Agent b1 = {C,B,a1}
        }


        private void SidebarMainChat(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = true;
            _OrchestrationCanvas.IsVisible = false;
            _ConfigView.IsVisible = false;
        }
        private void SidebarInstalledTools(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = false;
            _OrchestrationCanvas.IsVisible = true;
            _ConfigView.IsVisible = false;
        }
        private void SidebarOrchestrationSchema(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = false;
            _OrchestrationCanvas.IsVisible = true;
            _ConfigView.IsVisible = false;
            if(Orchestra.Instance._Selected != null)
                UIOrchestraDiagram.Instance.DrawOrchestration(Orchestra.Instance._Selected!);
        }
        private void SidebarCallStack(object? sender, RoutedEventArgs e)
        {
            _SideBar.IsVisible = false;
        }
        private void SidebarRecordLog(object? sender, RoutedEventArgs e)
        {

        }
        private void SidebarConfigSettings(object? sender, RoutedEventArgs e)
        {
            _MainChat.IsVisible = false;
            _OrchestrationCanvas.IsVisible = false;
            _ConfigView.IsVisible = true;
        }
            
        private void SidebarCollapse(object? sender, RoutedEventArgs e)
        {
            _SideBar.IsVisible = false;
        }

        private void SideBarEnable(object? sender, RoutedEventArgs e)
        {
            _SideBar.IsVisible = true;
        }






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