namespace Zirconium.Tools.CodeScanners;

public class Bandit : CodeScanner
{
    public Bandit() : base(
        "bandit",
        "Python Code Scanner",
        "pip install bandit",
        new[]{ "python" }
        )
    { }

    public override void Scan(object scan_path)
    {
        scan_out = Commander.RunProcess(
            Name, 
            $"-r {scan_path} --format custom --msg-template \"{{abspath}}:{{line}}: {{test_id}}[bandit]: {{severity}}: {{msg}}\""
            ).stdout;
    }
}
