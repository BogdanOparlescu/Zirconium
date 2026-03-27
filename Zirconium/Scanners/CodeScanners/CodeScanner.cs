using Zirconium.CodeScanners;

namespace Zirconium.Scanners.CodeScanners;

public abstract class CodeScanner : Tool
{
    public string Name { get; }
    public string Description { get; }
    public string ObtainingSource { get; }
    public enum CodeLanguage { C, Cpp, Python}
    public IReadOnlyCollection<CodeLanguage> SupportedLanguages { get; }
    public MemoryTable ScanResults;


    public CodeScanner(string name, string description, string obtainingSource, IEnumerable<CodeLanguage> supportedLanguages, MemoryTable scanResults)
    {
        Name = name;
        Description = description;
        ObtainingSource = obtainingSource;
        SupportedLanguages = supportedLanguages.ToList().AsReadOnly();
        ScanResults = scanResults;
    }


    public abstract void Dispose();

    public abstract bool VerifyInstall();

    public abstract void Scan();
}