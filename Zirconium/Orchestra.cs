using Zirconium.Agents;
using Zirconium.Tools;
using Zirconium.Tools.CodeScanners;
using Zirconium.Tools.Recon;

namespace Zirconium;

public sealed class Orchestra
{
    private static Orchestra? _instance;
    public static Orchestra Instance => _instance ??= new Orchestra();
    
    private Orchestra()
    {
        Initialize();
    }

    private ToolAgent? _Root { get; set; }
    public ToolAgent? _Selected { get; set; }
    private void Initialize()
    {
        ToolAgent reconA = new ToolAgent("Recon Agent", "Calls passive and active recon tools",
                                new GroqAgent("openai/gpt-oss-20b", 1024, "high", 0.8),
                                new List<Tool>() { new Nmap(), new Trivy(), new Trivy(), new Trivy(), new Trivy() });
        ToolAgent main = new ToolAgent("Vulnerability Detection Agent", "Orchestrates vulnerability findings",
                            new GroqAgent("openai/gpt-oss-120b", 2048, "high"),
                            new List<Tool>() { reconA, new Nmap() });

        _Selected = main;
        _Root = main;
    }

    public ToolAgent RootAgent() => _Root!;
    public async Task<string> Process(string query) => await _Selected!.Ask(query);
}
