using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Data.Entities;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Editor for a single Portsmouth-number class. Replaces the old <c>HandicapRecord</c> +
    /// code-behind; all persistence goes through <see cref="IPortsmouthNumberRepository"/>.
    /// </summary>
    public partial class HandicapEditViewModel : ObservableObject
    {
        private readonly IPortsmouthNumberRepository _repository;
        private Guid _id;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool>? CloseRequested;

        /// <summary>Id of the saved class (set after Save), so the list can scroll to it.</summary>
        public Guid Id => _id;

        [ObservableProperty]
        private string? _className;

        [ObservableProperty]
        private int? _noOfCrew;

        [ObservableProperty]
        private string? _rig;

        [ObservableProperty]
        private string? _spinnaker;

        [ObservableProperty]
        private string? _engine;

        [ObservableProperty]
        private string? _keel;

        [ObservableProperty]
        private int? _number;

        [ObservableProperty]
        private string? _status;

        [ObservableProperty]
        private string? _notes;

        public HandicapEditViewModel(IPortsmouthNumberRepository repository, Guid? id)
        {
            _repository = repository;

            if (id.HasValue)
            {
                var pn = repository.Get(id.Value);
                if (pn != null)
                {
                    _id = pn.Id;
                    ClassName = pn.ClassName;
                    NoOfCrew = pn.NoOfCrew;
                    Rig = pn.Rig;
                    Spinnaker = pn.Spinnaker;
                    Engine = pn.Engine;
                    Keel = pn.Keel;
                    Number = pn.Number;
                    Status = pn.Status;
                    Notes = pn.Notes;
                }
            }
        }

        [RelayCommand]
        private void Save()
        {
            var pn = new PortsmouthNumber
            {
                Id = _id,
                ClassName = ClassName,
                NoOfCrew = NoOfCrew,
                Rig = Rig,
                Spinnaker = Spinnaker,
                Engine = Engine,
                Keel = Keel,
                Number = Number,
                Status = Status,
                Notes = Notes
            };

            _repository.Save(pn);
            _id = pn.Id;
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }
    }
}
