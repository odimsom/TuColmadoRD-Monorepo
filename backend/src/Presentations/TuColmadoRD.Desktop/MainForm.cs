using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TuColmadoRD.Desktop;

public partial class MainForm : Form
{
    private readonly string _startUrl;
    private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 3000 };

    private Label _statusLabel = null!;

    public MainForm(string startUrl, bool openWebViewOnStart = false)
    {
        _startUrl = startUrl;

        InitializeComponent();

        this.Shown += async (_, _) => await RefreshStatusAsync();
        _statusTimer.Tick += async (_, _) => await RefreshStatusAsync();
        _statusTimer.Start();
    }

    private void InitializeComponent()
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor = AppTheme.Background;
        Size = new Size(760, 500);
        MinimumSize = new Size(700, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TuColmadoRD";
        Icon = BrandAssets.CreateLogoIcon(32);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = AppTheme.Background,
            Padding = new Padding(24)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22f));

        var logo = new PictureBox
        {
            Size = new Size(110, 110),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = BrandAssets.CreateLogoBitmap(110),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.None
        };

        var title = new Label
        {
            Text = "TuColmadoRD",
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };

        _statusLabel = new Label
        {
            Text = "Iniciando servicios...",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };

        var openSystemButton = new Button
        {
            Text = "Open System",
            Width = 280,
            Height = 62,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.None
        };
        openSystemButton.FlatAppearance.BorderSize = 0;
        openSystemButton.Click += (_, _) => OpenExternalUrl(_startUrl);

        var updateButton = new Button
        {
            Text = "Buscar actualizaciones",
            Width = 280,
            Height = 44,
            BackColor = Color.FromArgb(15, 23, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.None
        };
        updateButton.FlatAppearance.BorderColor = Color.FromArgb(51, 65, 85);
        updateButton.FlatAppearance.BorderSize = 1;
        updateButton.Click += async (_, _) => await CheckAndUpdateAsync(updateButton);

        layout.Controls.Add(logo, 0, 1);
        layout.Controls.Add(title, 0, 2);
        layout.Controls.Add(_statusLabel, 0, 3);
        layout.Controls.Add(openSystemButton, 0, 4);
        layout.Controls.Add(updateButton, 0, 5);

        Controls.Add(layout);
    }

    private async Task RefreshStatusAsync()
    {
        var gatewayUp = await IsEndpointUpAsync("http://localhost:5100/health");
        var apiUp = await IsEndpointUpAsync("http://localhost:5200/health");

        if (gatewayUp && apiUp)
        {
            _statusLabel.Text = "Sistema listo";
            _statusLabel.ForeColor = AppTheme.Green;
            return;
        }

        _statusLabel.Text = "Sistema iniciando...";
        _statusLabel.ForeColor = AppTheme.Amber;
    }

    private static async Task<bool> IsEndpointUpAsync(string url)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await http.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static void OpenExternalUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private async Task CheckAndUpdateAsync(Button updateButton)
    {
        try
        {
            updateButton.Enabled = false;
            updateButton.Text = "Buscando...";

            var result = await UpdateService.CheckForUpdateAsync();
            if (!result.IsUpdateAvailable || string.IsNullOrWhiteSpace(result.InstallerUrl))
            {
                MessageBox.Show("No hay actualizaciones disponibles en este momento.", "Actualizaciones", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Hay una nueva versión disponible ({result.LatestVersion}).\n\n¿Deseas descargarla e instalar ahora?",
                "Actualización disponible",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            updateButton.Text = "Descargando...";
            var installerPath = await UpdateService.DownloadInstallerAsync(result.InstallerUrl);
            UpdateService.LaunchInstaller(installerPath);
            Application.Exit();
        }
        catch (Exception ex)
        {
            AppLogger.Error("Error while updating desktop app", ex);
            var message =
                "No se pudo completar la actualización automática.\n\n" +
                $"Detalle: {ex.Message}\n\n" +
                $"Log: {AppLogger.LogFilePath}";
            MessageBox.Show(message, "Error de actualización", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
            {
                updateButton.Enabled = true;
                updateButton.Text = "Buscar actualizaciones";
            }
        }
    }
}
