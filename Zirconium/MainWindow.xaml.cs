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
            //_Debug.Text = await new Zirconium.Agents.GroqAgent("openai/gpt-oss-120b").Ask(_Debug.Text);

            MemoryTable mt = new MemoryTable("Blues", new string[] { "Name", "Age" });
            mt.Insert(new string[] { "Andrew", "12" });
            mt.Insert(new string[] { "Bob", "34" });
            mt.Insert(new string[] { "Alice", "36" });
            mt = mt.Query($"SELECT * FROM {mt.name} WHERE Age > 30");
            _Debug.Text = mt.ToString();
        }
    }
}