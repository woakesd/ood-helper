using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data;
using OodHelper.Services;
using OodHelper.ViewModels;
using Uno.Resizetizer;

namespace OodHelper.Uno;

public partial class App : Application
{
    /// <summary>
    /// Application-wide service provider. Mirrors the WPF app's <c>App.Services</c> pattern so the
    /// tab shell (code-behind) can resolve screen view-models on demand. Built by the Uno.Extensions host.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>The main window, used by <see cref="Services.DialogService"/> to reach a live XamlRoot.</summary>
    public static Window? MainWindowInstance { get; private set; }

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .ConfigureServices((context, services) => ConfigureServices(services))
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;
        MainWindowInstance = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
        Services = Host.Services;

        // The database is code-first over SQLite (schema owned by EF migrations), living in a
        // per-user LocalApplicationData location. Create the folder, apply migrations, then seed a
        // little demo data so the skeleton screens have rows to show on first run.
        InitializeDatabase(Services);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        //
        // EF Core context factory (short-lived contexts per unit of work), mirroring the WPF app.
        // The connection string is owned by SqliteConfig.
        //
        services.AddDbContextFactory<OodHelperContext>(opt =>
            opt.UseSqlite(SqliteConfig.ConnectionString));

        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ISeriesRepository, SeriesRepository>();
        services.AddSingleton<IPortsmouthNumberRepository, PortsmouthNumberRepository>();

        //
        // Screen view-models. The two editor view-models need a runtime id, so they are created
        // through factory delegates (as in the WPF app), while the list/picker view-models resolve
        // directly.
        //
        services.AddTransient<HandicapsViewModel>();
        services.AddTransient<SeriesViewModel>();
        services.AddTransient<SelectClassViewModel>();
        services.AddTransient<Func<int, SeriesEditViewModel>>(sp => sid =>
            new SeriesEditViewModel(
                sp.GetRequiredService<ISeriesRepository>(),
                sp.GetRequiredService<IDialogService>(),
                sid));
        services.AddTransient<Func<int, SeriesRaceSelectViewModel>>(sp => sid =>
            new SeriesRaceSelectViewModel(sp.GetRequiredService<ISeriesRepository>(), sid));
    }

    private static void InitializeDatabase(IServiceProvider services)
    {
        // Ensure the native SQLite provider is registered (bundle_e_sqlite3). Microsoft.Data.Sqlite
        // usually auto-initializes, but calling this explicitly is harmless and covers all heads.
        SQLitePCL.Batteries_V2.Init();

        Directory.CreateDirectory(SqliteConfig.DatabaseFolder);

        using var ctx = services.GetRequiredService<IDbContextFactory<OodHelperContext>>().CreateDbContext();
        ctx.Database.Migrate();
        DevDataSeeder.Seed(ctx);
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<MainPage, MainViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Main", View: views.FindByViewModel<MainViewModel>(), IsDefault: true),
                ]
            )
        );
    }
}
