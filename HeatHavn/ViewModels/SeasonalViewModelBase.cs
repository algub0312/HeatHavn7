using System.Collections.ObjectModel;
using Avalonia.Controls;
using ReactiveUI;

namespace HeatHavnAppProject.ViewModels;

public abstract class SeasonalViewModelBase : ViewModelBase
{
    private string _selectedSeason = "Summer";
    public string SelectedSeason
    {
        get => _selectedSeason;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSeason, value);
            OnSeasonChanged();
        }
    }

    public ObservableCollection<string> Seasons { get; } = new() { "Summer", "Winter" };

    protected abstract void OnSeasonChanged(); // for child to implement
}
