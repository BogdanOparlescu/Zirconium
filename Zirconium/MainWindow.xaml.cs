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
using Zirconium.Scanners.CodeScanners;

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
            _Debug.Text = $"{ApiKeys.Groq()} {ApiKeys.Cerebras()}";
            _Debug.Text = await new Agents.GroqAgent("openai/gpt-oss-120b").Ask(_Debug.Text);

            //Bandit bandit = new Bandit();
            //bandit.Scan("C:\\Users\\User\\Desktop\\z_tests\\vulpy-master");
            //_Debug.Text = bandit.scan_out;

            //Trufflehog trufflehog = new Trufflehog();
            //trufflehog.Scan("C:\\Users\\User\\Desktop\\z_tests\\vulpy-master");
            //_Debug.Text = trufflehog.Version();

            //Trivy trivy = new Trivy();
            //trivy.Scan("C:\\Users\\User\\Desktop\\z_tests\\vulpy-master");
            //_Debug.Text = trivy.Version() + trivy.VerifyInstall();
        }
    }
}