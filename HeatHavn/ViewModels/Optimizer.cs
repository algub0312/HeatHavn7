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
    public ICommand SaveCsvCommand   { get; }
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
private bool _scenario2Enabled;
public bool Scenario2Enabled
{
    get => _scenario2Enabled;
    set => this.RaiseAndSetIfChanged(ref _scenario2Enabled, value);
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
    {_lastOutputPath = "Data/optimization_results.csv"; // default output path

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
    {if (SelectedOptimization == "Cost")
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
   else if(Scenario2Enabled)
   {
    if (SelectedOptimization == "Cost")
    OptimizeCostScenario2AndSaveToCsv(productionUnits, heatDemandEntries,electricityEntries,startDate, endDate, outputCsvPath);
       
    else if (SelectedOptimization == "Emissions")
OptimizeEmissionsScenario2AndSaveToCsv(productionUnits, heatDemandEntries, electricityEntries, startDate, endDate, outputCsvPath);

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
        using var src  = File.OpenRead(_lastOutputPath);
        src.CopyTo(dest);
        Console.WriteLine($"✅ Results saved to {result.Name}");
    }

      if (result != null)
    {
        using var dest = result.OpenWriteAsync().GetAwaiter().GetResult();
        using var src  = File.OpenRead(_lastOutputPath);
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
        var usedUnits = units
    .Where(u => u.Name != "Gas Boiler 2")
    .ToList();
double avgElPrice = electricityEntries
    .Where(e => e.Timestamp >= startDate && e.Timestamp < endDate)
    .Average(e => e.Value);
double totalCost = 0;
double totalCO2 = 0;

var results = new List<(DateTime Time, double GB1, double OB1, double GM1, double HP1, double HeatDemand, double HourlyCO2, double HourlyCost)>();
foreach (var demand in heatEntries.Where(h => h.Timestamp >= startDate && h.Timestamp < endDate))
{
    var el = electricityEntries.FirstOrDefault(e => e.Timestamp == demand.Timestamp);
    if (el == null) continue;

    double heatNeeded = demand.Value;
    double hourlyCost = 0;
    double hourlyCO2 = 0;

    double gb1 = 0, ob1 = 0, gm1 = 0, hp1 = 0;

    // Construim o listă cu costul net, pe baza electricității
    var unitsWithNetCost = usedUnits.Select(unit =>
    {
        double netCost = unit.ProductionCosts;

     if (unit.Name.Contains("Gas Motor"))
{
    netCost -= el.Value * unit.MaxElectricity;

    if (netCost < 0)
        netCost = 0; // sau 50, în funcție de modelul economic
}

        else if (unit.Name.Contains("Heat Pump"))
            netCost += el.Value * Math.Abs(unit.MaxElectricity); // consumă curent -> crește costul

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

        if (unit.Name == "Gas Boiler 1") gb1 += heatFromUnit;
        if (unit.Name == "Oil Boiler 1") ob1 += heatFromUnit;
        if (unit.Name == "Gas Motor 1") gm1 += heatFromUnit;
        if (unit.Name == "Heat Pump 1") hp1 += heatFromUnit;

        hourlyCost += heatFromUnit * u.NetCost;
        hourlyCO2 += heatFromUnit * unit.CO2Emissions;

        heatNeeded -= heatFromUnit;
    }

    totalCost += hourlyCost;
    totalCO2 += hourlyCO2;

    results.Add((demand.Timestamp, gb1, ob1, gm1, hp1, demand.Value, hourlyCO2, hourlyCost));
}
var dir = Path.GetDirectoryName(outputCsvPath);
if (!Directory.Exists(dir))
    Directory.CreateDirectory(dir);

using (var writer = new StreamWriter(outputCsvPath))
{
    writer.WriteLine("Time,GB1,OB1,GM1,HP1,HeatDemand,CO2,Cost");
    foreach (var r in results)
    {
        writer.WriteLine($"{r.Time:yyyy-MM-dd HH:mm},{r.GB1:F2},{r.OB1:F2},{r.GM1:F2},{r.HP1:F2},{r.HeatDemand:F2},{r.HourlyCO2:F2},{r.HourlyCost:F2}");
    }
}
TotalCost = totalCost;
TotalCO2 = totalCO2;

Console.WriteLine("✅ Scenario 2 (cost) optimization complete.");

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

 foreach (var demand in demandEntries.Where(d => d.Timestamp >= startDate && d.Timestamp <= endDate))        
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

private void OptimizeEmissionsScenario2AndSaveToCsv(
    List<ProductionUnit> units,
    List<TimeSeriesEntry> heatEntries,
    List<TimeSeriesEntry> electricityEntries,
    DateTime startDate,
    DateTime endDate,
    string outputCsvPath)
{
    var usedUnits = units
    .Where(u => u.Name != "Gas Boiler 2")
    .ToList();

double avgElPrice = electricityEntries
    .Where(e => e.Timestamp >= startDate && e.Timestamp < endDate)
    .Average(e => e.Value);

double totalCost = 0;
double totalCO2 = 0;

var results = new List<(DateTime Time, double GB1, double OB1, double GM1, double HP1, double HeatDemand, double HourlyCO2, double HourlyCost)>();
foreach (var demand in heatEntries.Where(h => h.Timestamp >= startDate && h.Timestamp < endDate))
{
    var el = electricityEntries.FirstOrDefault(e => e.Timestamp == demand.Timestamp);
    if (el == null) continue;

    double heatNeeded = demand.Value;
    double hourlyCost = 0;
    double hourlyCO2 = 0;

    double gb1 = 0, ob1 = 0, gm1 = 0, hp1 = 0;

    // Ajustăm emisiile luând în considerare impactul electricității
    var unitsWithNetCO2 = usedUnits.Select(unit =>
    {
        double netCO2 = unit.CO2Emissions;

        // Dacă motorul produce curent (pozitiv), scade impactul de emisii
        if (unit.Name.Contains("Gas Motor"))
            netCO2 -= 0; // Presupunem 0 impact suplimentar pentru curent produs

        // Dacă pompa consumă curent (negativ MaxElectricity), adăugăm emisii suplimentare
        if (unit.Name.Contains("Heat Pump"))
            netCO2 += el.Value * 0.3; // presupunem 0.3 kg CO2 / DKK (exemplu)

        return new { Unit = unit, NetCO2 = netCO2 };
    })
    .OrderBy(x => x.NetCO2)
    .ToList();

    foreach (var u in unitsWithNetCO2)
    {
        if (heatNeeded <= 0)
            break;

        var unit = u.Unit;
        double heatFromUnit = Math.Min(unit.MaxHeat, heatNeeded);

        if (unit.Name == "Gas Boiler 1") gb1 += heatFromUnit;
        if (unit.Name == "Oil Boiler 1") ob1 += heatFromUnit;
        if (unit.Name == "Gas Motor 1") gm1 += heatFromUnit;
        if (unit.Name == "Heat Pump 1") hp1 += heatFromUnit;

        hourlyCost += heatFromUnit * unit.ProductionCosts;
        hourlyCO2 += heatFromUnit * unit.CO2Emissions;

        heatNeeded -= heatFromUnit;
    }

    totalCost += hourlyCost;
    totalCO2 += hourlyCO2;

    results.Add((demand.Timestamp, gb1, ob1, gm1, hp1, demand.Value, hourlyCO2, hourlyCost));
}
var dir = Path.GetDirectoryName(outputCsvPath);
if (!Directory.Exists(dir))
    Directory.CreateDirectory(dir);

using (var writer = new StreamWriter(outputCsvPath))
{
    writer.WriteLine("Time,GB1,OB1,GM1,HP1,HeatDemand,CO2,Cost");
    foreach (var r in results)
    {
        writer.WriteLine($"{r.Time:yyyy-MM-dd HH:mm},{r.GB1:F2},{r.OB1:F2},{r.GM1:F2},{r.HP1:F2},{r.HeatDemand:F2},{r.HourlyCO2:F2},{r.HourlyCost:F2}");
    }
}
TotalCost = totalCost;
TotalCO2 = totalCO2;

Console.WriteLine("✅ Scenario 2 (emissions) optimization complete.");

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
        var gm1 = new List<double>();
        var hp1 = new List<double>();
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
        foreach (var line in lines.Skip(1))
{
    var parts = line.Split(',');

    // SCENARIUL 2: CSV cu 8 coloane
    if (parts.Length >= 8)
    {
        timestamps.Add(DateTime.Parse(parts[0]).ToString("HH:mm"));
        gb1.Add(double.Parse(parts[1], CultureInfo.InvariantCulture));
        ob1.Add(double.Parse(parts[2], CultureInfo.InvariantCulture));
        gm1.Add(double.Parse(parts[3], CultureInfo.InvariantCulture));
        hp1.Add(double.Parse(parts[4], CultureInfo.InvariantCulture));
        heatDemand.Add(double.Parse(parts[5], CultureInfo.InvariantCulture));
    }
    // SCENARIUL 1: CSV cu 6 coloane
    else if (parts.Length >= 6)
    {
        timestamps.Add(DateTime.Parse(parts[0]).ToString("HH:mm"));
        gb1.Add(double.Parse(parts[1], CultureInfo.InvariantCulture));
        gb2.Add(double.Parse(parts[2], CultureInfo.InvariantCulture));
        ob1.Add(double.Parse(parts[3], CultureInfo.InvariantCulture));
        heatDemand.Add(double.Parse(parts[4], CultureInfo.InvariantCulture));
        // gm1 și hp1 rămân goale implicit
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
        new StackedAreaSeries<double>
    {
        Name = "GM1",
        Values = gm1,
    },
    new StackedAreaSeries<double>
    {
        Name = "HP1",
        Values = hp1,
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