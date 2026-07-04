namespace Zirconium.Tools;

public abstract class Scanner(string name, string description, string obtainingSource) : Tool, IDisposable
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string ObtainingSource { get; } = obtainingSource;
    public MemoryTable? ScanResults = null;
    public string scan_out = string.Empty; //Temporary - remove when ScanResults are properly implemented!

    public virtual void Dispose() => ScanResults?.Dispose();

    public virtual string Version() => Commander.RunProcess(Name, "--version", true).stdout;
    public virtual bool VerifyInstall()
    {
        try
        {
            return Commander.RunProcess(Name, "--version", true).exitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    public abstract void Scan(object scan);
}
