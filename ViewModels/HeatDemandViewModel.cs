using System;
using System.Collections.ObjectModel;
using HeatHavnAppProject.ViewModels;
namespace HeatHavnAppProject.ViewModels;

using System.Diagnostics;
using LiveChartsCore;
using ReactiveUI;

public class HeatDemandViewModel : SeasonalViewModelBase
{
    public override string Title => "🔥Heat Demand";

    public ObservableCollection<TimeSeriesEntry> FilteredHeatDemand { get; } = new();

    private readonly SourceDataManagerViewModel _source;

    public HeatDemandViewModel(SourceDataManagerViewModel sourceData)
    {
        _source = sourceData;

        Debug.WriteLine("🔥 HeatDemandViewModel constructor started!");
        Debug.WriteLine("Summer data count: " + _source.SummerDataHeat.Count);
        Debug.WriteLine("Winter data count: " + _source.WinterDataHeat.Count);

        
        UpdateFilteredHeatDemand();
    }
    protected override void OnSeasonChanged()
    {
        UpdateFilteredHeatDemand();
    }


    private void UpdateFilteredHeatDemand()
    {
         FilteredHeatDemand.Clear();
        var data = SelectedSeason == "Summer" ? _source.SummerDataHeat : _source.WinterDataHeat;
        foreach (var entry in data)
            FilteredHeatDemand.Add(entry);

        Debug.WriteLine($"🟢 Updated FilteredHeatDemand with {FilteredHeatDemand.Count} entries for {SelectedSeason}");
    }


}
