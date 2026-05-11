using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zirconium.Agents;
using Zirconium.Tools;
using Zirconium.Tools.CodeScanners;
using Zirconium.Tools.Recon;

namespace Zirconium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
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
            ToolAgent reconA = new ToolAgent("Recon Agent", "Calls passive and active recon tools", new GroqAgent("openai/gpt-oss-20b", 1024, "medium"), new List<Tool>() { new Nmap(), new Trivy()});
            ////Tool B = new ToolAgent("Recon Agent", "Calls passive and active recon tools", new GroqAgent("openai/gpt-oss-safeguard-20b", 1024), new List<Tool>() { reconA });
            string outRecon = await reconA.Ask(_Debug.Text);
            _Debug.Text = outRecon;
            //_Debug.Text = reconA.SystemPrompt;
            //ToolDatabase.CallTool("{\n  \"tool_name\": \"nmap\",\n  \"target\": \"192.168.0.24\",\n  \"arguments\": \"-O --minrate 20000\"\n}", reconA.Tools);
            // Agent a1 = {nmap}
            // Agent b1 = {C,B,a1}
        }
    }
}