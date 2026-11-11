using System.Drawing;
using System.Windows.Forms;

namespace PixelTo
{
    public class NewCanvasDialog : Form
    {
        private NumericUpDown numWidth;
        private NumericUpDown numHeight;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewCanvasDialog));
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "NewCanvasDialog";
            ResumeLayout(false);
        }

        public int CanvasWidth => (int)numWidth.Value;
        public int CanvasHeight => (int)numHeight.Value;

        public NewCanvasDialog()
        {
            InitializeComponent();
            Text = "Nový projekt — PixelTo";
            Size = new Size(400, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(45, 45, 45);

            var lblTitle = new Label
            {
                Text = "Nový projekt",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var lblWidth = new Label
            {
                Text = "Šířka (px):",
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(20, 75)
            };

            numWidth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1920,
                Value = 32,
                Location = new Point(130, 72),
                Width = 100,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            var lblHeight = new Label
            {
                Text = "Výška (px):",
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(20, 115)
            };

            numHeight = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1080,
                Value = 32,
                Location = new Point(130, 112),
                Width = 100,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            var lblPresets = new Label
            {
                Text = "Předvolby:",
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(20, 155)
            };

            var btnMinecraft = new Button
            {
                Text = "Minecraft (16×16)",
                Location = new Point(20, 178),
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMinecraft.FlatAppearance.BorderColor = Color.Gray;
            btnMinecraft.Click += (s, e) => { numWidth.Value = 16; numHeight.Value = 16; };

            var btnHD = new Button
            {
                Text = "HD (64×64)",
                Location = new Point(200, 178),
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnHD.FlatAppearance.BorderColor = Color.Gray;
            btnHD.Click += (s, e) => { numWidth.Value = 64; numHeight.Value = 64; };

            var btnFullHD = new Button
            {
                Text = "Full HD (128×128)",
                Location = new Point(20, 218),
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFullHD.FlatAppearance.BorderColor = Color.Gray;
            btnFullHD.Click += (s, e) => { numWidth.Value = 128; numHeight.Value = 128; };

            var btn256 = new Button
            {
                Text = "Ultra (256×256)",
                Location = new Point(200, 218),
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn256.FlatAppearance.BorderColor = Color.Gray;
            btn256.Click += (s, e) => { numWidth.Value = 256; numHeight.Value = 256; };

            var btnOk = new Button
            {
                Text = "Vytvořit",
                Location = new Point(130, 275),
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;

            Controls.Add(lblTitle);
            Controls.Add(lblWidth);
            Controls.Add(numWidth);
            Controls.Add(lblHeight);
            Controls.Add(numHeight);
            Controls.Add(lblPresets);
            Controls.Add(btnMinecraft);
            Controls.Add(btnHD);
            Controls.Add(btnFullHD);
            Controls.Add(btn256);
            Controls.Add(btnOk);
        }
    }
}