using System;
using System.Collections.ObjectModel;
using HeatHavnAppProject.ViewModels;
namespace HeatHavnAppProject.ViewModels;

using System.Diagnostics;
using LiveChartsCore;
using ReactiveUI;

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
            UpdateFilteredHeatDemand(); // actualizează lista
        }
    }

    public ObservableCollection<string> Seasons { get; } = new() { "Summer", "Winter" };


    public ObservableCollection<TimeSeriesEntry> FilteredHeatDemand { get; } = new();

    private readonly SourceDataManagerViewModel _source;

    public HeatDemandViewModel(SourceDataManagerViewModel sourceData)
    {
        _source = sourceData;

        Debug.WriteLine("🔥 HeatDemandViewModel constructor started!");
        Debug.WriteLine("Summer data count: " + _source.SummerData.Count);
        Debug.WriteLine("Winter data count: " + _source.WinterData.Count);

        SelectedSeason = "Summer";
        UpdateFilteredHeatDemand();
    }

    private void UpdateFilteredHeatDemand()
    {
        FilteredHeatDemand.Clear();

        var data = SelectedSeason == "Summer"
            ? _source.SummerData
            : _source.WinterData;

        foreach (var entry in data)
            FilteredHeatDemand.Add(entry);

        Debug.WriteLine($"🟢 Updated FilteredHeatDemand with {FilteredHeatDemand.Count} entries for {SelectedSeason}");
    }


}
