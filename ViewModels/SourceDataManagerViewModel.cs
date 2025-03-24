using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeatHavnAppProject.Models;
using HeatHavnAppProject.ViewModels;

public class SourceDataManagerViewModel : ViewModelBase
{
    public ObservableCollection<TimeSeriesEntry> HeatDemandSeries { get; } = new();
    public ObservableCollection<TimeSeriesEntry> ElectricityPriceSeries { get; } = new();

    public async Task LoadHeatDemandAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        HeatDemandSeries.Clear();
        foreach (var line in lines.Skip(1)) // assuming header
        {
            var parts = line.Split(',');
            HeatDemandSeries.Add(new TimeSeriesEntry
            {
                Time = DateTime.Parse(parts[0]),
                Value = double.Parse(parts[1], CultureInfo.InvariantCulture)
            });
        }
    }

    // Same for electricity prices...
}
