using System;
using System.Windows;
using System.Windows.Controls;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
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
            throw new Exception("Aha");
        }

        static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogger.LogException(e.Exception);
            Current.Shutdown();
        }
    }
}
