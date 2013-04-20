using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataGrid d = sender as DataGrid;
            if (d != null)
            {
                foreach (DataGridColumn c in d.Columns)
                {
                    c.SetValue(FrameworkElement.DataContextProperty, e.NewValue);
                }
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //
            // This ensures that the SQL Server DB is created.
            //
            Db C = new Db("SELECT 1");
            C.Dispose();
            //
            // This allows DataColumns to have DataContext properties as per their DataGrid.
            //
            FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn));

            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(DataGrid),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                    new PropertyChangedCallback(OnDataContextChanged)));
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogger.LogException(e.Exception);
            ShowException.DoShow(e.Exception);
            App.Current.Shutdown();
        }
    }
}
