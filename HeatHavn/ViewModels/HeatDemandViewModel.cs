using System;
using System.Collections.ObjectModel;
using HeatHavnAppProject.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Defaults;
using SkiaSharp;
using System.Diagnostics;
using LiveChartsCore;
using ReactiveUI;
using System.Linq;
namespace HeatHavnAppProject.ViewModels;


public class HeatDemandViewModel : SeasonalViewModelBase
{
    public override string Title => "🔥 Heat Demand";

    public ObservableCollection<TimeSeriesEntry> FilteredHeatDemand { get; } = new();
    public ObservableCollection<ObservablePoint> HeatDemandPoints { get; } = new();


    private readonly SourceDataManagerViewModel _source;

    public HeatDemandViewModel(SourceDataManagerViewModel sourceData)
    {
        _source = sourceData;

        Debug.WriteLine("🔥 HeatDemandViewModel constructor started!");
        Debug.WriteLine("Summer data count: " + _source.SummerDataHeat.Count);
        Debug.WriteLine("Winter data count: " + _source.WinterDataHeat.Count);

        
        UpdateFilteredHeatDemand();
    }
    protected override void OnSeasonChanged()
    {
        UpdateFilteredHeatDemand();
    }


    private void UpdateFilteredHeatDemand()
    {
         FilteredHeatDemand.Clear();
        HeatDemandPoints.Clear(); // ← clears chart values

        var data = SelectedSeason == "Summer" ? _source.SummerDataHeat : _source.WinterDataHeat;
           foreach (var entry in data)
        {
            FilteredHeatDemand.Add(entry);
            HeatDemandPoints.Add(new ObservablePoint(entry.Timestamp.ToOADate(), entry.Value));
        }
        Debug.WriteLine($"🟢 Updated FilteredHeatDemand with {FilteredHeatDemand.Count} entries for {SelectedSeason}");
    }


 public ISeries[] HeatSeries =>
        new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = HeatDemandPoints,
                     GeometrySize = 0, //Asta ascunde cercurile (punctele)
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.OrangeRed, 2)
            }
        };

    public Axis[] XAxes => new Axis[]
    {
        new Axis
        {
            Labeler = value => DateTime.FromOADate(value).ToString("HH:mm"),
            LabelsRotation = 45,
            UnitWidth = TimeSpan.FromHours(1).TotalDays,
            MinStep = TimeSpan.FromHours(1).TotalDays,
              LabelsPaint = new SolidColorPaint(SKColors.White),
        TicksPaint = new SolidColorPaint(SKColors.Gray),
        NamePaint = new SolidColorPaint(SKColors.White)
        }
    };

    public Axis[] YAxes => new Axis[]
    {
        new Axis
        {
            Name = "Heat Demand (MWh)",
              LabelsPaint = new SolidColorPaint(SKColors.White),
        TicksPaint = new SolidColorPaint(SKColors.Gray),
        NamePaint = new SolidColorPaint(SKColors.White)
        }
    };

}
