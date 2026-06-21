using Avalonia.Data.Converters;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
}