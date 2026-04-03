using System.Drawing;
using System.Windows.Forms;

namespace TuColmadoRD.Desktop;

internal sealed class SplashForm : Form
{
    private readonly Label _statusLabel;

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(460, 260);
        BackColor = AppTheme.Background;
        ShowInTaskbar = false;
        TopMost = true;
        Padding = new Padding(24);

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(8, 14, 26)
        };

        var logo = new PictureBox
        {
            Size = new Size(72, 72),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = BrandAssets.CreateLogoBitmap(72),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.None
        };

        var title = new Label
        {
            Text = "Iniciando TuColmadoRD...",
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };

        var progress = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 28,
            Width = 300,
            Height = 6,
            Anchor = AnchorStyles.None
        };

        _statusLabel = new Label
        {
            Text = "Preparando servicios locales...",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

        layout.Controls.Add(logo, 0, 1);
        layout.Controls.Add(title, 0, 2);
        layout.Controls.Add(progress, 0, 3);
        layout.Controls.Add(_statusLabel, 0, 4);

        content.Controls.Add(layout);
        Controls.Add(content);

        content.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(55, 30, 58, 138), 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, content.Width - 1, content.Height - 1);
        };
    }

    public void SetStatus(string text)
    {
        _statusLabel.Text = text;
    }
}
