using System;
using System.Collections.ObjectModel;
using HeatHavnAppProject.ViewModels;
namespace HeatHavnAppProject.ViewModels;
using LiveChartsCore;
public class HeatDemandViewModel : ViewModelBase
{
    public override string Title => "Heat Demand";

    public ObservableCollection<string> AvailableSeasons { get; } = new() { "Summer", "Winter" };

    private string _selectedSeason = "Summer";
    public string SelectedSeason
    {
        get => _selectedSeason;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSeason, value);
            UpdateFilteredHeatDemand();
        }
    }

    private void RaiseAndSetIfChanged(ref string selectedSeason, string value)
    {
        throw new NotImplementedException();
    }

    public ObservableCollection<TimeSeriesEntry> FilteredHeatDemand { get; } = new();

    private readonly SourceDataManagerViewModel _source;

    public HeatDemandViewModel(SourceDataManagerViewModel sourceData)
    {
        _source = sourceData;
        UpdateFilteredHeatDemand(); // load initial data
    }

    private void UpdateFilteredHeatDemand()
    {
        FilteredHeatDemand.Clear();

        var source = SelectedSeason == "Summer"
            ? _source.SummerData
            : _source.WinterData;

        foreach (var item in source)
            FilteredHeatDemand.Add(item);
    }
}
