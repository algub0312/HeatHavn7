namespace HeatHavnAppProject.ViewModels
{
    using LiveChartsCore;
    using LiveChartsCore.SkiaSharpView;
    using LiveChartsCore.SkiaSharpView.Painting;
    using ReactiveUI;
    using SkiaSharp;
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;
    using CsvHelper;
    using HeatHavnAppProject.Models;
    using System.Collections.Generic;

    public partial class Optimizer : ViewModelBase
    {
        // backing collection & series for the chart
        private readonly ObservableCollection<double> _chartValues;
        private readonly LineSeries<double> _series;

        // Exposed to XAML: Series="{Binding OptimizerSeries}"
        public ISeries[] OptimizerSeries => new[] { _series };

        public enum MetricType { Cost, Emissions }

        // UI pickers
        public ObservableCollection<int> Days    { get; } = new(Enumerable.Range(1, 31));
        public ObservableCollection<int> Hours   { get; } = new(Enumerable.Range(0, 24));
        public ObservableCollection<int> Months  { get; } = new() { 3, 8 };
        public ObservableCollection<MetricType> Metrics { get; } 
            = new() { MetricType.Cost, MetricType.Emissions };

        public string SelectedMetricText => $"{SelectedMetric} Over Time";

        // Selected values
        private int _startDay = 1;     public int StartDay   { get => _startDay;   set => this.RaiseAndSetIfChanged(ref _startDay, value); }
        private int _startHour = 0;    public int StartHour  { get => _startHour;  set => this.RaiseAndSetIfChanged(ref _startHour, value); }
        private int _endDay = 1;       public int EndDay     { get => _endDay;     set => this.RaiseAndSetIfChanged(ref _endDay, value); }
        private int _endHour = 23;     public int EndHour    { get => _endHour;    set => this.RaiseAndSetIfChanged(ref _endHour, value); }
        private int _month = 3;        public int Month      { get => _month;      set => this.RaiseAndSetIfChanged(ref _month, value); }
        private bool _scenario1Enabled = true;  public bool Scenario1Enabled { get => _scenario1Enabled; set => this.RaiseAndSetIfChanged(ref _scenario1Enabled, value); }
        private bool _scenario2Enabled;         public bool Scenario2Enabled { get => _scenario2Enabled; set => this.RaiseAndSetIfChanged(ref _scenario2Enabled, value); }
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

        // The command bound to your Optimize button
        public ReactiveCommand<Unit, Unit> OptimizeCommand { get; }

        public Optimizer(SourceDataManagerViewModel source)
        {
            _source       = source ?? throw new ArgumentNullException(nameof(source));
            _assetManager = new AssetManager();

            // 1) initialize chart data & series
            _chartValues = new ObservableCollection<double>();
            _series = new LineSeries<double>
            {
                Values       = _chartValues,
                GeometrySize = 5,
                Stroke       = new SolidColorPaint(SKColors.OrangeRed, 2),
                Fill         = null
            };
            // start empty
            _chartValues.Clear();

            // 2) wire up the command so it writes & reloads CSV on each click
            OptimizeCommand = ReactiveCommand.CreateFromTask(OptimizeAsync);
        }

      private async Task OptimizeAsync()
{
    try
    {
        // 1) Compute the lines
        var start  = new DateTime(2024, Month, StartDay,  StartHour, 0, 0);
        var end    = new DateTime(2024, Month, EndDay,    EndHour,   0, 0);
        var source = Month == 8 ? _source.SummerDataHeat : _source.WinterDataHeat;
        var window = source.Where(x => x.Timestamp >= start && x.Timestamp <= end).ToList();

        var units = _assetManager.GetAllUnits()
            .Where(u => u.Name.StartsWith("Gas Boiler") || u.Name.StartsWith("Oil Boiler"))
            .OrderBy(u => SelectedMetric == MetricType.Cost ? u.ProductionCosts : u.CO2Emissions)
            .ToList();

        var lines = window.Select(hour =>
        {
            double demand = hour.Value;
            double filled = 0, sum = 0;
            foreach (var u in units)
            {
                if (filled >= demand) break;
                var take = Math.Min(u.MaxHeat, demand - filled);
                filled += take;
                sum    += take * (SelectedMetric == MetricType.Cost ? u.ProductionCosts : u.CO2Emissions);
            }
            return $"{hour.Timestamp:MM/dd/HH}:00 {sum:0.##}";
        }).ToList();

        // 2) Ensure Data folder exists
        var projectDataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        Directory.CreateDirectory(projectDataDir);

        // 3) Write CSV there
        var csvPath = Path.Combine(projectDataDir, "optimizer_results.csv");
        Console.WriteLine($"[DEBUG] Writing CSV to: {csvPath}");
        File.WriteAllLines(csvPath, lines);

        // 4) Read back immediately to populate the chart
        await LoadOptimizerResults(csvPath);
    }
    catch (Exception ex)
    {
        // Quick logging so we can see if anything went wrong
        var log = Path.Combine(Directory.GetCurrentDirectory(), "Data", "optimizer_error.log");
        File.WriteAllText(log, ex.ToString());
        Console.WriteLine($"[ERROR] {ex}");
        throw;
    }
}



      private async Task LoadOptimizerResults(string path)
{
    if (!File.Exists(path)) return;

    var values = File.ReadAllLines(path)
        .Select(line =>
        {
            var parts = line.Split(' ');
            return double.TryParse(parts[1], out var v) ? v : 0d;
        })
        .ToList();

    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
    {
        _chartValues.Clear();
        foreach (var v in values)
            _chartValues.Add(v);
    });
}

    }
}
