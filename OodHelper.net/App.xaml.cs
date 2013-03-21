using System;
using System.Collections.Generic;
using System.Configuration;
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
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogger.LogException(e.Exception);
            System.Windows.MessageBox.Show(e.Exception.Message, "Error",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

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
            //
            // This ensures that the SQL Server Compact DB is created.
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

            if (System.IO.File.Exists(@".\data\settings.sdf"))
            {
                object bottomSeed = DbSettings.GetSetting(DbSettings.settBottomSeed);
                object topSeed = DbSettings.GetSetting(DbSettings.settTopSeed);
                object mysql = DbSettings.GetSetting(DbSettings.settMysql);
                object defaultDiscardProfile = DbSettings.GetSetting(DbSettings.settDefaultDiscardProfile);

                Settings.AddSetting(Settings.settBottomSeed, bottomSeed.ToString());
                Settings.AddSetting(Settings.settTopSeed, topSeed.ToString());
                Settings.AddSetting(Settings.settMysql, mysql.ToString());
                Settings.AddSetting(Settings.settDefaultDiscardProfile, defaultDiscardProfile.ToString());

                System.IO.File.Delete(@".\data\settings.sdf");
            }
        }
    }
}
