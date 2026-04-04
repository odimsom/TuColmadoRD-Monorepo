using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace TuColmadoRD.Desktop;

internal static class UpdateService
{
    private const string LocalLatestInstallerApi = "http://localhost:5100/gateway/updates/latest-installer";
    private static readonly string[] ReleasesUrls =
    {
        "https://api.github.com/repos/synsetsolutions/TuColmadoRD-Monorepo/releases",
        "https://api.github.com/repos/odimsom/TuColmadoRD-Monorepo/releases"
    };

    public static async Task<UpdateCheckResult> CheckForUpdateAsync()
    {
        var localResolved = await TryResolveLatestInstallerFromLocalApiAsync();
        if (localResolved != null)
        {
            var currentInstalledVersion = GetCurrentVersion();
            var isTestTag = localResolved.Tag.Contains("-test", StringComparison.OrdinalIgnoreCase);
            var hasNewerVersion = localResolved.Version > currentInstalledVersion;

            if (isTestTag || hasNewerVersion)
            {
                return new UpdateCheckResult
                {
                    IsUpdateAvailable = true,
                    LatestVersion = localResolved.Version.ToString(),
                    InstallerUrl = localResolved.InstallerUrl
                };
            }

            return UpdateCheckResult.NoUpdate;
        }

        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("TuColmadoRD-Desktop-Updater/1.0");

        List<GitHubRelease>? releases = null;
        Exception? lastError = null;

        foreach (var releasesUrl in ReleasesUrls)
        {
            try
            {
                using var response = await http.GetAsync(releasesUrl);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<GitHubRelease>();

                if (releases.Count > 0)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        if (releases == null)
        {
            throw new InvalidOperationException(
                "No se pudo consultar actualizaciones desde GitHub para ninguno de los repositorios configurados.",
                lastError);
        }

        var latest = releases
            .Where(r => !string.IsNullOrWhiteSpace(r.TagName))
            .Select(r => new
            {
                Release = r,
                Version = ParseVersion(r.TagName!)
            })
            .Where(x => x.Version != null)
            .OrderByDescending(x => x.Version)
            .FirstOrDefault();

        if (latest == null)
        {
            return UpdateCheckResult.NoUpdate;
        }

        var latestTag = latest.Release.TagName ?? string.Empty;
        var isTestRelease = latestTag.Contains("-test", StringComparison.OrdinalIgnoreCase);
        var currentVersion = GetCurrentVersion();
        if (!isTestRelease && latest.Version! <= currentVersion)
        {
            return UpdateCheckResult.NoUpdate;
        }

        var installerAsset = latest.Release.Assets
            .FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        return new UpdateCheckResult
        {
            IsUpdateAvailable = installerAsset != null,
            LatestVersion = latest.Version!.ToString(),
            InstallerUrl = installerAsset?.BrowserDownloadUrl
        };
    }

    private static async Task<ResolvedInstaller?> TryResolveLatestInstallerFromLocalApiAsync()
    {
        try
        {
            var channel = IsTestBuild() ? "test" : "production";

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            using var response = await http.GetAsync($"{LocalLatestInstallerApi}?channel={channel}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<LatestInstallerPayload>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null || string.IsNullOrWhiteSpace(payload.InstallerUrl) || string.IsNullOrWhiteSpace(payload.Tag))
            {
                return null;
            }

            var version = ParseVersion(payload.Tag);
            if (version == null)
            {
                return null;
            }

            return new ResolvedInstaller
            {
                Tag = payload.Tag,
                Version = version,
                InstallerUrl = payload.InstallerUrl
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsTestBuild()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("RELEASE_TYPE"), "test", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                return false;
            }

            using var stream = File.OpenRead(configPath);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("AppSettings", out var settings) &&
                settings.TryGetProperty("IsTestBuild", out var testNode))
            {
                return string.Equals(testNode.GetString(), "true", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public static async Task<string> DownloadInstallerAsync(string installerUrl)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("TuColmadoRD-Desktop-Updater/1.0");

        var fileName = Path.GetFileName(new Uri(installerUrl).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "TuColmadoRD-Setup-latest.exe";
        }

        var targetDir = Path.Combine(Path.GetTempPath(), "TuColmadoRD", "updates");
        Directory.CreateDirectory(targetDir);
        var targetPath = Path.Combine(targetDir, fileName);

        await using var remote = await http.GetStreamAsync(installerUrl);
        await using var local = File.Create(targetPath);
        await remote.CopyToAsync(local);

        return targetPath;
    }

    public static void LaunchInstaller(string installerPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            UseShellExecute = true,
            Verb = "runas"
        });
    }

    private static Version GetCurrentVersion()
    {
        var infoVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var parsed = ParseVersion(infoVersion);
        if (parsed != null)
        {
            return parsed;
        }

        return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    }

    private static Version? ParseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var dash = normalized.IndexOf('-');
        if (dash >= 0)
        {
            normalized = normalized[..dash];
        }

        return Version.TryParse(normalized, out var version) ? version : null;
    }

    private sealed class GitHubRelease
    {
        public string? TagName { get; set; }
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    private sealed class GitHubAsset
    {
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }

    private sealed class LatestInstallerPayload
    {
        public string Tag { get; set; } = string.Empty;
        public string InstallerUrl { get; set; } = string.Empty;
    }

    private sealed class ResolvedInstaller
    {
        public string Tag { get; set; } = string.Empty;
        public Version Version { get; set; } = new Version(0, 0, 0, 0);
        public string InstallerUrl { get; set; } = string.Empty;
    }
}

internal sealed class UpdateCheckResult
{
    public static readonly UpdateCheckResult NoUpdate = new()
    {
        IsUpdateAvailable = false
    };

    public bool IsUpdateAvailable { get; init; }
    public string LatestVersion { get; init; } = string.Empty;
    public string? InstallerUrl { get; init; }
}
