using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using System.Windows.Input;
using HeatHavnAppProject.Models;
using System.IO;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Globalization;

namespace HeatHavnAppProject.ViewModels;

public class Optimizer : ViewModelBase
{
    public override string Title => "📈 Optimizer";
    // Zile disponibile
    public List<int> Days { get; } = Enumerable.Range(1, 31).ToList();
    // Ore disponibile
    public List<int> Hours { get; } = Enumerable.Range(0, 24).ToList();
    // Luni disponibile
    public List<string> Months { get; } = new() { "March", "August" };
    private readonly SourceDataManagerViewModel _sourceDataManager;
    private readonly AssetManager _assetManager;
    public ISeries[] Series { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    // START DATE
    private int _startDay = 1;
    public int StartDay
    {
        get => _startDay;
        set
        {
            this.RaiseAndSetIfChanged(ref _startDay, value);
            UpdateStartDate();
        }
    }

    private double _totalCost;
    public double TotalCost
    {
        get => _totalCost;
        set => this.RaiseAndSetIfChanged(ref _totalCost, value);
    }

    private double _totalCO2;
    public double TotalCO2
    {
        get => _totalCO2;
        set => this.RaiseAndSetIfChanged(ref _totalCO2, value);
    }

    private int _startHour = 0;
    public int StartHour
    {
        get => _startHour;
        set
        {
            this.RaiseAndSetIfChanged(ref _startHour, value);
            UpdateStartDate();
        }
    }

    private string _startMonth = "March";
    public string StartMonth
    {
        get => _startMonth;
        set
        {
            this.RaiseAndSetIfChanged(ref _startMonth, value);
            UpdateStartDate();
        }
    }

    private DateTimeOffset? _startDate = DateTimeOffset.Now;
    public DateTimeOffset? StartDate
    {
        get => _startDate;
        private set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    private void UpdateStartDate()
    {
        var month = StartMonth == "August" ? 8 : 3;
        StartDate = new DateTimeOffset(new DateTime(2024, month, StartDay, StartHour, 0, 0));
    }

    private int _endDay = 1;
    public int EndDay
    {
        get => _endDay;
        set
        {
            this.RaiseAndSetIfChanged(ref _endDay, value);
            UpdateEndDate();
        }
    }

    private int _endHour = 0;
    public int EndHour
    {
        get => _endHour;
        set
        {
            this.RaiseAndSetIfChanged(ref _endHour, value);
            UpdateEndDate();
        }
    }

    private string _endMonth = "March";
    public string EndMonth
    {
        get => _endMonth;
        set
        {
            this.RaiseAndSetIfChanged(ref _endMonth, value);
            UpdateEndDate();
        }
    }

    private DateTimeOffset? _endDate = DateTimeOffset.Now;
    public DateTimeOffset? EndDate
    {
        get => _endDate;
        private set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    private void UpdateEndDate()
    {
        var month = EndMonth == "August" ? 8 : 3;
        EndDate = new DateTimeOffset(new DateTime(2024, month, EndDay, EndHour, 0, 0));
    }


    private bool _scenario1Enabled;
    public bool Scenario1Enabled
    {
        get => _scenario1Enabled;
        set => this.RaiseAndSetIfChanged(ref _scenario1Enabled, value);
    }

    private string _selectedOptimization = "Cost";
    public string SelectedOptimization
    {
        get => _selectedOptimization;
        set => this.RaiseAndSetIfChanged(ref _selectedOptimization, value);
    }

    public ICommand OptimizeCommand { get; }

    public Optimizer(SourceDataManagerViewModel sourceDataManager, AssetManager assetManager)
    {
        _sourceDataManager = sourceDataManager;
        _assetManager = assetManager;
        OptimizeCommand = new RelayCommand(() =>
{
    Console.WriteLine("Starting optimization...");

    // Selectam data de heat demand
    List<TimeSeriesEntry> heatDemandEntries = StartMonth switch
    {
        "March" => _sourceDataManager.WinterDataHeat.ToList(),
        "August" => _sourceDataManager.SummerDataHeat.ToList(),
        _ => throw new InvalidOperationException("Invalid month selected!")
    };

    if (heatDemandEntries == null || heatDemandEntries.Count == 0)
    {
        Console.WriteLine("Heat demand data is missing!");
        return;
    }

    // Luam ProductionUnits
    List<ProductionUnit> productionUnits = _assetManager.GetAllUnits();

    DateTime startDate = StartDate.Value.DateTime;
    DateTime endDate = EndDate.Value.DateTime;
    var outputCsvPath = "Data/optimization_results.csv";



    if (SelectedOptimization == "Cost")
    {
        Console.WriteLine("Optimizing by COST...");
        OptimizeCostScenario1AndSaveToCsv(productionUnits, heatDemandEntries, startDate, endDate, outputCsvPath);
    }
    else if (SelectedOptimization == "Emissions")
    {
        Console.WriteLine("Optimizing by EMISSIONS...");
        OptimizeEmissionsScenario1AndSaveToCsv(productionUnits, heatDemandEntries, startDate, endDate, outputCsvPath);
    }
    else
    {
        Console.WriteLine("Unknown optimization type!");
        return;
    }

    LoadGraphFromCsv(outputCsvPath);
    Console.WriteLine("Optimization finished and saved to CSV!");
});

    }
    private void OptimizeCostScenario1AndSaveToCsv(
        List<ProductionUnit> units,
        List<TimeSeriesEntry> demandEntries,
        DateTime startDate,
        DateTime endDate,
        string outputCsvPath)
    {

        double totalCost = 0;
        double totalCO2 = 0;

        var sortedUnits = units
            .Where(u => u.EnergyType == "Gas" || u.EnergyType == "Oil") // doar boilere
            .OrderBy(u => u.ProductionCosts)
            .ToList();
        Debug.WriteLine($"Found {sortedUnits.Count} production units.");
        var results = new List<(DateTime TimeFrom, double GB1, double GB2, double OB1, double HeatDemand, double hourlyCO2)>();

        foreach (var demand in demandEntries.Where(d => d.Timestamp >= startDate && d.Timestamp < endDate))
        {
            double heatNeeded = demand.Value;
            double gb1 = 0, gb2 = 0, ob1 = 0;
            double hourlyCO2 = 0;
            double hourlyCost = 0;

            foreach (var unit in sortedUnits)
            {
                if (heatNeeded <= 0)
                    break;

                double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

                if (unit.Name.Contains("Gas Boiler 1"))
                    gb1 += heatFromUnit;
                else if (unit.Name.Contains("Gas Boiler 2"))
                    gb2 += heatFromUnit;
                else if (unit.Name.Contains("Oil Boiler 1"))
                    ob1 += heatFromUnit;
                hourlyCO2 += heatFromUnit * unit.CO2Emissions;
                heatNeeded -= heatFromUnit;

                hourlyCost += heatFromUnit * unit.ProductionCosts;


            }
            totalCost += hourlyCost;
            totalCO2 += hourlyCO2;
            results.Add((demand.Timestamp, gb1, gb2, ob1, demand.Value, totalCO2));


        }

        var directory = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Console.WriteLine($"✅ Created missing directory: {directory}");
        }

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("TimeFrom,GB1,GB2,OB1,HeatDemand,CO2");
            foreach (var result in results)
            {
                writer.WriteLine($"{result.TimeFrom:yyyy-MM-dd HH:mm},{result.GB1:F2},{result.GB2:F2},{result.OB1:F2},{result.HeatDemand:F2},{result.hourlyCO2:F2}");
            }
        }

        Console.WriteLine($"✅ Optimization results saved to {outputCsvPath}");

        TotalCost = totalCost;     // dacă ai o variabilă calculată
        TotalCO2 = totalCO2;
    }



    private void OptimizeEmissionsScenario1AndSaveToCsv(
    List<ProductionUnit> units,
    List<TimeSeriesEntry> demandEntries,
    DateTime startDate,
    DateTime endDate,
    string outputCsvPath)
    {
        double totalCost = 0;
        double totalCO2 = 0;
        var sortedUnits = units
            .Where(u => u.EnergyType == "Gas" || u.EnergyType == "Oil") // doar boilere
            .OrderBy(u => u.CO2Emissions) // 🔥 AICI schimbăm criteriul de sortare!
            .ToList();

        Debug.WriteLine($"Found {sortedUnits.Count} production units (sorted by CO2 emissions).");

        var results = new List<(DateTime TimeFrom, double GB1, double GB2, double OB1, double HeatDemand, double hourlyCO2)>();

        foreach (var demand in demandEntries.Where(d => d.Timestamp >= startDate && d.Timestamp < endDate))
        {
            double heatNeeded = demand.Value;
            double gb1 = 0, gb2 = 0, ob1 = 0;
            double hourlyCO2 = 0;
            double hourlyCost = 0;
            foreach (var unit in sortedUnits)
            {
                if (heatNeeded <= 0)
                    break;

                double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

                if (unit.Name.Contains("Gas Boiler 1"))
                    gb1 += heatFromUnit;
                else if (unit.Name.Contains("Gas Boiler 2"))
                    gb2 += heatFromUnit;
                else if (unit.Name.Contains("Oil Boiler 1"))
                    ob1 += heatFromUnit;
                hourlyCO2 += heatFromUnit * unit.CO2Emissions;
                hourlyCost += heatFromUnit * unit.ProductionCosts;

                heatNeeded -= heatFromUnit;
            }
            totalCO2 += hourlyCO2;
            totalCost += hourlyCost;
            results.Add((demand.Timestamp, gb1, gb2, ob1, demand.Value, totalCO2));
        }

        var directory = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("TimeFrom,GB1,GB2,OB1,HeatDemand,CO2");
            foreach (var result in results)
            {
                writer.WriteLine($"{result.TimeFrom:yyyy-MM-dd HH:mm},{result.GB1:F2},{result.GB2:F2},{result.OB1:F2},{result.HeatDemand:F2},{result.hourlyCO2:F2}");
            }
        }

        Console.WriteLine($"✅ Emissions optimization results saved to {outputCsvPath}");
        TotalCO2 = totalCO2;
        TotalCost = totalCost;

    }


    private void LoadGraphFromCsv(string filePath)
    {
        Console.WriteLine("Loading graph from CSV...");

        var gb1 = new List<double>();
        var gb2 = new List<double>();
        var ob1 = new List<double>();
        var heatDemand = new List<double>();
        var timestamps = new List<string>();
        var co2Values = new List<double>();


        if (!File.Exists(filePath))
        {
            Console.WriteLine("CSV file missing for graph!");
            return;
        }

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines.Skip(1)) // skip header
        {
            var parts = line.Split(',');

            if (parts.Length >= 5)
            {
                timestamps.Add(DateTime.Parse(parts[0]).ToString("HH:mm"));
                gb1.Add(double.Parse(parts[1], CultureInfo.InvariantCulture));
                gb2.Add(double.Parse(parts[2], CultureInfo.InvariantCulture));
                ob1.Add(double.Parse(parts[3], CultureInfo.InvariantCulture));
                heatDemand.Add(double.Parse(parts[4], CultureInfo.InvariantCulture));


            }
        }

        Series = new ISeries[]
        {
        new StackedAreaSeries<double>
        {
            Name = "GB1",
            Values = gb1,


        },
        new StackedAreaSeries<double>
        {
            Name = "GB2",
            Values = gb2,

        },
        new StackedAreaSeries<double>
        {
            Name = "OB1",
            Values = ob1,

        },
        new LineSeries<double>
        {
            Name = "Heat Demand",
        Values = heatDemand,
        Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 2),
        Fill = null,
        GeometryFill = new SolidColorPaint(SKColors.White),
        GeometryStroke = new SolidColorPaint(SKColors.DeepSkyBlue, 2),
        GeometrySize = 10



}

        };

        XAxes = new[]
        {
        new Axis
        {
            Labels = timestamps,
            LabelsRotation = 45,
            TextSize = 10
        }
    };

        YAxes = new[]
        {
        new Axis
        {
            Name = "MW",
            TextSize = 10
        }
    };

        this.RaisePropertyChanged(nameof(Series));
        this.RaisePropertyChanged(nameof(XAxes));
        this.RaisePropertyChanged(nameof(YAxes));
    }
}