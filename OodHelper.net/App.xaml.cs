using System;
using System.Windows;
using System.Windows.Controls;
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
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ITabHost, TabHost>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDatabaseMaintenanceService, DatabaseMaintenanceService>();
            services.AddSingleton<Data.IBoatRepository, Data.BoatRepository>();

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
