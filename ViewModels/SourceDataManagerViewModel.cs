using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using HeatHavnAppProject.ViewModels;
namespace HeatHavnAppProject.ViewModels;
using CsvHelper.Configuration;
using System;

    

public class SourceDataManagerViewModel : ViewModelBase
{
    public ObservableCollection<TimeSeriesEntry> SummerDataHeat { get; } = new();
    public ObservableCollection<TimeSeriesEntry> WinterDataHeat { get; } = new();
    public ObservableCollection<TimeSeriesEntry> SummerDataEl { get; } = new();
    public ObservableCollection<TimeSeriesEntry> WinterDataEl { get; } = new();
    public void LoadSummerDataHeat(string filePath)
{
    var raw = LoadCsvRaw(filePath);
    SummerDataHeat.Clear();
    foreach (var row in raw)
        SummerDataHeat.Add(new TimeSeriesEntry
        {
            Timestamp = row.TimeFrom,
            Value = row.HeatDemand
        });
}
    public void LoadWinterDataHeat(string filePath)
    {
        var raw = LoadCsvRaw(filePath);
        WinterDataHeat.Clear();
        foreach (var row in raw)
            WinterDataHeat.Add(new TimeSeriesEntry
            {
                Timestamp = row.TimeFrom,
                Value = row.HeatDemand
            });
           
    }

 public void LoadSummerDataEl(string filePath)
{
    var raw = LoadCsvRaw(filePath);
    SummerDataEl.Clear();
    foreach (var row in raw)
        SummerDataEl.Add(new TimeSeriesEntry
        {
            Timestamp= row.TimeFrom,
            Value = row.ElectricityPrice
        });
}
    public void LoadWinterDataEl(string filePath)
    {
        var raw = LoadCsvRaw(filePath);
        WinterDataEl.Clear();
        foreach (var row in raw)
            WinterDataEl.Add(new TimeSeriesEntry
            {
                Timestamp = row.TimeFrom,
                Value = row.ElectricityPrice
            });
           
    }


   private List<RawSourceDataEntry> LoadCsvRaw(string path)
{
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return csv.GetRecords<RawSourceDataEntry>().ToList();
}
}
