using Avalonia;
using Serilog;
using System;
using System.IO;

namespace CurveEditor;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigureLogging();

        try
        {
            Log.Information("Starting MotorEditor application");
            Log.Information("Log files are written to {LogDirectory}", GetLogDirectory());

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledAppDomainException;
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        var logPath = Path.Combine(
            GetLogDirectory(),
            "motoreditor-.log");

        // Ensure directory exists
        var logDir = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        Log.Logger = new LoggerConfiguration()
    #if DEBUG
            .MinimumLevel.Debug()
    #else
            .MinimumLevel.Information()
    #endif
#if DEBUG
            .WriteTo.Console()
#endif
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static string GetLogDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MotorEditor",
            "logs");
    }

    private static void OnUnhandledAppDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Log.Fatal(exception, "Unhandled AppDomain exception. IsTerminating={IsTerminating}", e.IsTerminating);
        }
        else
        {
            Log.Fatal("Unhandled AppDomain exception of type {ExceptionType}. IsTerminating={IsTerminating}", e.ExceptionObject.GetType(), e.IsTerminating);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if RELEASE
            .WithInterFont();
#endif
#if DEBUG
            .WithInterFont()
            .LogToTrace();
#endif
}
