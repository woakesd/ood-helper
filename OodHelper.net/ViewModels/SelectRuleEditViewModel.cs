using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Rules;

namespace OodHelper.ViewModels
{
    public partial class SelectRuleEditViewModel : ObservableObject
    {
        private readonly ISelectRuleRepository _repository;
        private readonly BoatSelectRule _root;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool> CloseRequested;

        [ObservableProperty]
        private string _ruleName;

        public ObservableCollection<SelectRuleNodeViewModel> Rules { get; }

        public SelectRuleEditViewModel(ISelectRuleRepository repository, Guid? id)
        {
            _repository = repository;

            if (id.HasValue)
            {
                _root = repository.GetTree(id.Value);
                RuleName = _root.Name;
            }
            else
            {
                _root = new BoatSelectRule { Application = Apply.Any };
                _root.Add(new BoatSelectRule { Condition = ConditionType.Equals });
            }

            Rules = new ObservableCollection<SelectRuleNodeViewModel>
            {
                new SelectRuleNodeViewModel(_root)
            };
        }

        [RelayCommand]
        private void Save()
        {
            _root.Name = RuleName;
            _repository.Save(_root);
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }
    }
}
