namespace Zirconium.Scanners.CodeScanners;

public abstract class CodeScanner : Tool
{
    public string Name { get; }
    public string Description { get; }
    public string ObtainingSource { get; }
    public enum CodeLanguage { C=1, Cpp=2, Python=4}
    public IReadOnlyCollection<CodeLanguage> SupportedLanguages { get; }
    public MemoryTable ScanResults = null!; //Change this


    public CodeScanner(string name, string description, string obtainingSource, IEnumerable<CodeLanguage> supportedLanguages)
    {
        Name = name;
        Description = description;
        ObtainingSource = obtainingSource;
        SupportedLanguages = supportedLanguages.ToList().AsReadOnly();
    }


    public virtual void Dispose() => ScanResults.Dispose();
    public bool Supports(CodeLanguage language) => SupportedLanguages.Contains(language);

    public virtual string Version() => Commander.RunProcess(Name, "--version").stdout;

    public virtual bool VerifyInstall()
    {
        try
        {
            return Commander.RunProcess(Name, "--version").exitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public abstract void Scan(object scan);

}