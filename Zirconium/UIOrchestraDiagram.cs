using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System.Text;
using Zirconium.Agents;
using Zirconium.Tools;

namespace Zirconium;

public class UIOrchestraDiagram
{
    private static UIOrchestraDiagram? _instance;

    public static void Init(Border orchestrationCanvas, Canvas canvasHost, ScaleTransform canvasScale, TranslateTransform canvasTranslate)
    {
        _instance ??= new UIOrchestraDiagram(orchestrationCanvas, canvasHost, canvasScale, canvasTranslate);
    }

    public static UIOrchestraDiagram Instance => _instance ?? throw new InvalidOperationException("Call UIOrchestraDiagram.Init() first!");

    private UIOrchestraDiagram(Border orchestrationCanvas, Canvas canvasHost, ScaleTransform canvasScale, TranslateTransform canvasTranslate)
    {
        _OrchestrationCanvas = orchestrationCanvas;
        _CanvasHost = canvasHost;
        _canvasScale = canvasScale;
        _canvasTranslate = canvasTranslate;
    }

    private readonly Border _OrchestrationCanvas;
    private readonly Canvas _CanvasHost;
    private readonly ScaleTransform _canvasScale;
    private readonly TranslateTransform _canvasTranslate;
    ToolAgent? _LastDrawn = null;
    bool redrawAgentsEveryTime = false;


    private class LayoutNode
    {
        public Point Position;
        public double Diameter;
        public string Name = "";
        public bool IsAgent;
    }

    private class LayoutEdge
    {
        public Point FromCenter;
        public Point ToCenter;
        public double FromDiameter;
        public double ToDiameter;
    }

    /// <summary>
    /// Draws an orchestration tree for the given ToolAgent, centered on the canvas.
    /// Clears the canvas and resets pan/zoom before drawing.
    /// </summary>
    public void DrawOrchestration(ToolAgent rootAgent)
    {
        if (rootAgent == null) return;

        // ── Clear canvas & reset transforms ───────────────────────
        _canvasScale.ScaleX = 1;
        _canvasScale.ScaleY = 1;
        _canvasTranslate.X = 0;
        _canvasTranslate.Y = 0;
        if (!redrawAgentsEveryTime)
        {
            if (rootAgent == _LastDrawn)
                return;
            _LastDrawn = rootAgent;
        }
        _CanvasHost.Children.Clear();

        // ── Tunable parameters ───────────────────────────────────
        const double BaseBubbleDiameter = 180;
        const double SizeDecay = 0.80;
        const double ToolSizeRatio = 0.70;
        const double BaseDistance = 240;
        const double DistanceDecay = 0.85;
        const double MaxArc = Math.PI / 2;
        const double ArrowHeadSize = 10;
        const double IncomingArrowLength = 140;
        const double MinBubbleDiameter = 55;
        const double CircleBorderThickness = 2.5;
        const double FontSizeRatio = 0.12;
        const double ToolFontSizeRatio = 0.185;
        // ────────────────────────────────────────────────────────

        var nodes = new List<LayoutNode>();
        var edges = new List<LayoutEdge>();

        // Root at origin, direction = right (horizontal)
        Point rootPos = new Point(0, 0);
        Vector rootDir = new Vector(1, 0);
        double rootD = Math.Max(BaseBubbleDiameter, MinBubbleDiameter);

        // Incoming arrow from the left → entering root agent
        edges.Add(new LayoutEdge
        {
            FromCenter = rootPos - rootDir * IncomingArrowLength,
            ToCenter = rootPos,
            FromDiameter = 0, // no bubble at the tail
            ToDiameter = rootD
        });

        // Root node
        nodes.Add(new LayoutNode
        {
            Position = rootPos,
            Diameter = rootD,
            Name = rootAgent.Name ?? "Root",
            IsAgent = true
        });

        // Recursively layout the tree
        LayoutTree(rootAgent, rootPos, rootDir, rootD,
            BaseDistance, SizeDecay, ToolSizeRatio,
            DistanceDecay, MaxArc,
            MinBubbleDiameter,
            nodes, edges);

        if (nodes.Count == 0) return;

        // ── Compute bounding box ─────────────────────────────────
        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;

        foreach (var n in nodes)
        {
            double r = n.Diameter / 2;
            minX = Math.Min(minX, n.Position.X - r);
            maxX = Math.Max(maxX, n.Position.X + r);
            minY = Math.Min(minY, n.Position.Y - r);
            maxY = Math.Max(maxY, n.Position.Y + r);
        }
        foreach (var e in edges)
        {
            minX = Math.Min(minX, e.FromCenter.X);
            maxX = Math.Max(maxX, e.FromCenter.X);
            minY = Math.Min(minY, e.FromCenter.Y);
            maxY = Math.Max(maxY, e.FromCenter.Y);
        }

        // ── Centre on canvas ────────────────────────────────────
        double cw = _OrchestrationCanvas.Bounds.Width > 0
            ? _OrchestrationCanvas.Bounds.Width : 800;
        double ch = _OrchestrationCanvas.Bounds.Height > 0
            ? _OrchestrationCanvas.Bounds.Height : 600;

        double offX = cw / 2 - (minX + maxX) / 2;
        double offY = ch / 2 - (minY + maxY) / 2;

        // ── Draw edges first (so they sit behind nodes) ──────────
        foreach (var e in edges)
        {
            Vector d = e.ToCenter - e.FromCenter;
            double len = d.Length;
            if (len < 0.001) continue;
            d = d / len;

            // Start at parent circle boundary (or raw point if no bubble)
            Point start = e.FromDiameter > 0
                ? GetCircleEdge(e.FromCenter, d, e.FromDiameter)
                : e.FromCenter;

            // End at child circle boundary
            Point end = GetCircleEdge(e.ToCenter, -d, e.ToDiameter);

            // Apply centring offset
            start = new Point(start.X + offX, start.Y + offY);
            end = new Point(end.X + offX, end.Y + offY);

            DrawArrow(start, end, ArrowHeadSize);
        }

        // ── Draw nodes (circles) ─────────────────────────────────
        foreach (var n in nodes)
        {
            double d = n.Diameter;
            double x = n.Position.X + offX - d / 2;
            double y = n.Position.Y + offY - d / 2;
            double fs = Math.Max(8, d * (n.IsAgent ? FontSizeRatio : ToolFontSizeRatio));

            // ── Build display text ────────────────────────────────
            // Agents: wrap name into multiple lines (names are a-zA-Z + spaces)
            // Tools:  single line, ellipsis if too long
            string displayText;
            if (n.IsAgent)
            {
                // Estimate how many characters fit on one line inside the circle.
                // Approx average char width ≈ fontSize * 0.55 for typical fonts.
                // Usable text width ≈ diameter * 0.72 (inscribed square in circle).
                double usableWidth = d * 0.72;
                double avgCharWidth = fs * 0.55;
                int maxCharsPerLine = Math.Max(4, (int)(usableWidth / avgCharWidth));

                displayText = WrapName(n.Name, maxCharsPerLine);
            }
            else
            {
                displayText = n.Name;
            }

            var tb = new TextBlock
            {
                Text = displayText,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = fs,
                TextWrapping = n.IsAgent ? TextWrapping.NoWrap : TextWrapping.NoWrap,
                TextTrimming = n.IsAgent ? TextTrimming.None : TextTrimming.CharacterEllipsis,
                TextAlignment = TextAlignment.Center,
                IsHitTestVisible = false
            };

            var b = new Border
            {
                Width = d,
                Height = d,
                Background = n.IsAgent ? Brushes.Transparent : Brushes.DimGray, //Background = Brushes.Transparent,
                BorderBrush = n.IsAgent ? Brushes.White : Brushes.DarkGray,
                BorderThickness = new Thickness(CircleBorderThickness),
                CornerRadius = new CornerRadius(d / 2), // perfect circle
                Padding = new Thickness(2),
                IsHitTestVisible = false,
                Child = tb
            };

            Canvas.SetLeft(b, x);
            Canvas.SetTop(b, y);
            _CanvasHost.Children.Add(b);
        }
    }

    /// <summary>
    /// Wraps a name (a-zA-Z + spaces) into multiple lines so it fits nicely
    /// inside a circle. Uses a greedy word-wrap with a target max characters
    /// per line, then balances the lines so they're roughly equal in length.
    /// </summary>
    private static string WrapName(string name, int maxCharsPerLine)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 1) return name;

        // ── Greedy first pass: pack words into lines ─────────────
        var lines = new List<string>();
        var current = new StringBuilder();

        foreach (var word in words)
        {
            if (current.Length == 0)
            {
                current.Append(word);
            }
            else if (current.Length + 1 + word.Length <= maxCharsPerLine)
            {
                current.Append(' ');
                current.Append(word);
            }
            else
            {
                lines.Add(current.ToString());
                current.Clear();
                current.Append(word);
            }
        }
        if (current.Length > 0)
            lines.Add(current.ToString());

        // ── Balance pass: if the last line is much shorter than the
        //    second-to-last, try to steal a word back to even things out ──
        if (lines.Count >= 2)
        {
            BalanceLines(lines);
        }

        return string.Join('\n', lines);
    }

    /// <summary>
    /// Attempts to balance line lengths by moving words from longer lines
    /// to shorter adjacent lines when it improves overall balance.
    /// </summary>
    private static void BalanceLines(List<string> lines)
    {
        bool changed = true;
        int iterations = 0;
        while (changed && iterations < 10)
        {
            changed = false;
            iterations++;

            for (int i = lines.Count - 1; i > 0; i--)
            {
                string prev = lines[i - 1];
                string curr = lines[i];

                // Only try to rebalance if previous line is significantly longer
                if (prev.Length - curr.Length < 3) continue;

                // Find the last word of the previous line
                int lastSpace = prev.LastIndexOf(' ');
                if (lastSpace <= 0) continue; // single word, can't split

                string lastWord = prev.Substring(lastSpace + 1);
                string newPrev = prev.Substring(0, lastSpace);
                string newCurr = lastWord + " " + curr;

                // Only move if it doesn't make the current line too long
                // and the result is more balanced
                if (newCurr.Length <= newPrev.Length + 2)
                {
                    lines[i - 1] = newPrev;
                    lines[i] = newCurr;
                    changed = true;
                }
            }
        }
    }

    /// <summary>
    /// Recursively lays out children of a ToolAgent in a fan-tree pattern.
    /// Each child's direction is the parent's direction rotated by an even
    /// share of the arc — this is the "angle of incidence derived from parent."
    /// </summary>
    private void LayoutTree(
        ToolAgent parent, Point parentPos, Vector parentDir,
        double parentD,
        double distance, double sizeDecay, double toolSizeRatio,
        double distanceDecay, double maxArc,
        double minD,
        List<LayoutNode> nodes, List<LayoutEdge> edges)
    {
        if (parent.Tools == null || parent.Tools.Count == 0) return;

        int n = parent.Tools.Count;

        // Circle diameter for this generation (before tool ratio)
        double childD = Math.Max(parentD * sizeDecay, minD);
        double childDist = distance * distanceDecay;

        for (int i = 0; i < n; i++)
        {
            Tool child = parent.Tools[i];
            bool isAgent = child is ToolAgent;

            // Tools are smaller than agents at the same level
            double cd = Math.Max(isAgent ? childD : childD * toolSizeRatio, minD);

            // Even angular spacing within the arc
            double angle = n == 1
                ? 0
                : -maxArc / 2 + (double)i / (n - 1) * maxArc;

            // Rotate parentDir by angle → child direction
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            Vector childDir = new Vector(
                parentDir.X * cos - parentDir.Y * sin,
                parentDir.X * sin + parentDir.Y * cos
            );

            Point childPos = parentPos + childDir * childDist;

            // Edge: parent → child
            edges.Add(new LayoutEdge
            {
                FromCenter = parentPos,
                ToCenter = childPos,
                FromDiameter = parentD,
                ToDiameter = cd
            });

            // Node
            nodes.Add(new LayoutNode
            {
                Position = childPos,
                Diameter = cd,
                Name = child.Name ?? "Unknown",
                IsAgent = isAgent
            });

            // Recurse into child ToolAgents; plain Tools are leaves
            if (isAgent)
            {
                LayoutTree((ToolAgent)child, childPos, childDir, cd,
                    childDist, sizeDecay, toolSizeRatio,
                    distanceDecay, maxArc,
                    minD, nodes, edges);
            }
        }
    }

    /// <summary>
    /// Returns the point where a ray from the circle centre in the given
    /// direction intersects the circle boundary.
    /// </summary>
    private Point GetCircleEdge(Point center, Vector dir, double diameter)
    {
        double radius = diameter / 2;
        return center + dir * radius;
    }

    /// <summary>
    /// Draws an arrow (line + filled triangular arrowhead) on the canvas.
    /// </summary>
    private void DrawArrow(Point start, Point end, double size)
    {
        // Line
        _CanvasHost.Children.Add(new Line
        {
            StartPoint = start,
            EndPoint = end,
            Stroke = Brushes.White,
            StrokeThickness = 2,
            IsHitTestVisible = false
        });

        // Arrowhead
        Vector d = end - start;
        double len = d.Length;
        if (len < 0.001) return;
        d = d / len;
        Vector perp = new Vector(-d.Y, d.X);

        Point p1 = end - d * size + perp * (size * 0.5);
        Point p2 = end - d * size - perp * (size * 0.5);

        _CanvasHost.Children.Add(new Polygon
        {
            Points = new[] { end, p1, p2 },
            Fill = Brushes.White,
            IsHitTestVisible = false
        });
    }

    /* UI FUNCTIONS BEGIN HERE */

    private Point _lastPoint;
    private bool _isPanning;

    // 1. Handle starting the pan (left mouse click)
    public void PointerPressed(Visual visual, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(visual).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPoint = e.GetPosition(_OrchestrationCanvas);

            // AVALONIA FIX: Capture the pointer on the control via the pointer itself
            e.Pointer.Capture(_OrchestrationCanvas);

            e.Handled = true;
        }
    }

    // 2. Handle dragging to pan
    public void PointerMoved(PointerEventArgs e)
    {
        if (_isPanning)
        {
            var currentPoint = e.GetPosition(_OrchestrationCanvas);

            _canvasTranslate!.X += currentPoint.X - _lastPoint.X;
            _canvasTranslate!.Y += currentPoint.Y - _lastPoint.Y;

            _lastPoint = currentPoint;
        }
    }

    // 3. Handle releasing the pan
    public void PointerReleased(PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;

            // AVALONIA FIX: Release the capture by passing null
            e.Pointer.Capture(null);

            e.Handled = true;
        }
    }

    // 4. Handle mouse wheel to zoom
    public void PointerWheelChanged(PointerWheelEventArgs e)
    {
        double zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;

        var mousePos = e.GetPosition(_CanvasHost);

        double oldScaleX = _canvasScale!.ScaleX;
        double oldScaleY = _canvasScale!.ScaleY;

        double newScaleX = oldScaleX * zoomFactor;
        double newScaleY = oldScaleY * zoomFactor;

        _canvasTranslate!.X = mousePos.X - (mousePos.X - _canvasTranslate!.X) * (newScaleX / oldScaleX);
        _canvasTranslate!.Y = mousePos.Y - (mousePos.Y - _canvasTranslate!.Y) * (newScaleY / oldScaleY);

        _canvasScale!.ScaleX = newScaleX;
        _canvasScale!.ScaleY = newScaleY;

        e.Handled = true;
    }
}