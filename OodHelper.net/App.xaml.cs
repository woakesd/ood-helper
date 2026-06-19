using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Services;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static IServiceProvider Services { get; private set; }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var d = sender as DataGrid;
            if (d == null) return;
            foreach (var c in d.Columns)
            {
                c.SetValue(FrameworkElement.DataContextProperty, e.NewValue);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //
            // This ensures that the SQL Server DB is created.
            //
            var db = new Db("SELECT 1");
            db.Dispose();
            //
            // This allows DataColumns to have DataContext properties as per their DataGrid.
            //
            FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn));

            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(DataGrid),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                    OnDataContextChanged));

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            var main = Services.GetRequiredService<OodHelperWindow>();
            main.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //
            // EF Core context factory. A factory (rather than a scoped context) suits WPF:
            // there is no per-request scope, so each unit of work creates a short-lived
            // context, mirroring the existing `using (Db ...)` pattern. The connection string
            // is shared with the legacy Db helper during coexistence.
            //
            services.AddDbContextFactory<Data.OodHelperContext>(opt =>
                opt.UseSqlServer(Db.DatabaseConstr));

            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ITabHost, TabHost>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDatabaseMaintenanceService, DatabaseMaintenanceService>();
            services.AddSingleton<Data.IBoatRepository, Data.BoatRepository>();
            services.AddSingleton<Data.IRaceResultsRepository, Data.RaceResultsRepository>();

            //
            // The race-results view-models need a runtime race id, so they are created through
            // factory delegates rather than direct DI resolution. The editor factory loads its
            // data eagerly so RaceResults can inspect the rows (e.g. for auto-populate) on open.
            //
            services.AddTransient<Func<int, Results.ResultsEditorViewModel>>(sp => rid =>
            {
                var vm = new Results.ResultsEditorViewModel(rid, sp.GetRequiredService<Data.IRaceResultsRepository>());
                vm.Load();
                return vm;
            });
            services.AddTransient<Func<int[], Results.RaceResultsViewModel>>(sp => rids =>
                new Results.RaceResultsViewModel(rids,
                    sp.GetRequiredService<Data.IRaceResultsRepository>(),
                    sp.GetRequiredService<Func<int, Results.ResultsEditorViewModel>>()));
            services.AddTransient<Func<int[], Results.RaceResults>>(sp => rids =>
                new Results.RaceResults(sp.GetRequiredService<Func<int[], Results.RaceResultsViewModel>>()(rids)));

            services.AddTransient<OodHelperWindowViewModel>();
            services.AddSingleton<OodHelperWindow>();
            services.AddTransient<BoatsViewModel>();
            services.AddTransient<Maintain.Boats>();
            services.AddTransient<ConfigurationViewModel>();
            services.AddTransient<Configure>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            (Services as IDisposable)?.Dispose();
            base.OnExit(e);
        }

        static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogger.LogException(e.Exception);
            Current.Shutdown();
        }
    }
}
