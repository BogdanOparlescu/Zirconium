import re
from pathlib import Path

ROOT = Path(".")

PATTERNS = {
    r"\bSystem\.Windows\b": "WPF namespace",
    r"\bSystem\.Windows\.[A-Za-z0-9_.]+\b": "WPF namespace",
    r"\bWindow\b": "WPF Window",
    r"\bApplication\.Current\b": "WPF Application",
    r"\bDispatcher\b": "WPF Dispatcher",
    r"\bDependencyProperty\b": "DependencyProperty",
    r"\bDependencyObject\b": "DependencyObject",
    r"\bRoutedEvent\b": "RoutedEvent",
    r"\bRoutedEventArgs\b": "RoutedEventArgs",
    r"\bFrameworkElement\b": "FrameworkElement",
    r"\bUIElement\b": "UIElement",
    r"\bVisualTreeHelper\b": "VisualTreeHelper",
    r"\bLogicalTreeHelper\b": "LogicalTreeHelper",
    r"\bMessageBox\b": "WPF MessageBox",
    r"\bCommandManager\b": "CommandManager",
    r"\bDataObject\b": "Clipboard/DragDrop",
    r"\bDragDrop\b": "DragDrop",
    r"\bAdorner\b": "Adorner",
    r"\bItemsControl\b": "ItemsControl",
    r"\bControlTemplate\b": "ControlTemplate",
    r"\bBindingOperations\b": "BindingOperations",
    r"\bPresentationSource\b": "PresentationSource",
    r"\bHwndSource\b": "Win32 interop",
    r"\bHwndHost\b": "Win32 interop",
    r"\bSystem\.Windows\.Interop\b": "Win32 interop",
    r"\bSystem\.Windows\.Media\b": "WPF Media",
    r"\bSystem\.Windows\.Controls\b": "WPF Controls",
    r"\bSystem\.Windows\.Input\b": "WPF Input",
    r"\bSystem\.Windows\.Documents\b": "WPF Documents",
    r"\bSystem\.Windows\.Navigation\b": "WPF Navigation",
    r"\bSystem\.Windows\.Shapes\b": "WPF Shapes",
}

compiled = [(re.compile(k), v) for k, v in PATTERNS.items()]

for file in ROOT.rglob("*.cs"):
    try:
        text = file.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        continue

    hits = []

    for regex, description in compiled:
        for match in regex.finditer(text):
            line = text.count("\n", 0, match.start()) + 1
            hits.append((line, description, match.group(0)))

    if hits:
        print("=" * 80)
        print(file)

        for line, desc, value in hits:
            print(f"  L{line:<5} {desc:<25} {value}")