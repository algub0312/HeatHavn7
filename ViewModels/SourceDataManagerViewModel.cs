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
    public ObservableCollection<TimeSeriesEntry> SummerData { get; } = new();
    public ObservableCollection<TimeSeriesEntry> WinterData { get; } = new();

    public void LoadSummerData(string filePath)
{
    var raw = LoadCsvRaw(filePath);
    SummerData.Clear();
    foreach (var row in raw)
        SummerData.Add(new TimeSeriesEntry
        {
            Timestamp = row.TimeFrom,
            Value = row.HeatDemand
        });
}
    public void LoadWinterData(string filePath)
    {
        var raw = LoadCsvRaw(filePath);
        WinterData.Clear();
        foreach (var row in raw)
            WinterData.Add(new TimeSeriesEntry
            {
                Timestamp = row.TimeFrom,
                Value = row.HeatDemand
            });
           
    }

   private List<RawSourceDataEntry> LoadCsvRaw(string path)
{
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return csv.GetRecords<RawSourceDataEntry>().ToList();
}
}
