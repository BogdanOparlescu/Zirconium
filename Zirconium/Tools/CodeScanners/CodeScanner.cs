namespace Zirconium.Tools.CodeScanners;

public abstract class CodeScanner : Scanner
{
    public IReadOnlyCollection<string> SupportedLanguages { get; }


    public CodeScanner(string name, string description, string obtainingSource, IEnumerable<string> supportedLanguages) : base(name,description,obtainingSource)
    {
        SupportedLanguages = supportedLanguages.ToList().AsReadOnly();
    }

    public bool Supports(string language) => SupportedLanguages.Contains(language);
}