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
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;

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
    private string _lastOutputPath = "Data/optimization_results.csv";
    public ICommand SaveCsvCommand { get; }
    private readonly SourceDataManagerViewModel _sourceDataManager;
    private readonly AssetManager _assetManager;
    public ISeries[] Series { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }
    public ObservableCollection<ProductionUnit> AvailableUnits { get; set; }


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
    private bool _scenario2Enabled;
    public bool Scenario2Enabled
    {
        get => _scenario2Enabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _scenario2Enabled, value);
            if (value)
            {
                Scenario1Enabled = false;
                IsCustomScenarioSelected = false;
            }
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
    private string _saveStatus = "";
    public string SaveStatus
    {
        get => _saveStatus;
        set => this.RaiseAndSetIfChanged(ref _saveStatus, value);
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
        set
        {
            this.RaiseAndSetIfChanged(ref _scenario1Enabled, value);
            if (value)
            {
                Scenario2Enabled = false;
                IsCustomScenarioSelected = false;
            }
        }
    }

    private string _selectedOptimization = "Cost";
    public string SelectedOptimization
    {
        get => _selectedOptimization;
        set => this.RaiseAndSetIfChanged(ref _selectedOptimization, value);
    }

    private bool _isCustomScenarioSelected;
    public bool IsCustomScenarioSelected
    {
        get => _isCustomScenarioSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _isCustomScenarioSelected, value);
            if (value)
            {
                Scenario1Enabled = false;
                Scenario2Enabled = false;
            }
        }
    }


    public ICommand OptimizeCommand { get; }

    public Optimizer(SourceDataManagerViewModel sourceDataManager, AssetManager assetManager, AssetManagerViewModel assetManagerViewModel)
    {
        _lastOutputPath = "Data/optimization_results.csv"; // default output path

        _sourceDataManager = sourceDataManager;
        _assetManager = assetManager;
        AvailableUnits = assetManagerViewModel.AllUnits;
        Debug.WriteLine($"DEBUG: AvailableUnits count = {AvailableUnits.Count}");
        foreach (var unit in AvailableUnits)
        {
            Debug.WriteLine($"DEBUG Unit: {unit.Name}");
        }
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
    List<TimeSeriesEntry> electricityEntries = StartMonth switch
    {
        "March" => _sourceDataManager.WinterDataEl.ToList(),
        "August" => _sourceDataManager.SummerDataEl.ToList(),
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
    var outputCsvPath = _lastOutputPath;


    if (Scenario1Enabled)
    {
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
    }
    else if (Scenario2Enabled)
    {
        if (SelectedOptimization == "Cost")
            OptimizeCostScenario2AndSaveToCsv(productionUnits, heatDemandEntries, electricityEntries, startDate, endDate, outputCsvPath);

        else if (SelectedOptimization == "Emissions")
            OptimizeEmissionsScenario2AndSaveToCsv(productionUnits, heatDemandEntries, electricityEntries, startDate, endDate, outputCsvPath);

    }
    else if (IsCustomScenarioSelected)
    {
        var selectedUnits = AvailableUnits.Where(u => u.IsSelected).ToList();

        if (!selectedUnits.Any())
        {
            Console.WriteLine("No units selected for Custom Scenario!");
            return;
        }

        if (SelectedOptimization == "Cost")
        {
            Console.WriteLine("Optimizing Custom Scenario by COST...");
            OptimizeCostScenarioCustomAndSaveToCsv(selectedUnits, heatDemandEntries, electricityEntries, startDate, endDate, outputCsvPath);
        }
        else if (SelectedOptimization == "Emissions")
        {
            Console.WriteLine("Optimizing Custom Scenario by EMISSIONS...");
            OptimizeEmissionsScenarioCustomAndSaveToCsv(selectedUnits, heatDemandEntries, electricityEntries, startDate, endDate, outputCsvPath);
        }
        else
        {
            Console.WriteLine("Unknown optimization type for Custom Scenario!");
            return;
        }
    }



    LoadGraphFromCsv(outputCsvPath);
    Console.WriteLine("Optimization finished and saved to CSV!");
});


        SaveCsvCommand = new RelayCommand(() =>
        {
            if (Avalonia.Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var opts = new FilePickerSaveOptions
            {
                Title = "Save optimization results as…",
                FileTypeChoices = new List<FilePickerFileType>
                {
            new("CSV files") { Patterns = new List<string> { "*.csv" } }
                },
                SuggestedFileName = Path.GetFileName(_lastOutputPath)
            };

            var result = desktop.MainWindow.StorageProvider.SaveFilePickerAsync(opts).GetAwaiter().GetResult();
            if (result != null)
            {
                using var dest = result.OpenWriteAsync().GetAwaiter().GetResult();
                using var src = File.OpenRead(_lastOutputPath);
                src.CopyTo(dest);
                Console.WriteLine($"✅ Results saved to {result.Name}");
            }

            if (result != null)
            {
                using var dest = result.OpenWriteAsync().GetAwaiter().GetResult();
                using var src = File.OpenRead(_lastOutputPath);
                src.CopyTo(dest);

                // ←— set your status here
                SaveStatus = $"✅ Saved to “{result.Name}”";
                Observable
                        .Timer(TimeSpan.FromSeconds(2))
                        // make sure we’re back on the UI thread
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => SaveStatus = "");
            }
        });
    }
    private void OptimizeCostScenario2AndSaveToCsv(
     List<ProductionUnit> units,
     List<TimeSeriesEntry> heatEntries,
     List<TimeSeriesEntry> electricityEntries,
     DateTime startDate,
     DateTime endDate,
     string outputCsvPath)
    {
        double totalCost = 0;
        double totalCO2 = 0;

        var fixedUnitOrder = new[] { "Gas Boiler 1", "Oil Boiler 1", "Gas Motor 1", "Heat Pump 1" };

        var orderedUnits = fixedUnitOrder
            .Select(name => units.FirstOrDefault(u => u.Name == name))
            .Where(u => u != null)
            .ToList();

        var results = new List<(DateTime Time, double HeatDemand, double HourlyCO2, double HourlyCost, Dictionary<string, double> UnitProductions)>();

        foreach (var demand in heatEntries.Where(h => h.Timestamp >= startDate && h.Timestamp < endDate))
        {
            var el = electricityEntries.FirstOrDefault(e => e.Timestamp == demand.Timestamp);
            if (el == null) continue;

            double heatNeeded = demand.Value;
            double hourlyCost = 0;
            double hourlyCO2 = 0;

            var unitProductions = fixedUnitOrder.ToDictionary(u => u, u => 0.0);

            var unitsWithNetCost = orderedUnits
                .Select(unit =>
                {
                    double netCost = unit.ProductionCosts;

                    if (unit.Name.Contains("Gas Motor"))
                        netCost = Math.Max(0, netCost - el.Value * unit.MaxElectricity);
                    else if (unit.Name.Contains("Heat Pump"))
                        netCost += el.Value * Math.Abs(unit.MaxElectricity);

                    return new { Unit = unit, NetCost = netCost };
                })
                .OrderBy(x => x.NetCost)
                .ToList();

            foreach (var u in unitsWithNetCost)
            {
                if (heatNeeded <= 0)
                    break;

                var unit = u.Unit;
                double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

                hourlyCost += heatFromUnit * u.NetCost;
                hourlyCO2 += heatFromUnit * unit.CO2Emissions;

                if (unitProductions.ContainsKey(unit.Name))
                    unitProductions[unit.Name] += heatFromUnit;

                heatNeeded -= heatFromUnit;
            }

            totalCost += hourlyCost;
            totalCO2 += hourlyCO2;

            results.Add((demand.Timestamp, demand.Value, hourlyCO2, hourlyCost, unitProductions));
        }

        var dir = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("Time,GB1,OB1,GM1,HP1,HeatDemand,CO2,Cost");
            foreach (var r in results)
            {
                writer.WriteLine($"{r.Time:yyyy-MM-dd HH:mm}," +
                                 $"{r.UnitProductions["Gas Boiler 1"]:F2}," +
                                 $"{r.UnitProductions["Oil Boiler 1"]:F2}," +
                                 $"{r.UnitProductions["Gas Motor 1"]:F2}," +
                                 $"{r.UnitProductions["Heat Pump 1"]:F2}," +
                                 $"{r.HeatDemand:F2},{r.HourlyCO2:F2},{r.HourlyCost:F2}");
            }
        }

        TotalCost = totalCost;
        TotalCO2 = totalCO2;

        Console.WriteLine("✅ Scenario 2 (Cost) now aligned with Custom Scenario logic.");
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

        var results = new List<(DateTime TimeFrom, double GB1, double GB2, double OB1, double HeatDemand, double HourlyCO2, double HourlyCost)>();

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

            totalCost += hourlyCost;
            totalCO2 += hourlyCO2;

            results.Add((demand.Timestamp, gb1, gb2, ob1, demand.Value, hourlyCO2, hourlyCost));
        }

        var directory = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Console.WriteLine($"✅ Created missing directory: {directory}");
        }

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("TimeFrom,GB1,GB2,OB1,HeatDemand,CO2,Cost");
            foreach (var result in results)
            {
                writer.WriteLine($"{result.TimeFrom:yyyy-MM-dd HH:mm},{result.GB1:F2},{result.GB2:F2},{result.OB1:F2},{result.HeatDemand:F2},{result.HourlyCO2:F2},{result.HourlyCost:F2}");
            }
        }

        Console.WriteLine($"✅ Optimization results (Scenario 1 - Cost) saved to {outputCsvPath}");

        TotalCost = totalCost;
        TotalCO2 = totalCO2;
    }


    private void OptimizeEmissionsScenario2AndSaveToCsv(
     List<ProductionUnit> units,
     List<TimeSeriesEntry> heatEntries,
     List<TimeSeriesEntry> electricityEntries,
     DateTime startDate,
     DateTime endDate,
     string outputCsvPath)
    {
        double totalCost = 0;
        double totalCO2 = 0;

        var fixedUnitOrder = new[] { "Gas Boiler 1", "Oil Boiler 1", "Gas Motor 1", "Heat Pump 1" };

        // Only use units for Scenario 2 (no GB2)
        var orderedUnits = fixedUnitOrder
            .Select(name => units.FirstOrDefault(u => u.Name == name))
            .Where(u => u != null)
            .ToList();

        var results = new List<(DateTime Time, double HeatDemand, double HourlyCO2, double HourlyCost, Dictionary<string, double> UnitProductions)>();

        foreach (var demand in heatEntries.Where(h => h.Timestamp >= startDate && h.Timestamp < endDate))
        {
            var el = electricityEntries.FirstOrDefault(e => e.Timestamp == demand.Timestamp);
            if (el == null) continue;

            double heatNeeded = demand.Value;
            double hourlyCost = 0;
            double hourlyCO2 = 0;

            var unitProductions = fixedUnitOrder.ToDictionary(u => u, u => 0.0);

            var unitsWithNetEmissions = orderedUnits
                .Select(unit =>
                {
                    double netEmissions = unit.CO2Emissions;

                    if (unit.Name.Contains("Heat Pump"))
                        netEmissions += el.Value * 0.3; // adjust for electricity

                    return new { Unit = unit, NetEmissions = netEmissions };
                })
                .OrderBy(x => x.NetEmissions)
                .ToList();

            foreach (var u in unitsWithNetEmissions)
            {
                if (heatNeeded <= 0)
                    break;

                var unit = u.Unit;
                double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

                hourlyCO2 += heatFromUnit * u.NetEmissions;
                hourlyCost += heatFromUnit * unit.ProductionCosts;

                if (unitProductions.ContainsKey(unit.Name))
                    unitProductions[unit.Name] += heatFromUnit;

                heatNeeded -= heatFromUnit;
            }

            totalCost += hourlyCost;
            totalCO2 += hourlyCO2;

            results.Add((demand.Timestamp, demand.Value, hourlyCO2, hourlyCost, unitProductions));
        }

        var dir = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("Time,GB1,OB1,GM1,HP1,HeatDemand,CO2,Cost");
            foreach (var r in results)
            {
                writer.WriteLine($"{r.Time:yyyy-MM-dd HH:mm},{r.UnitProductions["Gas Boiler 1"]:F2},{r.UnitProductions["Oil Boiler 1"]:F2},{r.UnitProductions["Gas Motor 1"]:F2},{r.UnitProductions["Heat Pump 1"]:F2},{r.HeatDemand:F2},{r.HourlyCO2:F2},{r.HourlyCost:F2}");
            }
        }

        TotalCost = totalCost;
        TotalCO2 = totalCO2;

        Console.WriteLine("✅ Scenario 2 (Emissions) now aligned with Custom Scenario logic.");
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
    private void OptimizeCostScenarioCustomAndSaveToCsv(
    List<ProductionUnit> selectedUnits,
    List<TimeSeriesEntry> heatEntries,
    List<TimeSeriesEntry> electricityEntries,
    DateTime startDate,
    DateTime endDate,
    string outputCsvPath)
    {
        double totalCost = 0;
        double totalCO2 = 0;

        var isScenario1 = selectedUnits.All(u =>
            u.Name == "Gas Boiler 1" ||
            u.Name == "Gas Boiler 2" ||
            u.Name == "Oil Boiler 1");

        var fixedUnitOrder = new[] { "Gas Boiler 1", "Gas Boiler 2", "Oil Boiler 1", "Gas Motor 1", "Heat Pump 1" };
        var orderedUnits = fixedUnitOrder
            .Select(name => selectedUnits.FirstOrDefault(u => u.Name == name))
            .Where(u => u != null)
            .ToList();

        var results = new List<(DateTime Time, double HeatDemand, double HourlyCO2, double HourlyCost, Dictionary<string, double> UnitProductions)>();

        foreach (var demand in heatEntries.Where(h => h.Timestamp >= startDate && h.Timestamp < endDate))
        {
            var el = electricityEntries.FirstOrDefault(e => e.Timestamp == demand.Timestamp);
            if (el == null) continue;

            double heatNeeded = demand.Value;
            double hourlyCost = 0;
            double hourlyCO2 = 0;

            var unitProductions = fixedUnitOrder.ToDictionary(u => u, u => 0.0);

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            var unitsWithCost = isScenario1
                ? orderedUnits.OrderBy(u => u.ProductionCosts).Select(u => new { Unit = u, NetCost = u.ProductionCosts }).ToList()
                : orderedUnits.Select(unit =>
                    {
                        double netCost = unit.ProductionCosts;
                        if (unit.Name.Contains("Gas Motor"))
                            netCost = Math.Max(0, netCost - el.Value * unit.MaxElectricity);
                        else if (unit.Name.Contains("Heat Pump"))
                            netCost += el.Value * Math.Abs(unit.MaxElectricity);
                        return new { Unit = unit, NetCost = netCost };
                    })
                    .OrderBy(x => x.NetCost)
                    .ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

            foreach (var u in unitsWithCost)
            {
                if (heatNeeded <= 0) break;

                var unit = u.Unit;
                double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

                hourlyCost += heatFromUnit * u.NetCost;
                hourlyCO2 += heatFromUnit * unit.CO2Emissions;

                if (unitProductions.ContainsKey(unit.Name))
                    unitProductions[unit.Name] += heatFromUnit;

                heatNeeded -= heatFromUnit;
            }

            totalCost += hourlyCost;
            totalCO2 += hourlyCO2;

            results.Add((demand.Timestamp, demand.Value, hourlyCO2, hourlyCost, unitProductions));
        }

        var dir = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("Time,GB1,GB2,OB1,GM1,HP1,HeatDemand,CO2,Cost");
            foreach (var r in results)
            {
                writer.WriteLine($"{r.Time:yyyy-MM-dd HH:mm}," +
                                 $"{r.UnitProductions["Gas Boiler 1"]:F2}," +
                                 $"{r.UnitProductions["Gas Boiler 2"]:F2}," +
                                 $"{r.UnitProductions["Oil Boiler 1"]:F2}," +
                                 $"{r.UnitProductions["Gas Motor 1"]:F2}," +
                                 $"{r.UnitProductions["Heat Pump 1"]:F2}," +
                                 $"{r.HeatDemand:F2},{r.HourlyCO2:F2},{r.HourlyCost:F2}");
            }
        }

        TotalCost = totalCost;
        TotalCO2 = totalCO2;
        Console.WriteLine("✅ Custom Scenario (Cost) matches Scenario 1 or 2 depending on selected units.");
    }


    private void OptimizeEmissionsScenarioCustomAndSaveToCsv(
    List<ProductionUnit> selectedUnits,
    List<TimeSeriesEntry> heatEntries,
    List<TimeSeriesEntry> electricityEntries,
    DateTime startDate,
    DateTime endDate,
    string outputCsvPath)
    {
        double totalCost = 0;
        double totalCO2 = 0;

        var fixedUnitOrder = new[] { "Gas Boiler 1", "Gas Boiler 2", "Oil Boiler 1", "Gas Motor 1", "Heat Pump 1" };
        var orderedUnits = fixedUnitOrder
            .Select(name => selectedUnits.FirstOrDefault(u => u.Name == name))
            .Where(u => u != null)
            .ToList();

        var results = new List<(DateTime Time, double HeatDemand, double HourlyCO2, double HourlyCost, Dictionary<string, double> UnitProductions)>();

        foreach (var demand in heatEntries.Where(h => h.Timestamp >= startDate && h.Timestamp < endDate))
        {
            var el = electricityEntries.FirstOrDefault(e => e.Timestamp == demand.Timestamp);
            if (el == null) continue;

            double heatNeeded = demand.Value;
            double hourlyCost = 0;
            double hourlyCO2 = 0;

            var unitProductions = fixedUnitOrder.ToDictionary(u => u, u => 0.0);

            var unitsWithNetEmissions = orderedUnits
                .Select(unit =>
                {
                    double netEmissions = unit.CO2Emissions;

                    if (unit.Name.Contains("Heat Pump"))
                        netEmissions += el.Value * 0.3;

                    return new { Unit = unit, NetEmissions = netEmissions };
                })
                .OrderBy(x => x.NetEmissions)
                .ToList();

            foreach (var u in unitsWithNetEmissions)
            {
                if (heatNeeded <= 0)
                    break;

                var unit = u.Unit;
                double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

                hourlyCO2 += heatFromUnit * u.NetEmissions;
                hourlyCost += heatFromUnit * unit.ProductionCosts;

                if (unitProductions.ContainsKey(unit.Name))
                    unitProductions[unit.Name] += heatFromUnit;

                heatNeeded -= heatFromUnit;
            }

            totalCost += hourlyCost;
            totalCO2 += hourlyCO2;

            results.Add((demand.Timestamp, demand.Value, hourlyCO2, hourlyCost, unitProductions));
        }

        var dir = Path.GetDirectoryName(outputCsvPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("Time,GB1,GB2,OB1,GM1,HP1,HeatDemand,CO2,Cost");
            foreach (var r in results)
            {
                writer.WriteLine($"{r.Time:yyyy-MM-dd HH:mm},{r.UnitProductions["Gas Boiler 1"]:F2},{r.UnitProductions["Gas Boiler 2"]:F2},{r.UnitProductions["Oil Boiler 1"]:F2},{r.UnitProductions["Gas Motor 1"]:F2},{r.UnitProductions["Heat Pump 1"]:F2},{r.HeatDemand:F2},{r.HourlyCO2:F2},{r.HourlyCost:F2}");
            }
        }

        TotalCost = totalCost;
        TotalCO2 = totalCO2;
        Console.WriteLine("✅ Custom Scenario (Emissions) now aligns with Scenario 2 logic.");
    }





    private void LoadGraphFromCsv(string filePath)
    {
        Console.WriteLine("Loading graph from CSV...");

        var timestamps = new List<string>();
        var heatDemand = new List<double>();
        var gb1 = new List<double>();
        var gb2 = new List<double>();
        var ob1 = new List<double>();
        var gm1 = new List<double>();
        var hp1 = new List<double>();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("CSV file missing for graph!");
            return;
        }

        var lines = File.ReadAllLines(filePath);
        var header = lines[0].Split(',');

        // 🔥 Găsim indexul corect al fiecărui boiler
        int gb1Index = Array.FindIndex(header, h => h.Contains("GB1", StringComparison.OrdinalIgnoreCase));
        int gb2Index = Array.FindIndex(header, h => h.Contains("GB2", StringComparison.OrdinalIgnoreCase));
        int ob1Index = Array.FindIndex(header, h => h.Contains("OB1", StringComparison.OrdinalIgnoreCase));
        int gm1Index = Array.FindIndex(header, h => h.Contains("GM1", StringComparison.OrdinalIgnoreCase));
        int hp1Index = Array.FindIndex(header, h => h.Contains("HP1", StringComparison.OrdinalIgnoreCase));
        int heatDemandIndex = Array.FindIndex(header, h => h.Contains("Heat", StringComparison.OrdinalIgnoreCase));

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');

            if (parts.Length < 2) continue;

            timestamps.Add(DateTime.Parse(parts[0]).ToString("HH:mm"));

            if (gb1Index != -1 && gb1Index < parts.Length)
                gb1.Add(double.Parse(parts[gb1Index], CultureInfo.InvariantCulture));
            else
                gb1.Add(0);

            if (gb2Index != -1 && gb2Index < parts.Length)
                gb2.Add(double.Parse(parts[gb2Index], CultureInfo.InvariantCulture));
            else
                gb2.Add(0);

            if (ob1Index != -1 && ob1Index < parts.Length)
                ob1.Add(double.Parse(parts[ob1Index], CultureInfo.InvariantCulture));
            else
                ob1.Add(0);

            if (gm1Index != -1 && gm1Index < parts.Length)
                gm1.Add(double.Parse(parts[gm1Index], CultureInfo.InvariantCulture));
            else
                gm1.Add(0);

            if (hp1Index != -1 && hp1Index < parts.Length)
                hp1.Add(double.Parse(parts[hp1Index], CultureInfo.InvariantCulture));
            else
                hp1.Add(0);

            if (heatDemandIndex != -1 && heatDemandIndex < parts.Length)
                heatDemand.Add(double.Parse(parts[heatDemandIndex], CultureInfo.InvariantCulture));
        }

        // Series exact ca la tine:
        Series = new ISeries[]
        {
        new StackedAreaSeries<double> { Name = "GB1", Values = gb1 },
        new StackedAreaSeries<double> { Name = "GB2", Values = gb2 },
        new StackedAreaSeries<double> { Name = "OB1", Values = ob1 },
        new StackedAreaSeries<double> { Name = "GM1", Values = gm1 },
        new StackedAreaSeries<double> { Name = "HP1", Values = hp1 },
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
            TextSize = 10,
            LabelsPaint = new SolidColorPaint(SKColors.Black),
            TicksPaint = new SolidColorPaint(SKColors.Black),
            NamePaint = new SolidColorPaint(SKColors.Black)
        }
    };

        YAxes = new[]
        {
        new Axis
        {
            Name = "MW",
            TextSize = 10,
            LabelsPaint = new SolidColorPaint(SKColors.Black),
            TicksPaint = new SolidColorPaint(SKColors.Black),
            NamePaint = new SolidColorPaint(SKColors.Black)
        }
    };

        this.RaisePropertyChanged(nameof(Series));
        this.RaisePropertyChanged(nameof(XAxes));
        this.RaisePropertyChanged(nameof(YAxes));
    }



}