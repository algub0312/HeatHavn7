using System;
using System.Collections.ObjectModel;
using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Models;
namespace HeatHavnAppProject.ViewModels;

using System.Diagnostics;
using LiveChartsCore;
using ReactiveUI;

public class ElectricityPricesViewModel : SeasonalViewModelBase
{
    public override string Title => "⚡Electricity Prices";


    public ObservableCollection<TimeSeriesEntry> FilteredElectricityPrices { get; } = new();

    private readonly SourceDataManagerViewModel _source;

    public ElectricityPricesViewModel(SourceDataManagerViewModel sourceData)
    {
        _source = sourceData;

        Debug.WriteLine("🔥 electricity is rising high");
        Debug.WriteLine("Summer data count: " + _source.SummerDataEl.Count);
        Debug.WriteLine("Winter data count: " + _source.WinterDataEl.Count);

        
        UpdateFilteredElectricityPrices();
    }
 protected override void OnSeasonChanged()
    {
        UpdateFilteredElectricityPrices();
    }

    private void UpdateFilteredElectricityPrices()
    {
        FilteredElectricityPrices.Clear();

            var data = SelectedSeason == "Summer" ? _source.SummerDataEl : _source.WinterDataEl;

        foreach (var entry in data)
        {
            FilteredElectricityPrices.Add(entry);
        }
    }

}
