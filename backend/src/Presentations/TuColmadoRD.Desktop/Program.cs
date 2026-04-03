using System.IO;
using Microsoft.AspNetCore.Builder;
using TuColmadoRD.ApiGateway;
using TuColmadoRD.Presentation.API;

namespace TuColmadoRD.Desktop;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => AppLogger.Error("Unhandled UI exception", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                AppLogger.Error("Unhandled domain exception", exception);
            }
        };

        ApplicationConfiguration.Initialize();
        AppLogger.Info("Desktop startup begin");

        // 1. Start Core API on 5200
        var coreApp = CoreApiHostBuilder.BuildCoreApi(args, isLocal: true);
        _ = Task.Run(() => coreApp.RunAsync("http://localhost:5200"));

        // 2. Start Auth Mock on 5300
        var authApp = AuthLocalHostBuilder.BuildAuthLocal(args);
        _ = Task.Run(() => authApp.RunAsync("http://localhost:5300"));

        // 3. Start Gateway on 5100
        var gatewayApp = GatewayHostBuilder.BuildGateway(args, new GatewayOptions
        {
            IsLocalMode = true,
            AuthApiUrl = "http://localhost:5300",
            CoreApiUrl = "http://localhost:5200",
            AllowedOrigins = new[] { "http://localhost:5100" }
        });
        
        // Serve static files (Angular build in wwwroot)
        gatewayApp.UseStaticFiles();
        // spa should be handled by UseStaticFiles if it's index.html, 
        // but we can add fallback for Angular routing
        gatewayApp.MapFallbackToFile("index.html");

        _ = Task.Run(() => gatewayApp.RunAsync("http://localhost:5100"));

        // 4. Run WinForms immediately; the launcher will reflect service readiness.
        var hasIdentity = HasDeviceIdentity();
        var startUrl = "http://localhost:5100/auth/login";

        var mainForm = new MainForm(startUrl, openWebViewOnStart: !hasIdentity);
        mainForm.Shown += (_, _) => AppLogger.Info("Main form shown");
        Application.Run(mainForm);
    }

    private static bool HasDeviceIdentity()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "device_identity.dat"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TuColmadoRD", "device_identity.dat")
        };

        return candidatePaths.Any(File.Exists);
    }

}
