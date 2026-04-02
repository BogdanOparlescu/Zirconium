namespace Zirconium.Scanners.CodeScanners;

public class Bandit : CodeScanner
{
    public Bandit() : base(
        "bandit",
        "Python Code Scanner",
        "pip install bandit",
        new List<CodeLanguage>() { CodeLanguage.Python }
        )
    { }

    public string scan_out = string.Empty;
    public override void Scan(object scan_path)
    {
        scan_out = Commander.RunProcess(
            Name, 
            $"-r {scan_path} --format custom --msg-template \"{{abspath}}:{{line}}: {{test_id}}[bandit]: {{severity}}: {{msg}}\""
            ).stdout;
    }
}
