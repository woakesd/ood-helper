using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Data;
using OodHelper.Services;

namespace OodHelper.Sun
{
    /// <summary>
    ///     Interaction logic for ReadData.xaml
    /// </summary>
    public partial class DoSunSetRise
    {
        private readonly Data _dataContext = new Data();

        public DoSunSetRise()
        {
            CalculateSunData();
            DataContext = _dataContext;
        }

        private void CalculateSunData()
        {
            _dataContext.SunRiseTable = new DataTable();

            _dataContext.SunRiseTable.Columns.Add("date", typeof (DateTime));
            _dataContext.SunRiseTable.Columns.Add("sunrise", typeof (DateTime));
            _dataContext.SunRiseTable.Columns.Add("sunset", typeof (DateTime));
            InitializeComponent();
            var calc = new Sun(55.996700991558, -3.409237861633301);
            var workDate = new DateTime(Int32.Parse((Year.SelectedItem as ComboBoxItem)!.Tag as string ?? "0"), 1, 1);
            var endData = workDate.AddYears(1);
            while (workDate < endData)
            {
                DateTime? rise, set;
                calc.Calc(workDate, out rise, out set);
                var dr = _dataContext.SunRiseTable.NewRow();
                dr["date"] = workDate;
                dr["sunrise"] = rise;
                dr["sunset"] = set;
                _dataContext.SunRiseTable.Rows.Add(dr);
                workDate = workDate.AddDays(1);
            }
        }

        private async void UploadSun_Click(object sender, RoutedEventArgs e)
        {
            var dialogs = App.Services.GetRequiredService<IDialogService>();
            var uploader = App.Services.GetRequiredService<ISunTideUploadService>();
            try
            {
                var completed = await dialogs.ShowProgressAsync("Uploading Sun Data",
                    (progress, ct) => uploader.UploadSunAsync(_dataContext.SunRiseTable, progress, ct));
                dialogs.ShowInformation(completed ? "Sun Data Upload Complete" : "Sun Data Upload Cancelled",
                    completed ? "Finished" : "Cancel");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
                dialogs.ShowError("Sun Data Upload Failed", "Failed");
            }
        }

        private void Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateSunData();
            _dataContext.OnPropertyChanged("SunDataView");
        }

        private class Data : NotifyPropertyChanged
        {
            public DataTable SunRiseTable = new DataTable();

            public DataView SunDataView
            {
                get { return SunRiseTable.DefaultView; }
            }
        }
    }
}