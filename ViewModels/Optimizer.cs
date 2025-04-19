using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Metadata;
using System.Security.Cryptography;
using System.Security.AccessControl;
using HeatHavnAppProject.Models;
using System.Runtime.Versioning;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using System.IO;
using Avalonia.Threading;

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
    
    private void LoadDataDynamic(out List<TimeSeriesEntry> SummerEl, out List<TimeSeriesEntry> SummerHeat,  out List<TimeSeriesEntry> WinterEl, out List<TimeSeriesEntry> WinterHeat)
    {
        
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string summerCsvPath = Path.Combine(basePath, "Data", "summerperiod.csv");

        string basePath2 = AppDomain.CurrentDomain.BaseDirectory;
        string winterCsvPath = Path.Combine(basePath2, "Data", "summerperiod.csv");

        var dataManager = new SourceDataManagerViewModel();
        
        dataManager.LoadSummerDataHeat(summerCsvPath);
        dataManager.LoadWinterDataHeat(winterCsvPath);
        dataManager.LoadSummerDataEl(summerCsvPath);
        dataManager.LoadWinterDataEl(winterCsvPath);
        

        SummerEl = dataManager.SummerDataEl.ToList();
        SummerHeat = dataManager.SummerDataHeat.ToList();
        WinterHeat = dataManager.WinterDataHeat.ToList();
        WinterEl = dataManager.WinterDataEl.ToList();
    }

    private void LoadAssets(out List<ProductionUnit> ListAssets)
    {
        var assetsClass= new AssetManager();
        ListAssets=assetsClass.GetAllUnits();
    }
    
    private void  OptimiseCosts(out List<TimeSeriesEntry> Cost_Hour,out List<TimeSeriesEntry> Emmisions_Hour, int Season)
    {
        var SummerEl = new List<TimeSeriesEntry>();
        var SummerHeat = new List<TimeSeriesEntry>();
        var WinterEl = new List<TimeSeriesEntry>();
        var WinterHeat = new List<TimeSeriesEntry>();

        var ListAssets= new List<ProductionUnit>();

        LoadAssets(out ListAssets);
        LoadDataDynamic(out SummerEl, out SummerHeat, out WinterEl, out WinterHeat);
        

        double TotalCost = 0;
        double TotalEmissions = 0;

        double Cost_Time=0;
        double Emissions_Time=0;

        List<TimeSeriesEntry> Elect= new List<TimeSeriesEntry>();
        List<TimeSeriesEntry> Heat = new List<TimeSeriesEntry>();
        if (Season==1)
            {
                Elect=new List<TimeSeriesEntry>(SummerEl);
                Heat= new List<TimeSeriesEntry> (SummerHeat);
            }
        if (Season==2)
            {
                Elect=new List<TimeSeriesEntry>(WinterEl);
                Heat= new List<TimeSeriesEntry> (WinterHeat);    
            }

        List<ProductionUnit> NextEff = new List<ProductionUnit>();
        ProductionUnit Temp= new ProductionUnit();

        double MinPrice=500000;

        Cost_Hour = new List<TimeSeriesEntry>();
        Emmisions_Hour = new List<TimeSeriesEntry>();

        TimeSeriesEntry Bonus=new TimeSeriesEntry();

        foreach (var (ElEntry,HeatEntry) in Elect.Zip(Heat))
            {
                Cost_Time=0;
                Emissions_Time=0;
                
                if (ElEntry.Timestamp>=StartDate && ElEntry.Timestamp<=EndDate) 
                    {   
                        MinPrice=500000;
                        LoadAssets(out ListAssets);
                        for(int i=0;i<5;i++)
                        {   
                            MinPrice=500000;
                            foreach(var entry in ListAssets)
                                {    
                                    if (entry.MaxElectricity==0)
                                        if (entry.ProductionCosts<MinPrice)
                                            {
                                                MinPrice=entry.ProductionCosts;
                                                Temp=entry;
                                            }

                                    else if (entry.MaxElectricity!=0)
                                        if (entry.ProductionCosts - ElEntry.Value * (entry.MaxElectricity/entry.MaxHeat) < MinPrice)
                                            {
                                                MinPrice=entry.ProductionCosts;
                                                Temp=entry;
                                            }
                                }

                                NextEff.Add(Temp);
                                ListAssets.Remove(Temp);
                            }

                            foreach(var entry in NextEff)
                            {   
                                if (HeatEntry.Value >= entry.MaxHeat)
                                    {
                                        HeatEntry. Value= HeatEntry.Value- entry.MaxHeat;
                                        Console.WriteLine("Name of applicance:" + entry.Name);
                                        
                                        TotalCost = TotalCost + entry.ProductionCosts * entry.MaxHeat;
                                        Cost_Time = Cost_Time + entry.ProductionCosts * entry.MaxHeat;
                                        
                                        TotalEmissions = TotalEmissions + entry.CO2Emissions * entry.MaxHeat;
                                        Emissions_Time = Emissions_Time + entry.CO2Emissions * entry.MaxHeat;
                                    }

                                else if (HeatEntry.Value < entry.MaxHeat)
                                    {
                                        Console.WriteLine("Name of appliance:"+entry.Name+"and percentage is:"+100/entry.MaxHeat*HeatEntry.Value);
                                        HeatEntry.Value=0;

                                        TotalCost = TotalCost + entry.ProductionCosts * HeatEntry.Value;
                                        Cost_Time = Cost_Time + entry.ProductionCosts * HeatEntry.Value;

                                        TotalEmissions = TotalEmissions + entry.CO2Emissions * HeatEntry.Value;
                                        Emissions_Time = Emissions_Time + entry.CO2Emissions * HeatEntry.Value;
                                    }
                                if (HeatEntry.Value==0)
                                    {    
                                        Cost_Hour.Add(new TimeSeriesEntry
                                        {
                                        Value = Cost_Time,
                                        Timestamp = ElEntry.Timestamp
                                        });

                                        Emmisions_Hour.Add(new TimeSeriesEntry
                                        {
                                        Value = Emissions_Time,
                                        Timestamp = ElEntry.Timestamp
                                        });
                    
                                        break;
                                    }                                
                            }
                    }
            }
    }
    
    private void OptimiseCO2(out List<TimeSeriesEntry> Cost_Hour,out List<TimeSeriesEntry> Emmisions_Hour, int Season)
    {

        var SummerEl = new List<TimeSeriesEntry>();
        var SummerHeat = new List<TimeSeriesEntry>();
        var WinterEl = new List<TimeSeriesEntry>();
        var WinterHeat = new List<TimeSeriesEntry>();

        var ListAssets= new List<ProductionUnit>();

        LoadAssets(out ListAssets);
        LoadDataDynamic(out SummerEl, out SummerHeat, out WinterEl, out WinterHeat);
        
    double TotalCost = 0;
        double TotalEmissions = 0;

        List<TimeSeriesEntry> Elect= new List<TimeSeriesEntry>();
        List<TimeSeriesEntry> Heat = new List<TimeSeriesEntry>();

        if (Season==1)
            {
                Elect=new List<TimeSeriesEntry>(SummerEl);
                Heat= new List<TimeSeriesEntry> (SummerHeat);
            }
        if (Season==2)
            {
                Elect=new List<TimeSeriesEntry>(WinterEl);
                Heat= new List<TimeSeriesEntry> (WinterHeat);    
            }

        
        double Cost_Time=0;
        double Emissions_Time=0;

        Cost_Hour = new List<TimeSeriesEntry>();
        Emmisions_Hour = new List<TimeSeriesEntry>();

        TimeSeriesEntry Bonus=new TimeSeriesEntry();
        ListAssets.Sort((x, y) => x.CO2Emissions.CompareTo(y.CO2Emissions));

        foreach (var (ElEntry,HeatEntry) in Elect.Zip(Heat))
        {
            foreach(var entry in ListAssets)
            {   
                if (HeatEntry.Value >= entry.MaxHeat)
                    {
                        HeatEntry. Value= HeatEntry.Value- entry.MaxHeat;
                        Console.WriteLine("Name of applicance:" + entry.Name);
                                        
                        TotalCost = TotalCost + entry.ProductionCosts * entry.MaxHeat;
                        Cost_Time = Cost_Time + entry.ProductionCosts * entry.MaxHeat;
                                        
                        TotalEmissions = TotalEmissions + entry.CO2Emissions * entry.MaxHeat;
                        Emissions_Time = Emissions_Time + entry.CO2Emissions * entry.MaxHeat;
                    }

                else if (HeatEntry.Value < entry.MaxHeat)
                    {
                        Console.WriteLine("Name of appliance:"+entry.Name+"and percentage is:" + 100/entry.MaxHeat*HeatEntry.Value);
                        HeatEntry.Value=0;
                                        
                        TotalCost = TotalCost + entry.ProductionCosts * HeatEntry.Value;
                        Cost_Time = Cost_Time + entry.ProductionCosts * HeatEntry.Value;

                        TotalEmissions = TotalEmissions + entry.CO2Emissions * HeatEntry.Value;
                        Emissions_Time = Emissions_Time + entry.CO2Emissions * HeatEntry.Value;
                    }
            
                if (HeatEntry.Value==0)
                    {    
                        Cost_Hour.Add(new TimeSeriesEntry
                        {
                            Value = Cost_Time,
                            Timestamp = ElEntry.Timestamp
                        });

                        Emmisions_Hour.Add(new TimeSeriesEntry
                        {
                            Value = Emissions_Time,
                            Timestamp = ElEntry.Timestamp
                        });
                    
                            break;
                        }
            }
        }
    }
    
        public ISeries[] CostSeries => new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values= Optimised_Cost,
                GeometrySize = 0, //Asta ascunde cercurile (punctele)
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.OrangeRed, 2)
            }
        };
        public ISeries[] CO2Series => new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = Optimised_CO2,
                GeometrySize = 0,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LightGreen, 2)
            }
};


    public Axis[] XAxes => new Axis[]
    {
        new Axis
        {
            Labeler = value => DateTime.FromOADate(value).ToString("HH:mm"),
            LabelsRotation = 45,
            UnitWidth = TimeSpan.FromHours(1).TotalDays,
            MinStep = TimeSpan.FromHours(1).TotalDays
        }
    };

    public Axis[] YAxes => new Axis[]
    {
        new Axis
        {
            Name = "Cost/Hour (DKK)",   
        }
    };

    public Axis[] YAxes2 => new Axis[]
    {
        new Axis
        {
            Name = "CO2 Emissions)",
        }
    };
        
    

    public ObservableCollection<ObservablePoint> Optimised_Cost { get; } = new();
    public ObservableCollection<ObservablePoint> Optimised_CO2 { get; } = new();

    public ReactiveCommand<Unit, Unit> OptimizeCommand { get; }

public Optimizer()
{
    OptimizeCommand = ReactiveCommand.Create(() =>
    {
            // Placeholder logic
            int Season = 1;

            List<TimeSeriesEntry> Cost_Hour = new List<TimeSeriesEntry>();
            List<TimeSeriesEntry> Emmisions_Hour = new List<TimeSeriesEntry>();

            if (Season == 1)
                OptimiseCosts(out Cost_Hour, out Emmisions_Hour, Season);

            if (Season == 0)
                OptimiseCO2(out Cost_Hour, out Emmisions_Hour, Season);

           
             Dispatcher.UIThread.Post(() =>
             {

            Optimised_Cost.Clear();
            Optimised_CO2.Clear();

            foreach (var entry in Cost_Hour)
            {
                Optimised_Cost.Add(new ObservablePoint(entry.Timestamp.ToOADate(), entry.Value));
            }
            foreach (var entry in Emmisions_Hour)
            {
                Optimised_CO2.Add(new ObservablePoint(entry.Timestamp.ToOADate(), entry.Value));
            }
             
             });

        });
        
    }
}


