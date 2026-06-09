using System.Text;

namespace Zirconium;

public static class Config
{
    public static bool ToolCallJSON = true;
    public static bool ToolExplicitDescription = true; //If true: Zirconium tools also provide their description in the tool usage JSON
    public static uint ToolNumberOfTriesOnFailedUse = 3;
    public static string ZirconiumCurrentVersion => $"{ZirconiumProject} 0.0";
    public static string ZirconiumProject = "Zirconium";
}
