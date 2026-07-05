using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OodHelper.Results
{
    /// <summary>
    ///     Interaction logic for ResultsEditor.xaml.
    /// </summary>
    /// <remarks>
    ///     The editor is now a thin view bound to a <see cref="ResultsEditorViewModel"/>. The
    ///     forwarding accessors below keep the small public surface that sibling screens still
    ///     depend on during coexistence: the print pages (<c>OpenHandicapResultsPage</c> /
    ///     <c>RollingHandicapResultsPage</c>), <c>SelectBoats</c>, and the <c>RaceResults</c>
    ///     host commands. Print-selection state stays on the view because
    ///     <c>ResultsPrintSelector</c> binds the editors as <see cref="IPrintSelectItem"/>.
    /// </remarks>
    public partial class ResultsEditor : IPrintSelectItem
    {
        private readonly ResultsEditorViewModel _vm;

        public ResultsEditor(ResultsEditorViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            PrintIncludeCopies = 1;
            DataContext = _vm;
        }

        public ResultsEditorViewModel ViewModel => _vm;

        // -- Forwarding accessors (consumed by print pages / SelectBoats / RaceResults) -----

        public int Rid => _vm.Rid;
        public string RaceName => _vm.RaceName;
        public string RaceClass => _vm.RaceClass;
        public DateTime StartDate => _vm.StartDate;
        public TimeSpan StartTime => _vm.StartTime;
        public string Ood => _vm.Ood;
        public string Handicap => _vm.Handicap;
        public IRaceScore? Scorer => _vm.Scorer;
        public IReadOnlyList<ResultRowViewModel> Rows => _vm.Rows;

        public void LoadGrid() => _vm.Load();
        public int CountAutoPopulateData() => _vm.CountAutoPopulateData();
        public void DoAutoPopulate() => _vm.DoAutoPopulate();

        // -- IPrintSelectItem ----------------------------------------------------------------

        public bool PrintIncludeAllVisible { get; set; }
        public bool PrintIncludeAll { get; set; }
        public bool PrintInclude { get; set; }
        public int PrintIncludeCopies { get; set; }

        public string PrintIncludeDescription
        {
            get { return _vm.RaceName; }
            set { }
        }

        public int PrintIncludeGroup { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
