namespace Zirconium.Tools;

public interface Tool
{
    string Name { get; }
    string Description { get; }
    string ObtainingSource { get; }

    bool VerifyInstall();

    string Version();
}
