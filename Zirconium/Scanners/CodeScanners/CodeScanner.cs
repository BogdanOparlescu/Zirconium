namespace Zirconium.Scanners.CodeScanners;

public abstract class CodeScanner : Tool
{
    public string Name { get; }
    public string Description { get; }
    public string ObtainingSource { get; }
    public enum CodeLanguage { C, Cpp, Python}
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

    public abstract bool VerifyInstall();

    public abstract void Scan(object scan);

    public bool Supports(CodeLanguage language) => SupportedLanguages.Contains(language);
}