using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

public class CanvasControl : UserControl
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Canvas Canvas { get; set; } = null!;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ActiveColor { get; set; } = Color.Black;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Zoom { get; set; } = 16;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public bool IsEraser { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public int BrushSize { get; set; } = 1;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBucket { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsLine { get; set; } = false;
    private Point _lineStart;
    private Point _lineEnd;
    private bool _isDrawingLine = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsEyedropper { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsRectangle { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsFilledRectangle { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public bool ShowGrid { get; set; } = true;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public bool ShowCenter { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowCheckerboard { get; set; } = true;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public bool SymmetryX { get; set; } = false;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SymmetryY { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ColorOpacity { get; set; } = 255;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int HighlightLayerIndex { get; set; } = -1;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color SecondaryColor { get; set; } = Color.White;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]


    public bool IsImporting { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    
    public bool IsMoveLayer { get; set; } = false;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    private Point _moveLayerStart;
    private Color[,]? _moveLayerSnapshot = null;
    public event Action<bool>? ImportCenteredChanged;
    private Bitmap? _importImage = null;
    private Rectangle _importRect;
    private bool _isDraggingImport = false;
    private bool _isResizingImport = false;
    private Point _importDragOffset;
    private Point _rectStart;
    private Point _rectEnd;
    private bool _isDrawingRect = false;
    private Stack<List<(string Name, bool IsVisible, Color[,] Pixels)>> undoStack = new();
    private Stack<List<(string Name, bool IsVisible, Color[,] Pixels)>> redoStack = new();
    private const int MaxUndoSteps = 20;
    private bool _importCentered = false;



    public CanvasControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, true);

        this.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter && IsImporting)
            {
                ConfirmImport();
                Invalidate();
            }
            if (e.KeyCode == Keys.Escape && IsImporting)
            {
                CancelImport();
            }
        };
    }

    public event Action? CanvasChanged;
    public event Action? LayersChanged;
    private bool _isResizingLayer = false;
    private Point _resizeLayerStart;
    private Color[,]? _resizeLayerSnapshot = null;
    protected override void OnPaint(PaintEventArgs e)
    {

        if (Canvas == null) return;

        var g = e.Graphics;

        for (int x = 0; x < Canvas.Width; x++)
        {
            for (int y = 0; y < Canvas.Height; y++)
            {
                Color color;
                if (ShowCheckerboard)
                    color = (x + y) % 2 == 0 ? Color.LightGray : Color.White;
                else
                    color = Color.White;
                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, x * Zoom, y * Zoom, Zoom, Zoom);
            }
        }

        // Nakresli všechny vrstvy
        foreach (var layer in Canvas.Layers.AsEnumerable().Reverse())
        {
            if (!layer.IsVisible) continue;

            for (int x = 0; x < Canvas.Width; x++)
            {
                for (int y = 0; y < Canvas.Height; y++)
                {
                    var color = layer.Pixels[x, y];
                    if (color == Color.Transparent) continue;
                    using var brush = new SolidBrush(color);
                    g.FillRectangle(brush, x * Zoom, y * Zoom, Zoom, Zoom);
                }
            }
        }
        // Střed plátna
        if (ShowCenter)
        {
            //using var centerPen = new Pen(Color.FromArgb(100, Color.Red), 3);
            using var centerPen = new Pen(GetCenterLineColor(), 3);
            int cx = Canvas.Width / 2 * Zoom;
            int cy = Canvas.Height / 2 * Zoom;
            g.DrawLine(centerPen, cx, 0, cx, Canvas.Height * Zoom);
            g.DrawLine(centerPen, 0, cy, Canvas.Width * Zoom, cy);
        }
        // Zvýraznění pixelů aktivní vrstvy
        if (HighlightLayerIndex >= 0 && HighlightLayerIndex < Canvas.Layers.Count)
        {
            using var highlightBrush = new SolidBrush(Color.FromArgb(100, Color.Yellow));
            var highlightLayer = Canvas.Layers[HighlightLayerIndex];
            for (int x = 0; x < Canvas.Width; x++)
                for (int y = 0; y < Canvas.Height; y++)
                    if (highlightLayer.Pixels[x, y] != Color.Transparent)
                        g.FillRectangle(highlightBrush, x * Zoom, y * Zoom, Zoom, Zoom);
        }
        // Mřížka
        if (ShowGrid)
        {
            using var pen = new Pen(Color.FromArgb(40, Color.Black));
            for (int x = 0; x <= Canvas.Width; x++)
                g.DrawLine(pen, x * Zoom, 0, x * Zoom, Canvas.Height * Zoom);
            for (int y = 0; y <= Canvas.Height; y++)
                g.DrawLine(pen, 0, y * Zoom, Canvas.Width * Zoom, y * Zoom);
        }

        // Náhled čáry
        if (IsLine && _isDrawingLine)
        {
            using var linePen = new Pen(ActiveColor, 1);
            g.DrawLine(linePen,
                _lineStart.X * Zoom + Zoom / 2,
                _lineStart.Y * Zoom + Zoom / 2,
                _lineEnd.X * Zoom + Zoom / 2,
                _lineEnd.Y * Zoom + Zoom / 2);
        }
        // Náhled obdélníku
        if (IsRectangle && _isDrawingRect)
        {
            int rx = Math.Min(_rectStart.X, _rectEnd.X) * Zoom;
            int ry = Math.Min(_rectStart.Y, _rectEnd.Y) * Zoom;
            int rw = (Math.Abs(_rectEnd.X - _rectStart.X) + 1) * Zoom;
            int rh = (Math.Abs(_rectEnd.Y - _rectStart.Y) + 1) * Zoom;

            using var rectPen = new Pen(ActiveColor, 1);
            g.DrawRectangle(rectPen, rx, ry, rw, rh);
        }
        // Import overlay
        if (IsImporting && _importImage != null)
        {
            var destRect = new Rectangle(
                _importRect.X * Zoom,
                _importRect.Y * Zoom,
                _importRect.Width * Zoom,
                _importRect.Height * Zoom
            );

            using var attrs = new System.Drawing.Imaging.ImageAttributes();
            var cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.7f };
            attrs.SetColorMatrix(cm);

            g.DrawImage(_importImage, destRect,
                0, 0, _importImage.Width, _importImage.Height,
                GraphicsUnit.Pixel, attrs);

            // Rohové táhlo pro změnu velikosti
            using var handleBrush = new SolidBrush(Color.FromArgb(255, 0, 120, 215));
            using var handlePen = new Pen(Color.White, 2);
            var handle = new Rectangle(
                destRect.Right - 12,
                destRect.Bottom - 12,
                12, 12
                        );
            g.FillRectangle(handleBrush, handle);
            g.DrawRectangle(handlePen, handle);

        }
        if (IsMoveLayer && _moveLayerSnapshot != null)
        {
            // Najdi hranice obsahu vrstvy
            var layer = Canvas.ActiveLayer;
            int minX = Canvas.Width, minY = Canvas.Height, maxX = 0, maxY = 0;
            bool hasPixels = false;

            for (int x = 0; x < Canvas.Width; x++)
                for (int y = 0; y < Canvas.Height; y++)
                    if (layer.Pixels[x, y] != Color.Transparent)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                        hasPixels = true;
                    }

            if (hasPixels)
            {
                using var handleBrush = new SolidBrush(Color.FromArgb(255, 0, 120, 215));
                using var handlePen = new Pen(Color.White, 2);
                var handle = new Rectangle(
                    (maxX + 1) * Zoom - 12,
                    (maxY + 1) * Zoom - 12,
                    12, 12
                );
                g.FillRectangle(handleBrush, handle);
                g.DrawRectangle(handlePen, handle);
            }
        }
    }

    private bool _isDrawing = false;
    private bool _isMiddleDrawing = false;
    private void MoveLayer(int offsetX, int offsetY)
    {
        if (_moveLayerSnapshot == null) return;

        var layer = Canvas.ActiveLayer;
        var newPixels = new Color[Canvas.Width, Canvas.Height];

        for (int x = 0; x < Canvas.Width; x++)
            for (int y = 0; y < Canvas.Height; y++)
            {
                int srcX = x - offsetX;
                int srcY = y - offsetY;
                if (srcX >= 0 && srcX < Canvas.Width && srcY >= 0 && srcY < Canvas.Height)
                    newPixels[x, y] = _moveLayerSnapshot[srcX, srcY];
                else
                    newPixels[x, y] = Color.Transparent;
            }

        Array.Copy(newPixels, layer.Pixels, newPixels.Length);
        Invalidate();
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        this.Focus();
        int x = e.Location.X / Zoom;
        int y = e.Location.Y / Zoom;

        if (IsImporting && _importImage != null)
        {
            if (e.Button == MouseButtons.Right)
            {
                CancelImport();
                return;
            }
            var handleX = (_importRect.X + _importRect.Width) * Zoom - 8;
            var handleY = (_importRect.Y + _importRect.Height) * Zoom - 8;
            var handle = new Rectangle(handleX, handleY, 8, 8);

            if (handle.Contains(e.Location))
            {
                _isResizingImport = true;
            }
            else if (x >= _importRect.X && x < _importRect.X + _importRect.Width &&
                     y >= _importRect.Y && y < _importRect.Y + _importRect.Height)
            {
                _isDraggingImport = true;
                _importDragOffset = new Point(x - _importRect.X, y - _importRect.Y);
            }
            return;
        }

        if (e.Button == MouseButtons.Middle)
        {
            _isMiddleDrawing = true;
            SaveSnapshot();
            var prevEraser = IsEraser;
            IsEraser = true;
            DrawPixel(e.Location);
            IsEraser = prevEraser;
            return;
        }

        if (IsMoveLayer && e.Button == MouseButtons.Left)
        {
            _moveLayerStart = new Point(x, y);
            SaveSnapshot();
            _moveLayerSnapshot = new Color[Canvas.Width, Canvas.Height];
            Array.Copy(Canvas.ActiveLayer.Pixels, _moveLayerSnapshot, Canvas.ActiveLayer.Pixels.Length);
            return;
        }
        if (IsMoveLayer && e.Button == MouseButtons.Left)
        {
            // Zkontroluj zda klikáme na táhlo pro resize
            var layer = Canvas.ActiveLayer;
            int maxX = 0, maxY = 0;
            bool hasPixels = false;

            for (int px = 0; px < Canvas.Width; px++)
                for (int py = 0; py < Canvas.Height; py++)
                    if (layer.Pixels[px, py] != Color.Transparent)
                    {
                        maxX = Math.Max(maxX, px);
                        maxY = Math.Max(maxY, py);
                        hasPixels = true;
                    }

            if (hasPixels)
            {
                var handle = new Rectangle(
                    (maxX + 1) * Zoom - 12,
                    (maxY + 1) * Zoom - 12,
                    12, 12
                );

                if (handle.Contains(e.Location))
                {
                    _isResizingLayer = true;
                    _resizeLayerStart = new Point(x, y);
                    _resizeLayerSnapshot = new Color[Canvas.Width, Canvas.Height];
                    Array.Copy(Canvas.ActiveLayer.Pixels, _resizeLayerSnapshot, Canvas.ActiveLayer.Pixels.Length);
                    return;
                }
            }

            _moveLayerStart = new Point(x, y);
            SaveSnapshot();
            _moveLayerSnapshot = new Color[Canvas.Width, Canvas.Height];
            Array.Copy(Canvas.ActiveLayer.Pixels, _moveLayerSnapshot, Canvas.ActiveLayer.Pixels.Length);
            return;
        }


        if (IsEyedropper)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (x >= 0 && x < Canvas.Width && y >= 0 && y < Canvas.Height)
                {
                    foreach (var layer in Canvas.Layers)
                    {
                        if (!layer.IsVisible) continue;
                        var color = layer.Pixels[x, y];
                        if (color != Color.Transparent)
                        {
                            ActiveColor = color;
                            break;
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (x >= 0 && x < Canvas.Width && y >= 0 && y < Canvas.Height)
                {
                    foreach (var layer in Canvas.Layers)
                    {
                        if (!layer.IsVisible) continue;
                        var color = layer.Pixels[x, y];
                        if (color != Color.Transparent)
                        {
                            SecondaryColor = color;
                            break;
                        }
                    }
                }
            }
            return;
        }

        if (IsLine)
        {
            _lineStart = new Point(x, y);
            _lineEnd = new Point(x, y);
            _isDrawingLine = true;
            SaveSnapshot();
            return;
        }

        if (IsRectangle)
        {
            _rectStart = new Point(x, y);
            _rectEnd = new Point(x, y);
            _isDrawingRect = true;
            SaveSnapshot();
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            _isDrawing = true;
            SaveSnapshot();
            DrawPixel(e.Location);
        }
        else if (e.Button == MouseButtons.Right)
        {
            SaveSnapshot();
            var prevColor = ActiveColor;
            ActiveColor = SecondaryColor;
            DrawPixel(e.Location);
            ActiveColor = prevColor;
        }
    }
    private void ResizeLayer(int newMaxX, int newMaxY)
    {
        if (_resizeLayerSnapshot == null) return;

        // Najdi hranice původního obsahu
        int minX = Canvas.Width, minY = Canvas.Height, maxX = 0, maxY = 0;
        for (int x = 0; x < Canvas.Width; x++)
            for (int y = 0; y < Canvas.Height; y++)
                if (_resizeLayerSnapshot[x, y] != Color.Transparent)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }

        int origW = maxX - minX + 1;
        int origH = maxY - minY + 1;
        int newW = Math.Max(1, newMaxX - minX + 1);
        int newH = Math.Max(1, newMaxY - minY + 1);

        // Vytvoř dočasný bitmap z původní vrstvy
        var srcBmp = new Bitmap(origW, origH);
        for (int x = 0; x < origW; x++)
            for (int y = 0; y < origH; y++)
                srcBmp.SetPixel(x, y, _resizeLayerSnapshot[minX + x, minY + y]);

        // Zmenši/zvětši
        var scaledBmp = new Bitmap(srcBmp, newW, newH);

        // Vypálení do vrstvy
        var layer = Canvas.ActiveLayer;
        for (int x = 0; x < Canvas.Width; x++)
            for (int y = 0; y < Canvas.Height; y++)
                layer.Pixels[x, y] = Color.Transparent;

        for (int x = 0; x < newW; x++)
            for (int y = 0; y < newH; y++)
            {
                int px = minX + x;
                int py = minY + y;
                if (px >= 0 && px < Canvas.Width && py >= 0 && py < Canvas.Height)
                {
                    var color = scaledBmp.GetPixel(x, y);
                    if (color.A > 0)
                        layer.Pixels[px, py] = color;
                }
            }

        Invalidate();
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (IsImporting)
        {
            _isDraggingImport = false;
            _isResizingImport = false;
            return;
        }
        if (IsMoveLayer)
        {
            _moveLayerSnapshot = null;
            return;
        }
        if (_isResizingLayer)
{
    _isResizingLayer = false;
    _resizeLayerSnapshot = null;
    SaveSnapshot();
    return;
}
        if (IsLine && _isDrawingLine)
        {
            DrawLine(_lineStart, _lineEnd);
            _isDrawingLine = false;
            Invalidate();
            return;
        }
        if (IsRectangle && _isDrawingRect)
        {
            DrawRectangle(_rectStart, _rectEnd);
            _isDrawingRect = false;
            Invalidate();
            return;
        }
        if (e.Button == MouseButtons.Middle)
        {
            _isMiddleDrawing = false;
            return;
        }
        _isDrawing = false;
    }
    public event Action<int, int>? PixelHovered;
    protected override void OnMouseMove(MouseEventArgs e)
    {
        int x = e.Location.X / Zoom;
        int y = e.Location.Y / Zoom;

        if (IsImporting && _importImage != null)
        {
            if (_isDraggingImport)
            {
                int newX = x - _importDragOffset.X;
                int newY = y - _importDragOffset.Y;

                int imgCenterX = newX + _importRect.Width / 2;
                int imgCenterY = newY + _importRect.Height / 2;
                int canvasCenterX = Canvas.Width / 2;
                int canvasCenterY = Canvas.Height / 2;

                int snapDistance = Math.Max(2, Canvas.Width / 50);

                if (Math.Abs(imgCenterX - canvasCenterX) < snapDistance)
                    newX = canvasCenterX - _importRect.Width / 2;
                if (Math.Abs(imgCenterY - canvasCenterY) < snapDistance)
                    newY = canvasCenterY - _importRect.Height / 2;

                if (Math.Abs(newX) < snapDistance) newX = 0;
                if (Math.Abs(newY) < snapDistance) newY = 0;
                if (Math.Abs(newX + _importRect.Width - Canvas.Width) < snapDistance)
                    newX = Canvas.Width - _importRect.Width;
                if (Math.Abs(newY + _importRect.Height - Canvas.Height) < snapDistance)
                    newY = Canvas.Height - _importRect.Height;

                _importRect.X = newX;
                _importRect.Y = newY;

                bool wasCentered = _importCentered;
                int finalCenterX = _importRect.X + _importRect.Width / 2;
                int finalCenterY = _importRect.Y + _importRect.Height / 2;
                _importCentered = (Math.Abs(finalCenterX - canvasCenterX) <= 1 &&
                                   Math.Abs(finalCenterY - canvasCenterY) <= 1);

                if (wasCentered != _importCentered)
                    ImportCenteredChanged?.Invoke(_importCentered);

                Invalidate();
            }
            else if (_isResizingImport)
            {
                int newWidth = Math.Max(1, x - _importRect.X);
                int newHeight = Math.Max(1, y - _importRect.Y);
                _importRect.Width = newWidth;
                _importRect.Height = newHeight;
                Invalidate();
            }
            return;
        }

        if (IsMoveLayer && e.Button == MouseButtons.Left && _moveLayerSnapshot != null)
        {
            int offsetX = x - _moveLayerStart.X;
            int offsetY = y - _moveLayerStart.Y;
            MoveLayer(offsetX, offsetY);
            return;
        }
        if (_isResizingLayer && _resizeLayerSnapshot != null)
        {
            ResizeLayer(x, y);
            return;
        }

        if (e.Button == MouseButtons.Right && !IsEyedropper)
        {
            var prevColor = ActiveColor;
            ActiveColor = SecondaryColor;
            DrawPixel(e.Location);
            ActiveColor = prevColor;
            return;
        }

        if (x >= 0 && x < Canvas.Width && y >= 0 && y < Canvas.Height)
            PixelHovered?.Invoke(x, y);

        if (IsLine && _isDrawingLine)
        {
            _lineEnd = new Point(x, y);
            Invalidate();
            return;
        }

        if (IsRectangle && _isDrawingRect)
        {
            _rectEnd = new Point(x, y);
            Invalidate();
            return;
        }

        if (e.Button == MouseButtons.Left)
            DrawPixel(e.Location);

        if (_isMiddleDrawing)
        {
            var prevEraser = IsEraser;
            IsEraser = true;
            DrawPixel(e.Location);
            IsEraser = prevEraser;
            return;
        }
    }
    private void DrawPixel(Point location)
    {
        if (Canvas == null) return;

        int cx = location.X / Zoom;
        int cy = location.Y / Zoom;

        DrawPixelAt(cx, cy);

        if (SymmetryX)
            DrawPixelAt(Canvas.Width - 1 - cx, cy);

        if (SymmetryY)
            DrawPixelAt(cx, Canvas.Height - 1 - cy);

        if (SymmetryX && SymmetryY)
            DrawPixelAt(Canvas.Width - 1 - cx, Canvas.Height - 1 - cy);

        CanvasChanged?.Invoke();
        Invalidate();
    }
    private void DrawPixelAt(int cx, int cy)
    {
        for (int dx = 0; dx < BrushSize; dx++)
            for (int dy = 0; dy < BrushSize; dy++)
            {
                int x = cx + dx;
                int y = cy + dy;

                if (x >= 0 && x < Canvas.Width && y >= 0 && y < Canvas.Height)
                {
                    if (IsBucket)
                        FloodFill(x, y, Canvas.ActiveLayer.Pixels[x, y], ActiveColor);
                    else if (IsEraser)
                        Canvas.ActiveLayer.Pixels[x, y] = Color.Transparent;
                    else
                    {
                        var c = ActiveColor;
                        Canvas.ActiveLayer.Pixels[x, y] = Color.FromArgb(ColorOpacity, c.R, c.G, c.B);
                    }
                }
            }
    }

    private void FloodFill(int startX, int startY, Color target, Color fill)
    {
        if (target == fill) return;

        var stack = new Stack<Point>();
        stack.Push(new Point(startX, startY));

        while (stack.Count > 0)
        {
            var p = stack.Pop();
            int x = p.X;
            int y = p.Y;

            if (x < 0 || x >= Canvas.Width || y < 0 || y >= Canvas.Height) continue;
            if (Canvas.ActiveLayer.Pixels[x, y] != target) continue;

            Canvas.ActiveLayer.Pixels[x, y] = fill;
            stack.Push(new Point(x + 1, y));
            stack.Push(new Point(x - 1, y));
            stack.Push(new Point(x, y + 1));
            stack.Push(new Point(x, y - 1));
        }
    }
    public void SaveSnapshot()
    {
        var snapshot = Canvas.Layers.Select(layer =>
        {
            var pixels = new Color[Canvas.Width, Canvas.Height];
            Array.Copy(layer.Pixels, pixels, layer.Pixels.Length);
            return (layer.Name, layer.IsVisible, pixels);
        }).ToList();

        undoStack.Push(snapshot);

        if (undoStack.Count > MaxUndoSteps)
            undoStack = new Stack<List<(string, bool, Color[,])>>(undoStack.Take(MaxUndoSteps));

        redoStack.Clear();
    }
    public void Undo()
    {
        if (undoStack.Count == 0) return;

        var current = Canvas.Layers.Select(layer =>
        {
            var pixels = new Color[Canvas.Width, Canvas.Height];
            Array.Copy(layer.Pixels, pixels, layer.Pixels.Length);
            return (layer.Name, layer.IsVisible, pixels);
        }).ToList();
        redoStack.Push(current);

        var snapshot = undoStack.Pop();
        RestoreSnapshot(snapshot);
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;

        var current = Canvas.Layers.Select(layer =>
        {
            var pixels = new Color[Canvas.Width, Canvas.Height];
            Array.Copy(layer.Pixels, pixels, layer.Pixels.Length);
            return (layer.Name, layer.IsVisible, pixels);
        }).ToList();
        undoStack.Push(current);

        var snapshot = redoStack.Pop();
        RestoreSnapshot(snapshot);
    }
    private void RestoreSnapshot(List<(string Name, bool IsVisible, Color[,] Pixels)> snapshot)
    {
        Canvas.Layers.Clear();
        foreach (var (name, isVisible, pixels) in snapshot)
        {
            var layer = new Layer(name, Canvas.Width, Canvas.Height);
            layer.IsVisible = isVisible;
            Array.Copy(pixels, layer.Pixels, pixels.Length);
            Canvas.Layers.Add(layer);
        }

        if (Canvas.ActiveLayerIndex >= Canvas.Layers.Count)
            Canvas.ActiveLayerIndex = Canvas.Layers.Count - 1;

        LayersChanged?.Invoke();
        Invalidate();
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (e.Delta > 0)
            Zoom = Math.Min(Zoom + 1, 32); // přiblížit, max zoom 32
        else
            Zoom = Math.Max(Zoom - 1, 2); // oddálit, min zoom 2

        Width = Canvas.Width * Zoom;
        Height = Canvas.Height * Zoom;
        Invalidate();
    }

    private void DrawLine(Point start, Point end)
    {
        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Nakresli čtverec BrushSize x BrushSize
            for (int bx = 0; bx < BrushSize; bx++)
                for (int by = 0; by < BrushSize; by++)
                {
                    int px = x0 + bx;
                    int py = y0 + by;
                    if (px >= 0 && px < Canvas.Width && py >= 0 && py < Canvas.Height)
                        Canvas.ActiveLayer.Pixels[px, py] = ActiveColor;
                }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void DrawRectangle(Point start, Point end)
    {
        int x0 = Math.Min(start.X, end.X);
        int y0 = Math.Min(start.Y, end.Y);
        int x1 = Math.Max(start.X, end.X);
        int y1 = Math.Max(start.Y, end.Y);

        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
            {
                if (x < 0 || x >= Canvas.Width || y < 0 || y >= Canvas.Height) continue;

                bool isEdge = x == x0 || x == x1 || y == y0 || y == y1;

                if (IsFilledRectangle || isEdge)
                    Canvas.ActiveLayer.Pixels[x, y] = ActiveColor;
            }
    }
    private Color GetCenterLineColor()
    {
        if (IsImporting && _importCentered)
            return Color.FromArgb(220, Color.LimeGreen);

        if (IsImporting)
            return Color.FromArgb(200, Color.Orange);
        if (IsImporting && _importCentered)
        return Color.FromArgb(220, Color.LimeGreen);

        int cx = Canvas.Width / 2;
        int cy = Canvas.Height / 2;

        // Vezmi průměrnou světlost pixelů podél středových čar
        float totalBrightness = 0;
        int count = 0;

        for (int x = 0; x < Canvas.Width; x++)
        {
            foreach (var layer in Canvas.Layers)
            {
                if (!layer.IsVisible) continue;
                var c = layer.Pixels[x, cy];
                if (c != Color.Transparent)
                {
                    totalBrightness += c.GetBrightness();
                    count++;
                    break;
                }
            }
        }

        for (int y = 0; y < Canvas.Height; y++)
        {
            foreach (var layer in Canvas.Layers)
            {
                if (!layer.IsVisible) continue;
                var c = layer.Pixels[cx, y];
                if (c != Color.Transparent)
                {
                    totalBrightness += c.GetBrightness();
                    count++;
                    break;
                }
            }
        }

        float avgBrightness = count > 0 ? totalBrightness / count : 1f;

        // Tmavé pozadí → oranžová, světlé pozadí → černá
        if (avgBrightness < 0.5f)
            return Color.FromArgb(200, Color.Orange);
        else
            return Color.FromArgb(200, Color.Black);
    }
    private bool _showCenterBeforeImport = false;

    public void StartImport(Bitmap image)
    {
        _importImage = image;

        float scaleX = (float)Canvas.Width / image.Width;
        float scaleY = (float)Canvas.Height / image.Height;
        float scale = Math.Min(scaleX, scaleY);

        int w = Math.Max(1, (int)(image.Width * scale));
        int h = Math.Max(1, (int)(image.Height * scale));

        _importRect = new Rectangle(0, 0, w, h);
        IsImporting = true;

        // Zapni střed plátna
        _showCenterBeforeImport = ShowCenter;
        ShowCenter = true;

        Invalidate();
    }

    public void ConfirmImport()
    {
        if (_importImage == null) return;

        SaveSnapshot();

        // Vytvoř novou vrstvu pro obrázek
        var importLayer = new Layer($"Import_{DateTime.Now:HHmmss}", Canvas.Width, Canvas.Height);
        Canvas.Layers.Insert(0, importLayer);
        Canvas.ActiveLayerIndex = 0;

        using var scaled = new Bitmap(_importImage, _importRect.Width, _importRect.Height);

        for (int x = 0; x < _importRect.Width; x++)
            for (int y = 0; y < _importRect.Height; y++)
            {
                int px = _importRect.X + x;
                int py = _importRect.Y + y;

                if (px >= 0 && px < Canvas.Width && py >= 0 && py < Canvas.Height)
                {
                    var color = scaled.GetPixel(x, y);
                    if (color.A > 0)
                        importLayer.Pixels[px, py] = color;
                }
            }

        _importImage = null;
        IsImporting = false;
        ShowCenter = _showCenterBeforeImport;
        LayersChanged?.Invoke();
        Invalidate();
    }

    public void CancelImport()
    {
        _importImage = null;
        IsImporting = false;
        ShowCenter = _showCenterBeforeImport; // ← vrať stav
        Invalidate();
    }
}