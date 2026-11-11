using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace PixelTo
{
    public partial class Form1 : Form
    {
        private CanvasControl canvasControl;
        private Form? previewForm = null;
        private PictureBox? previewBox = null;
        private Label lblCoords = null!;
        private Panel leftPanel = null!;
        private Panel rightPanel = null!;
        private Panel centerPanel = null!;
        private Button? activeTool = null;
        private Button? activeColor = null;

        const int PANEL_WIDTH = 160;

        public Form1()
        {
            WindowState = FormWindowState.Maximized;
            InitializeComponent();
            InitLayout();
            InitCanvas();
            InitPalette();
            InitTools();
            InitLayers();
            InitKeyboard();
            InitMenu();
            this.Text = "PixelTo";
        }
        private Label? lblImportStatus = null;

        private void InitLayout()
        {
            this.BackColor = Color.FromArgb(60, 60, 60);

            leftPanel = new Panel
            {
                Width = PANEL_WIDTH,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10)
            };

            rightPanel = new Panel
            {
                Width = PANEL_WIDTH,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10)
            };

            centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 60),
                AutoScroll = true
            };

            Controls.Add(centerPanel);
            Controls.Add(rightPanel);
            Controls.Add(leftPanel);
        }

        private void InitCanvas()
        {
            var dialog = new NewCanvasDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            int canvasSize = 700;
            int zoom = canvasSize / Math.Max(dialog.CanvasWidth, dialog.CanvasHeight);

            var canvas = new Canvas(dialog.CanvasWidth, dialog.CanvasHeight);

            canvasControl = new CanvasControl
            {
                Canvas = canvas,
                ActiveColor = Color.Black,
                Zoom = zoom,
                Width = dialog.CanvasWidth * zoom,
                Height = dialog.CanvasHeight * zoom,
            };

            centerPanel.Controls.Add(canvasControl);
            CenterCanvas();
            centerPanel.Resize += (s, e) => CenterCanvas();
            canvasControl.CanvasChanged += () => UpdatePreview();
            canvasControl.LayersChanged += () => RefreshLayerButtons();
            lblImportStatus = new Label
            {
                Text = "⬜ Obrázek není vycentrován",
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false,
                Location = new Point(10, 10),
                Font = new Font("Arial", 9)
            };
            centerPanel.Controls.Add(lblImportStatus);
            lblImportStatus.BringToFront();

            canvasControl.ImportCenteredChanged += (centered) =>
            {
                if (lblImportStatus == null) return;
                if (centered)
                {
                    lblImportStatus.Text = "✅ Obrázek je vycentrován!";
                    lblImportStatus.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lblImportStatus.Text = "⬜ Obrázek není vycentrován";
                    lblImportStatus.ForeColor = Color.FromArgb(180, 180, 180);
                }
            };
        }

        private void CenterCanvas()
        {
            if (canvasControl == null) return;
            canvasControl.Location = new Point(
                Math.Max(0, (centerPanel.Width - canvasControl.Width) / 2),
                Math.Max(0, (centerPanel.Height - canvasControl.Height) / 2)
            );
        }

        private void SetActiveTool(Button btn)
        {
            if (activeTool != null)
            {
                activeTool.BackColor = Color.FromArgb(70, 70, 70);
                activeTool.FlatAppearance.BorderColor = Color.Gray;
                activeTool.FlatAppearance.BorderSize = 1;
            }
            activeTool = btn;
            activeTool.BackColor = Color.FromArgb(0, 120, 215);
            activeTool.FlatAppearance.BorderColor = Color.White;
            activeTool.FlatAppearance.BorderSize = 2;
        }

        private void SetActiveColor(Button btn)
        {
            if (activeColor != null)
            {
                activeColor.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                activeColor.FlatAppearance.BorderSize = 2;
            }
            activeColor = btn;
            activeColor.FlatAppearance.BorderColor = Color.White;
            activeColor.FlatAppearance.BorderSize = 3;
        }

        private void InitPalette()
        {

            var colors = new Color[]
            {
                Color.FromArgb(220, 50, 50),
                Color.FromArgb(50, 200, 50),
                Color.FromArgb(230, 120, 20),
                Color.FromArgb(20, 180, 220),
                Color.FromArgb(230, 210, 20),
                Color.FromArgb(80, 20, 220),
                Color.FromArgb(200, 20, 200),
                Color.FromArgb(20, 200, 150),
                Color.White,
                Color.Black,
                Color.FromArgb(150, 150, 150),
                Color.FromArgb(139, 90, 43),
            };

            var lblColors = new Label
            {
                Text = "Barvy",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, 10),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            leftPanel.Controls.Add(lblColors);

            int btnSize = 32;
            int spacing = 38;
            int startX = 10;
            int startY = 35;

            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                int col = i % 2;
                int row = i / 2;

                var btn = new Button
                {
                    Size = new Size(btnSize, btnSize),
                    Location = new Point(startX + col * spacing, startY + row * spacing),
                    BackColor = color,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = color
                };
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);

                btn.MouseDown += (s, e) =>
                {

                    if (e.Button == MouseButtons.Right)
                        canvasControl.SecondaryColor = color;
                    else if (e.Button == MouseButtons.Left)
                    {
                        canvasControl.ActiveColor = color;
                        SetActiveColor(btn);
                    }
                };

                leftPanel.Controls.Add(btn);
            }

            int afterColors = startY + (colors.Length / 2) * spacing + 10;

            var btnCustom = new Button
            {
                Text = "+",
                Size = new Size(btnSize, btnSize),
                Location = new Point(startX, afterColors),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            btnCustom.FlatAppearance.BorderSize = 1;
            btnCustom.FlatAppearance.BorderColor = Color.Gray;

            btnCustom.Click += (s, e) =>
            {
                var dialog = new ColorDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                    canvasControl.ActiveColor = dialog.Color;
            };
            btnCustom.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var dialog = new ColorDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                        canvasControl.SecondaryColor = dialog.Color;
                }
            };

            leftPanel.Controls.Add(btnCustom);

            var lblHint = new Label
            {
                Text = "L = primární  |  P = sekundární",
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent,
                AutoSize = false,
                Width = PANEL_WIDTH - 20,
                Height = 30,
                Location = new Point(10, afterColors + 45),
                Font = new Font("Arial", 7)
            };
            leftPanel.Controls.Add(lblHint);
        }

        private void InitTools()
        {
            int toolsY = 350;

            var sep = new Panel
            {
                Location = new Point(10, toolsY - 10),
                Size = new Size(PANEL_WIDTH - 20, 1),
                BackColor = Color.FromArgb(80, 80, 80)
            };
            leftPanel.Controls.Add(sep);

            var lblTools = new Label
            {
                Text = "Nástroje",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, toolsY),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            leftPanel.Controls.Add(lblTools);

            toolsY += 25;

            var lblTool = new Label
            {
                Text = "Aktivní: Štětec",
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = false,
                Width = PANEL_WIDTH - 20,
                Location = new Point(10, toolsY),
                Font = new Font("Arial", 8)
            };
            leftPanel.Controls.Add(lblTool);

            toolsY += 25;


            var tools = new (string text, string name, Action action)[]
            {
                ("P", "Štětec", () => { canvasControl.IsEraser = false; canvasControl.IsBucket = false; canvasControl.IsEyedropper = false; canvasControl.IsLine = false; canvasControl.IsRectangle = false;canvasControl.IsMoveLayer = false; lblTool.Text = "Aktivní: Štětec"; }),
                ("E", "Guma", () => { canvasControl.IsEraser = true; canvasControl.IsBucket = false; canvasControl.IsEyedropper = false; canvasControl.IsLine = false; canvasControl.IsRectangle = false;canvasControl.IsMoveLayer = false; lblTool.Text = "Aktivní: Guma"; }),
                ("B", "Kbelík", () => { canvasControl.IsEraser = false; canvasControl.IsBucket = true; canvasControl.IsEyedropper = false; canvasControl.IsLine = false; canvasControl.IsRectangle = false;canvasControl.IsMoveLayer = false; lblTool.Text = "Aktivní: Kbelík"; }),
                ("👁", "Pipeta", () => { canvasControl.IsEraser = false; canvasControl.IsBucket = false; canvasControl.IsEyedropper = true; canvasControl.IsLine = false; canvasControl.IsRectangle = false;canvasControl.IsMoveLayer = false; lblTool.Text = "Aktivní: Pipeta"; }),
                ("/", "Čára", () => { canvasControl.IsEraser = false; canvasControl.IsBucket = false; canvasControl.IsEyedropper = false; canvasControl.IsLine = true; canvasControl.IsRectangle = false;canvasControl.IsMoveLayer = false; lblTool.Text = "Aktivní: Čára"; }),
                ("□", "Obdélník", () => { canvasControl.IsEraser = false; canvasControl.IsBucket = false; canvasControl.IsEyedropper = false; canvasControl.IsLine = false; canvasControl.IsRectangle = true;canvasControl.IsMoveLayer = false; canvasControl.IsFilledRectangle = false; lblTool.Text = "Aktivní: Obdélník"; }),
                ("■", "Plný obdélník", () => { canvasControl.IsEraser = false; canvasControl.IsBucket = false; canvasControl.IsEyedropper = false; canvasControl.IsLine = false; canvasControl.IsRectangle = true;canvasControl.IsMoveLayer = false; canvasControl.IsFilledRectangle = true; lblTool.Text = "Aktivní: Plný obdélník"; }),
                ("✥", "Přesun vrstvy", () => {
    canvasControl.IsEraser = false;
    canvasControl.IsBucket = false;
    canvasControl.IsEyedropper = false;
    canvasControl.IsLine = false;
    canvasControl.IsRectangle = false;
    canvasControl.IsMoveLayer = true;
    lblTool.Text = "Aktivní: Přesun vrstvy";
}),
            };

            int btnSize = 32;
            int spacing = 38;
            var toolButtons = new List<Button>();

            for (int i = 0; i < tools.Length; i++)
            {
                var tool = tools[i];
                int col = i % 2;
                int row = i / 2;

                var btn = new Button
                {
                    Text = tool.text,
                    Size = new Size(btnSize, btnSize),
                    Location = new Point(10 + col * spacing, toolsY + row * spacing),
                    BackColor = Color.FromArgb(70, 70, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 10),
                    Tag = tool.name
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.Gray;

                btn.Click += (s, e) =>
                {
                    tool.action();
                    SetActiveTool(btn);
                };

                toolButtons.Add(btn);
                leftPanel.Controls.Add(btn);
            }

            // Nastav první nástroj jako aktivní
            if (toolButtons.Count > 0)
                SetActiveTool(toolButtons[0]);

            int afterTools = toolsY + ((tools.Length / 2) + 1) * spacing + 15;

            var sep2 = new Panel
            {
                Location = new Point(10, afterTools - 5),
                Size = new Size(PANEL_WIDTH - 20, 1),
                BackColor = Color.FromArgb(80, 80, 80)
            };
            leftPanel.Controls.Add(sep2);

            var lblBrush = new Label
            {
                Text = "Štětec: 1",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, afterTools),
                Font = new Font("Arial", 8)
            };

            var sliderBrush = new TrackBar
            {
                Minimum = 1,
                Maximum = 8,
                Value = 1,
                TickFrequency = 1,
                Location = new Point(10, afterTools + 20),
                Width = PANEL_WIDTH - 20,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            sliderBrush.ValueChanged += (s, e) =>
            {
                canvasControl.BrushSize = sliderBrush.Value;
                lblBrush.Text = $"Štětec: {sliderBrush.Value}";
            };

            leftPanel.Controls.Add(lblBrush);
            leftPanel.Controls.Add(sliderBrush);

            lblCoords = new Label
            {
                Text = "X: 0, Y: 0",
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, afterTools + 70),
                Font = new Font("Arial", 8)
            };
            leftPanel.Controls.Add(lblCoords);

            canvasControl.PixelHovered += (x, y) =>
            {
                lblCoords.Text = $"X: {x}, Y: {y}";
            };
            var lblOpacity = new Label
            {
                Text = "100%",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(88, 35),
                Font = new Font("Arial", 7)
            };

            var opacitySlider = new TrackBar
            {
                Minimum = 0,
                Maximum = 10,
                Value = 10,
                Orientation = Orientation.Vertical,
                TickFrequency = 1,
                Location = new Point(88, 55),
                Height = (12 / 2) * spacing,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            opacitySlider.ValueChanged += (s, e) =>
            {
                canvasControl.ColorOpacity = opacitySlider.Value * 25;
                lblOpacity.Text = $"{opacitySlider.Value * 10}%";
            };

            leftPanel.Controls.Add(lblOpacity);
            leftPanel.Controls.Add(opacitySlider);
        }

        private void InitLayers()
        {
            var lblLayers = new Label
            {
                Text = "Vrstvy",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, 10),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblLayers);

            var btnAddLayer = new Button
            {
                Text = "+",
                Size = new Size(32, 32),
                Location = new Point(10, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            btnAddLayer.FlatAppearance.BorderSize = 1;
            btnAddLayer.FlatAppearance.BorderColor = Color.Gray;
            btnAddLayer.Click += (s, e) =>
            {
                canvasControl.SaveSnapshot();
                var name = $"Vrstva {canvasControl.Canvas.Layers.Count + 1}";
                canvasControl.Canvas.Layers.Insert(0, new Layer(name, canvasControl.Canvas.Width, canvasControl.Canvas.Height));
                canvasControl.Canvas.ActiveLayerIndex = 0;
                RefreshLayerButtons();
            };

            var btnMerge = new Button
            {
                Text = "M",
                Size = new Size(32, 32),
                Location = new Point(52, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9)
            };
            var btnUp = new Button
            {
                Text = "↑",
                Size = new Size(32, 32),
                Location = new Point(94, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12)
            };
            btnUp.FlatAppearance.BorderSize = 1;
            btnUp.FlatAppearance.BorderColor = Color.Gray;
            btnUp.Click += (s, e) =>
            {
                int index = canvasControl.Canvas.ActiveLayerIndex;
                if (index <= 0) return;
                canvasControl.SaveSnapshot();
                var layer = canvasControl.Canvas.Layers[index];
                canvasControl.Canvas.Layers.RemoveAt(index);
                canvasControl.Canvas.Layers.Insert(index - 1, layer);
                canvasControl.Canvas.ActiveLayerIndex = index - 1;
                RefreshLayerButtons();
                canvasControl.Invalidate();
            };

            var btnDown = new Button
            {
                Text = "↓",
                Size = new Size(32, 32),
                Location = new Point(126, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12)
            };
            btnDown.FlatAppearance.BorderSize = 1;
            btnDown.FlatAppearance.BorderColor = Color.Gray;
            btnDown.Click += (s, e) =>
            {
                int index = canvasControl.Canvas.ActiveLayerIndex;
                if (index >= canvasControl.Canvas.Layers.Count - 1) return;
                canvasControl.SaveSnapshot();
                var layer = canvasControl.Canvas.Layers[index];
                canvasControl.Canvas.Layers.RemoveAt(index);
                canvasControl.Canvas.Layers.Insert(index + 1, layer);
                canvasControl.Canvas.ActiveLayerIndex = index + 1;
                RefreshLayerButtons();
                canvasControl.Invalidate();
            };

            rightPanel.Controls.Add(btnUp);
            rightPanel.Controls.Add(btnDown);
            btnMerge.FlatAppearance.BorderSize = 1;
            btnMerge.FlatAppearance.BorderColor = Color.Gray;
            btnMerge.Click += (s, e) =>
            {
                var canvas = canvasControl.Canvas;
                int index = canvas.ActiveLayerIndex;

                if (index >= canvas.Layers.Count - 1)
                {
                    MessageBox.Show("Pod aktivní vrstvou není žádná vrstva!", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var topLayer = canvas.Layers[index];
                var bottomLayer = canvas.Layers[index + 1];

                for (int x = 0; x < canvas.Width; x++)
                    for (int y = 0; y < canvas.Height; y++)
                    {
                        var color = topLayer.Pixels[x, y];
                        if (color != Color.Transparent)
                            bottomLayer.Pixels[x, y] = color;
                    }

                canvas.Layers.RemoveAt(index);
                canvas.ActiveLayerIndex = index - 1 < 0 ? 0 : index - 1;
                RefreshLayerButtons();
                canvasControl.Invalidate();
            };

            rightPanel.Controls.Add(btnAddLayer);
            rightPanel.Controls.Add(btnMerge);
            RefreshLayerButtons();
        }

        private void RefreshLayerButtons()
        {
            var toRemove = rightPanel.Controls.OfType<Control>()
                .Where(c => c.Tag?.ToString() == "layer").ToList();
            foreach (var ctrl in toRemove)
                rightPanel.Controls.Remove(ctrl);

            for (int i = 0; i < canvasControl.Canvas.Layers.Count; i++)
            {
                var index = i;
                var layer = canvasControl.Canvas.Layers[i];

                var btn = new Button
                {
                    Text = layer.Name,
                    Tag = "layer",
                    Location = new Point(10, 80 + i * 45),
                    Width = PANEL_WIDTH - 60,
                    Height = 32,
                    BackColor = index == canvasControl.Canvas.ActiveLayerIndex
                        ? Color.FromArgb(70, 130, 180)
                        : Color.FromArgb(70, 70, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 8)
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.Gray;

                btn.Click += (s, e) =>
                {
                    canvasControl.Canvas.ActiveLayerIndex = index;
                    RefreshLayerButtons();
                    canvasControl.Invalidate();
                };

                btn.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        canvasControl.HighlightLayerIndex = index;
                        canvasControl.Invalidate();
                    }
                };

                btn.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        canvasControl.HighlightLayerIndex = -1;
                        canvasControl.Invalidate();
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        var newName = AskForInput("Přejmenovat vrstvu", layer.Name);
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            canvasControl.Canvas.Layers[index].Name = newName;
                            RefreshLayerButtons();
                        }
                    }
                };

                var btnVisible = new Button
                {
                    Text = layer.IsVisible ? "👁" : "🚫",
                    Tag = "layer",
                    Location = new Point(PANEL_WIDTH - 80, 80 + i * 45),
                    Width = 32,
                    Height = 32,
                    BackColor = Color.FromArgb(70, 70, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 10)
                };
                btnVisible.FlatAppearance.BorderSize = 1;
                btnVisible.FlatAppearance.BorderColor = Color.Gray;
                btnVisible.Click += (s, e) =>
                {
                    canvasControl.Canvas.Layers[index].IsVisible = !canvasControl.Canvas.Layers[index].IsVisible;
                    RefreshLayerButtons();
                    canvasControl.Invalidate();
                };

                var btnDelete = new Button
                {
                    Text = "X",
                    Tag = "layer",
                    Location = new Point(PANEL_WIDTH - 45, 80 + i * 45),
                    Width = 32,
                    Height = 32,
                    BackColor = Color.FromArgb(180, 50, 50),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 8)
                };
                btnDelete.FlatAppearance.BorderSize = 1;
                btnDelete.FlatAppearance.BorderColor = Color.Gray;
                btnDelete.Click += (s, e) =>
                {
                    if (canvasControl.Canvas.Layers.Count <= 1) return;
                    canvasControl.SaveSnapshot();
                    canvasControl.Canvas.Layers.RemoveAt(index);
                    if (canvasControl.Canvas.ActiveLayerIndex >= canvasControl.Canvas.Layers.Count)
                        canvasControl.Canvas.ActiveLayerIndex = canvasControl.Canvas.Layers.Count - 1;
                    RefreshLayerButtons();
                    canvasControl.Invalidate();
                };

                rightPanel.Controls.Add(btn);
                rightPanel.Controls.Add(btnVisible);
                rightPanel.Controls.Add(btnDelete);
            }
        }

        private string? AskForInput(string title, string defaultValue)
        {
            var dialog = new Form
            {
                Text = title,
                Size = new Size(300, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            var textBox = new TextBox
            {
                Text = defaultValue,
                Location = new Point(20, 20),
                Width = 240
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(90, 60),
                Width = 80,
                DialogResult = DialogResult.OK
            };

            dialog.Controls.Add(textBox);
            dialog.Controls.Add(btnOk);

            return dialog.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        private void InitKeyboard()
        {
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && canvasControl.IsImporting)
                {
                    canvasControl.ConfirmImport();
                    canvasControl.Invalidate();
                    return;
                }
                if (e.KeyCode == Keys.Escape && canvasControl.IsImporting)
                {
                    canvasControl.CancelImport();
                    return;
                }
                if (e.KeyCode == Keys.Enter && canvasControl.IsImporting)
                {
                    canvasControl.ConfirmImport();
                    HideImportStatus();
                    canvasControl.Invalidate();
                    return;
                }
                if (e.KeyCode == Keys.Escape && canvasControl.IsImporting)
                {
                    canvasControl.CancelImport();
                    HideImportStatus();
                    return;
                }
                if (e.Control && e.KeyCode == Keys.Z) canvasControl.Undo();
                if (e.Control && e.KeyCode == Keys.Y) canvasControl.Redo();
                if (e.Control && e.KeyCode == Keys.S) SaveProject();
                if (e.Control && e.KeyCode == Keys.O) LoadProject();
                if (e.Control && e.KeyCode == Keys.E) ExportPng();
                if (e.Control && e.KeyCode == Keys.Oemplus)
                {
                    canvasControl.Zoom = Math.Min(canvasControl.Zoom + 1, 32);
                    canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
                    canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
                    CenterCanvas();
                    canvasControl.Invalidate();
                }
                if (e.Control && e.KeyCode == Keys.OemMinus)
                {
                    canvasControl.Zoom = Math.Max(canvasControl.Zoom - 1, 2);
                    canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
                    canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
                    CenterCanvas();
                    canvasControl.Invalidate();
                }
                if (e.Control && e.KeyCode == Keys.D0)
                {
                    canvasControl.Zoom = 700 / Math.Max(canvasControl.Canvas.Width, canvasControl.Canvas.Height);
                    canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
                    canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
                    CenterCanvas();
                    canvasControl.Invalidate();
                }
            };

            KeyPreview = true;
        }
        private void HideImportStatus()
        {
            if (lblImportStatus != null)
                lblImportStatus.Visible = false;
        }
        private void InitMenu()
        {
            var menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White
            };

            menuStrip.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());

            // ── Soubor ──
            var menuFile = new ToolStripMenuItem("Soubor") { ForeColor = Color.White };
            var itemSave = new ToolStripMenuItem("Uložit\tCtrl+S") { ForeColor = Color.White };
            var itemLoad = new ToolStripMenuItem("Otevřít\tCtrl+O") { ForeColor = Color.White };
            var itemImport = new ToolStripMenuItem("Vložit obrázek") { ForeColor = Color.White };
            itemImport.Click += (s, e) =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Obrázky|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "Vybrat obrázek"
                };

                if (dialog.ShowDialog() != DialogResult.OK) return;

                var bmp = new Bitmap(dialog.FileName);
                if (lblImportStatus != null)
                    lblImportStatus.Visible = true;
                canvasControl.StartImport(bmp);
            };

            menuFile.DropDownItems.Add(itemImport);
            var itemExport = new ToolStripMenuItem("Exportovat jako PNG\tCtrl+E") { ForeColor = Color.White };
            var itemExit = new ToolStripMenuItem("Ukončit\tAlt+F4") { ForeColor = Color.White };

            itemSave.Click += (s, e) => SaveProject();
            itemLoad.Click += (s, e) => LoadProject();
            itemExport.Click += (s, e) => ExportPng();
            itemExit.Click += (s, e) => Application.Exit();

            menuFile.DropDownItems.Add(itemSave);
            menuFile.DropDownItems.Add(itemLoad);
            menuFile.DropDownItems.Add(itemExport);
            menuFile.DropDownItems.Add(new ToolStripSeparator());
            menuFile.DropDownItems.Add(itemExit);

            // ── View ──
            var menuView = new ToolStripMenuItem("View") { ForeColor = Color.White };

            var itemGrid = new ToolStripMenuItem("Mřížka") { Checked = true, CheckOnClick = true, ForeColor = Color.White };
            itemGrid.CheckedChanged += (s, e) => { canvasControl.ShowGrid = itemGrid.Checked; canvasControl.Invalidate(); };

            var itemCheckerboard = new ToolStripMenuItem("Šachovnice") { Checked = true, CheckOnClick = true, ForeColor = Color.White };
            itemCheckerboard.CheckedChanged += (s, e) => { canvasControl.ShowCheckerboard = itemCheckerboard.Checked; canvasControl.Invalidate(); };

            var itemCoords = new ToolStripMenuItem("Souřadnice") { Checked = true, CheckOnClick = true, ForeColor = Color.White };
            itemCoords.CheckedChanged += (s, e) => { lblCoords.Visible = itemCoords.Checked; };

            var itemCenter = new ToolStripMenuItem("Střed plátna") { Checked = false, CheckOnClick = true, ForeColor = Color.White };
            itemCenter.CheckedChanged += (s, e) => { canvasControl.ShowCenter = itemCenter.Checked; canvasControl.Invalidate(); };

            var itemZoomIn = new ToolStripMenuItem("Přiblížit\tCtrl++") { ForeColor = Color.White };
            itemZoomIn.Click += (s, e) =>
            {
                canvasControl.Zoom = Math.Min(canvasControl.Zoom + 1, 32);
                canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
                canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
                CenterCanvas();
                canvasControl.Invalidate();
            };

            var itemZoomOut = new ToolStripMenuItem("Oddálit\tCtrl+-") { ForeColor = Color.White };
            itemZoomOut.Click += (s, e) =>
            {
                canvasControl.Zoom = Math.Max(canvasControl.Zoom - 1, 2);
                canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
                canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
                CenterCanvas();
                canvasControl.Invalidate();
            };

            var itemZoomReset = new ToolStripMenuItem("Reset zoom\tCtrl+0") { ForeColor = Color.White };
            itemZoomReset.Click += (s, e) =>
            {
                canvasControl.Zoom = 700 / Math.Max(canvasControl.Canvas.Width, canvasControl.Canvas.Height);
                canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
                canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
                CenterCanvas();
                canvasControl.Invalidate();
            };

            var itemSymX = new ToolStripMenuItem("Symetrie X") { Checked = false, CheckOnClick = true, ForeColor = Color.White };
            itemSymX.CheckedChanged += (s, e) => { canvasControl.SymmetryX = itemSymX.Checked; };

            var itemSymY = new ToolStripMenuItem("Symetrie Y") { Checked = false, CheckOnClick = true, ForeColor = Color.White };
            itemSymY.CheckedChanged += (s, e) => { canvasControl.SymmetryY = itemSymY.Checked; };

            menuView.DropDownItems.Add(itemGrid);
            menuView.DropDownItems.Add(itemCheckerboard);
            menuView.DropDownItems.Add(itemCoords);
            menuView.DropDownItems.Add(itemCenter);
            menuView.DropDownItems.Add(new ToolStripSeparator());
            menuView.DropDownItems.Add(itemZoomIn);
            menuView.DropDownItems.Add(itemZoomOut);
            menuView.DropDownItems.Add(itemZoomReset);
            menuView.DropDownItems.Add(new ToolStripSeparator());
            menuView.DropDownItems.Add(itemSymX);
            menuView.DropDownItems.Add(itemSymY);

            // ── O programu ──
            var menuAbout = new ToolStripMenuItem("O programu") { ForeColor = Color.White };
            menuAbout.Click += (s, e) =>
            {
                var aboutDialog = new Form
                {
                    Text = "O programu",
                    Size = new Size(350, 250),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(45, 45, 45)
                };

                aboutDialog.Controls.Add(new Label { Text = "PixelTo", Font = new Font("Arial", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
                aboutDialog.Controls.Add(new Label { Text = "Verze: 1.0.0", Location = new Point(20, 60), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
                aboutDialog.Controls.Add(new Label { Text = "Cíl: Jednoduchý pixel art editor s vrstvami", Location = new Point(20, 85), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
                aboutDialog.Controls.Add(new Label { Text = "Tvůrce: Matyáš Vrbka", Location = new Point(20, 110), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
                aboutDialog.Controls.Add(new Label { Text = "Technologie: C#, WinForms", Location = new Point(20, 135), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
                aboutDialog.Controls.Add(new Button { Text = "Zavřít", Location = new Point(120, 175), Width = 80, DialogResult = DialogResult.OK, ForeColor = Color.White, BackColor = Color.FromArgb(70, 70, 70), FlatStyle = FlatStyle.Flat });

                aboutDialog.ShowDialog();
            };

            menuStrip.Items.Add(menuFile);
            menuStrip.Items.Add(menuView);
            menuStrip.Items.Add(menuAbout);

            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }

        private void ExportPng()
        {
            var scaleDialog = new Form
            {
                Text = "Velikost exportu",
                Size = new Size(300, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            var lbl = new Label
            {
                Text = $"Originál: {canvasControl.Canvas.Width}×{canvasControl.Canvas.Height} px\nMěřítko:",
                Location = new Point(20, 20),
                Width = 240,
                Height = 40,
                ForeColor = Color.White
            };

            var numScale = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 10,
                Value = 1,
                Location = new Point(20, 70),
                Width = 80
            };

            var lblResult = new Label
            {
                Text = $"Výsledek: {canvasControl.Canvas.Width}×{canvasControl.Canvas.Height} px",
                Location = new Point(20, 100),
                Width = 240,
                AutoSize = true,
                ForeColor = Color.White
            };

            numScale.ValueChanged += (s2, e2) =>
            {
                int scale = (int)numScale.Value;
                lblResult.Text = $"Výsledek: {canvasControl.Canvas.Width * scale}×{canvasControl.Canvas.Height * scale} px";
            };

            var btnOk = new Button
            {
                Text = "Exportovat",
                Location = new Point(90, 130),
                Width = 100,
                Height = 30,
                DialogResult = DialogResult.OK
            };

            scaleDialog.Controls.Add(lbl);
            scaleDialog.Controls.Add(numScale);
            scaleDialog.Controls.Add(lblResult);
            scaleDialog.Controls.Add(btnOk);

            if (scaleDialog.ShowDialog() != DialogResult.OK) return;

            int exportScale = (int)numScale.Value;

            var saveDialog = new SaveFileDialog
            {
                Filter = "PNG soubor|*.png",
                Title = "Exportovat jako PNG",
                FileName = "pixel_art"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            int exportWidth = canvasControl.Canvas.Width * exportScale;
            int exportHeight = canvasControl.Canvas.Height * exportScale;

            var bmp = new Bitmap(exportWidth, exportHeight);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            foreach (var layer in canvasControl.Canvas.Layers.AsEnumerable().Reverse())
            {
                if (!layer.IsVisible) continue;
                for (int x = 0; x < canvasControl.Canvas.Width; x++)
                    for (int y = 0; y < canvasControl.Canvas.Height; y++)
                    {
                        var color = layer.Pixels[x, y];
                        if (color == Color.Transparent) continue;
                        using var brush = new SolidBrush(color);
                        g.FillRectangle(brush, x * exportScale, y * exportScale, exportScale, exportScale);
                    }
            }

            bmp.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
            MessageBox.Show("Exportováno!", "Hotovo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveProject()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PixelTo projekt|*.ptx",
                Title = "Uložit projekt",
                FileName = "projekt"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            var data = new ProjectData
            {
                Width = canvasControl.Canvas.Width,
                Height = canvasControl.Canvas.Height
            };

            foreach (var layer in canvasControl.Canvas.Layers)
            {
                var layerData = new LayerData
                {
                    Name = layer.Name,
                    IsVisible = layer.IsVisible
                };

                for (int x = 0; x < layer.Width; x++)
                    for (int y = 0; y < layer.Height; y++)
                    {
                        var color = layer.Pixels[x, y];
                        if (color == Color.Transparent) continue;

                        layerData.Pixels.Add(new PixelData
                        {
                            X = x,
                            Y = y,
                            Color = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}"
                        });
                    }

                data.Layers.Add(layerData);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json);
            MessageBox.Show("Projekt uložen!", "Hotovo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PixelTo projekt|*.ptx",
                Title = "Otevřít projekt"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            var json = File.ReadAllText(dialog.FileName);
            var data = System.Text.Json.JsonSerializer.Deserialize<ProjectData>(json);
            if (data == null) return;

            var canvas = new Canvas(data.Width, data.Height);
            canvas.Layers.Clear();

            foreach (var layerData in data.Layers)
            {
                var layer = new Layer(layerData.Name, data.Width, data.Height);
                layer.IsVisible = layerData.IsVisible;

                foreach (var pixel in layerData.Pixels)
                {
                    var color = ColorTranslator.FromHtml(pixel.Color.Substring(0, 7));
                    layer.Pixels[pixel.X, pixel.Y] = color;
                }

                canvas.Layers.Add(layer);
            }

            canvasControl.Canvas = canvas;
            canvasControl.Zoom = 700 / Math.Max(data.Width, data.Height);
            canvasControl.Width = canvasControl.Canvas.Width * canvasControl.Zoom;
            canvasControl.Height = canvasControl.Canvas.Height * canvasControl.Zoom;
            CenterCanvas();
            canvasControl.Invalidate();
            RefreshLayerButtons();
        }

        private void UpdatePreview()
        {
            if (previewForm == null || previewForm.IsDisposed || previewBox == null) return;

            var bmp = new Bitmap(
                Math.Max(1, canvasControl.Canvas.Width),
                Math.Max(1, canvasControl.Canvas.Height));

            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            foreach (var layer in canvasControl.Canvas.Layers.AsEnumerable().Reverse())
            {
                if (!layer.IsVisible) continue;
                for (int x = 0; x < canvasControl.Canvas.Width; x++)
                    for (int y = 0; y < canvasControl.Canvas.Height; y++)
                    {
                        var color = layer.Pixels[x, y];
                        if (color == Color.Transparent) continue;
                        bmp.SetPixel(x, y, color);
                    }
            }

            if (previewBox.Image != null)
                previewBox.Image.Dispose();

            previewBox.Image = bmp;
            previewBox.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    public class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(70, 70, 70);
        public override Color MenuItemBorder => Color.FromArgb(100, 100, 100);
        public override Color MenuBorder => Color.FromArgb(100, 100, 100);
        public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 45);
        public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 45);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 45);
        public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 45);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 70);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 70);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(90, 90, 90);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(90, 90, 90);
        public override Color SeparatorDark => Color.FromArgb(80, 80, 80);
    }
}