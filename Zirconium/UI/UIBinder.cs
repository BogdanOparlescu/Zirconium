using Avalonia.Data.Converters;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Zirconium.Agents;
using Zirconium.Tools;
namespace Zirconium.UI;

public static class UIBinder
{
    public static bool CallStackPresent = true;
    //public static void CallStackUpdate()
    //{
    //    //get tooldb static db variable!
    //    MainWindow.Instance!._Debug.Text = "aaaaabcd";
    //    MainWindow.Instance!._Debug.UpdateLayout();
    //}

    public static async Task CallStackUpdate()
    {
        if (!CallStackPresent)
            return;
        StringBuilder sb = new();
        foreach ((ToolAgent, string) a in ToolDatabase.ToolCallStack)
            sb.Append($"{a.Item1.Name}\n{a.Item2}\n\n");
        MainWindow.Instance!.CallStackUpdate(sb.ToString());
        await Task.Delay(2000);
    }
    public record ToolItem(string Agent, string Tool, bool Installed);

    public static List<ToolItem> ToolingItems() =>
        ToolDatabase.GetTools().Select(t => new ToolItem(t.agent, t.tool.Name, t.tool.VerifyInstall())).ToList();

    public record ActionLogItem(string Caller, string Action);
    public static List<ActionLogItem> ActionDB() =>
        ActionDatabase.GetAllActions().Select(a => new ActionLogItem(a.caller, a.action)).ToList();

    public record ConfigItem(string Name, string Value, string Type);
    public static List<ConfigItem> ConfigItems()
    {
        var rows = new List<ConfigItem>();
        var t = typeof(Config);
        var flags = BindingFlags.Public | BindingFlags.Static;

        foreach (var f in t.GetFields(flags))
            rows.Add(new ConfigItem(f.Name, f.GetValue(null)?.ToString() ?? "", f.FieldType.Name));

        foreach (var p in t.GetProperties(flags))
        {
            if (p.GetIndexParameters().Length > 0) continue;
            rows.Add(new ConfigItem(p.Name, p.GetValue(null)?.ToString() ?? "", p.PropertyType.Name));
        }
        return rows;
    }
}