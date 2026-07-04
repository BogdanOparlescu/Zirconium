namespace Zirconium.Tools;

public static class ActionDatabase
{
    private static List<(string, string)> Actions = new();

    public static void RecordAction(string caller, string action) => Actions.Add((caller, action));

    public static List<(string caller, string action)> GetAllActions() => Actions;
}
