namespace HeatHavnAppProject.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using CsvHelper;
using ReactiveUI;
using HeatHavnAppProject.Models;
using System.Collections.Generic;

public partial class Optimizer : ViewModelBase
{
    public enum MetricType { Cost, Emissions }

    public ObservableCollection<int> Days { get; } = new(Enumerable.Range(1, 31));
    public ObservableCollection<int> Hours { get; } = new(Enumerable.Range(0, 24));
    public ObservableCollection<int> Months { get; } = new() { 3, 8 };
    public ObservableCollection<MetricType> Metrics { get; } = new() { MetricType.Cost, MetricType.Emissions };
public string SelectedMetricText => $"{SelectedMetric} Over Time";

    private int _startDay = 1;
    public int StartDay
    {
        get => _startDay;
        set => this.RaiseAndSetIfChanged(ref _startDay, value);
    }

    private int _startHour = 0;
    public int StartHour
    {
        get => _startHour;
        set => this.RaiseAndSetIfChanged(ref _startHour, value);
    }

    private int _endDay = 1;
    public int EndDay
    {
        get => _endDay;
        set => this.RaiseAndSetIfChanged(ref _endDay, value);
    }

    private int _endHour = 23;
    public int EndHour
    {
        get => _endHour;
        set => this.RaiseAndSetIfChanged(ref _endHour, value);
    }

    private int _month = 3;
    public int Month
    {
        get => _month;
        set => this.RaiseAndSetIfChanged(ref _month, value);
    }

    private bool _scenario1Enabled = true;
    public bool Scenario1Enabled
    {
        get => _scenario1Enabled;
        set => this.RaiseAndSetIfChanged(ref _scenario1Enabled, value);
    }

    private bool _scenario2Enabled;
    public bool Scenario2Enabled
    {
        get => _scenario2Enabled;
        set => this.RaiseAndSetIfChanged(ref _scenario2Enabled, value);
    }

    private MetricType _selectedMetric = MetricType.Cost;
    public MetricType SelectedMetric
{
    get => _selectedMetric;
    set
    {
        this.RaiseAndSetIfChanged(ref _selectedMetric, value);
        this.RaisePropertyChanged(nameof(SelectedMetricText));
    }
}

    private readonly SourceDataManagerViewModel _source;
    private readonly AssetManager _assetManager;

    public Optimizer(SourceDataManagerViewModel source)
    {
        _source = source;
        _assetManager = new AssetManager();
OptimizeCommand = ReactiveCommand.CreateFromTask(async () =>
{
    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(RunOptimization);
});
    }

    public ReactiveCommand<Unit, Unit> OptimizeCommand { get; }

  private void RunOptimization()
{
    Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
    {
        try
        {
            if (!Scenario1Enabled) return;

            var start = new DateTime(2024, Month, StartDay, StartHour, 0, 0);
            var end = new DateTime(2024, Month, EndDay, EndHour, 0, 0);

            var data = Month == 8 ? _source.SummerDataHeat : _source.WinterDataHeat;

            Console.WriteLine($"🔍 Filtering from {start} to {end} (Month: {Month})");
            Console.WriteLine($"📊 Total entries: {data.Count}");

            var range = data.Where(x => x.Timestamp >= start && x.Timestamp <= end).ToList();

            Console.WriteLine($"✅ Filtered entries for range: {range.Count}");

            var units = _assetManager.GetAllUnits()
                .Where(x => x.Name.StartsWith("Gas Boiler") || x.Name.StartsWith("Oil Boiler"))
                .OrderBy(x => SelectedMetric == MetricType.Cost ? x.ProductionCosts : x.CO2Emissions)
                .ToList();

            var results = new List<string>();

            foreach (var hour in range)
            {
                double demand = hour.Value;
                double total = 0;
                double metricSum = 0;

                foreach (var unit in units)
                {
                    if (total >= demand) break;
                    var available = Math.Min(unit.MaxHeat, demand - total);
                    total += available;
                    metricSum += available * (SelectedMetric == MetricType.Cost ? unit.ProductionCosts : unit.CO2Emissions);
                }

                results.Add($"{hour.Timestamp:MM/dd/HH}:00 {metricSum:0.##}");
            }

            Directory.CreateDirectory("data");
            using var writer = new StreamWriter("data/optimizer_results.csv");
            foreach (var line in results)
                writer.WriteLine(line);

            Console.WriteLine("✅ Optimization complete. Results written to data/optimizer_results.csv");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine($"📄 STACK TRACE: {ex.StackTrace}");
        }
    });
}
}
