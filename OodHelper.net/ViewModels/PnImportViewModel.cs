using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Bulk import of Portsmouth numbers from a CSV file. Replaces the old ODBC "Microsoft Text
    /// Driver" read with CsvHelper, and writes through <see cref="IPortsmouthNumberRepository"/>.
    /// The CSV headers (ClassName, NoOfCrew, Rig, Spinnaker, Engine, Keel, Number, Status, Notes)
    /// map straight onto <see cref="PortsmouthNumber"/> properties.
    /// </summary>
    public partial class PnImportViewModel : ObservableObject
    {
        private readonly IPortsmouthNumberRepository _repository;
        private readonly IDialogService _dialogs;

        public PnImportViewModel(IPortsmouthNumberRepository repository, IDialogService dialogs)
        {
            _repository = repository;
            _dialogs = dialogs;
        }

        [ObservableProperty]
        private string _fileName;

        [ObservableProperty]
        private ObservableCollection<PortsmouthNumber> _rows;

        [RelayCommand]
        private void Browse()
        {
            var path = _dialogs.PickOpenFile("CSV files (*.csv)|*.csv");
            if (path == null) return;

            FileName = path;
            Rows = new ObservableCollection<PortsmouthNumber>(ReadCsv(path));
        }

        [RelayCommand]
        private void Import()
        {
            if (Rows == null || Rows.Count == 0) return;
            _repository.ReplaceAll(Rows);
        }

        private static List<PortsmouthNumber> ReadCsv(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // The legacy text-driver import tolerated extra/missing columns; keep that.
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                var rows = csv.GetRecords<PortsmouthNumber>().ToList();
                foreach (var row in rows)
                    row.Id = Guid.NewGuid();
                return rows;
            }
        }
    }
}
