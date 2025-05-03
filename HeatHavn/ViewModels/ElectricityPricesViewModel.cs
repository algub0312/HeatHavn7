using System;
using System.Collections.ObjectModel;
using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Models;
namespace HeatHavnAppProject.ViewModels;

using System.Diagnostics;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;

public class ElectricityPricesViewModel : SeasonalViewModelBase
{
    public override string Title => "⚡ Electricity Prices";


    public ObservableCollection<TimeSeriesEntry> FilteredElectricityPrices { get; } = new();
        public ObservableCollection<ObservablePoint> ElPoints { get; } = new();


    private readonly SourceDataManagerViewModel _source;

    public ElectricityPricesViewModel(SourceDataManagerViewModel sourceData)
    {
        _source = sourceData;

        Debug.WriteLine("🔥 electricity is rising high");
        Debug.WriteLine("Summer data count: " + _source.SummerDataEl.Count);
        Debug.WriteLine("Winter data count: " + _source.WinterDataEl.Count);

        
        UpdateFilteredElectricityPrices();
    }
 protected override void OnSeasonChanged()
    {
        UpdateFilteredElectricityPrices();
    }

    private void UpdateFilteredElectricityPrices()
    {
        FilteredElectricityPrices.Clear();
        ElPoints.Clear(); // ← clears chart values

            var data = SelectedSeason == "Summer" ? _source.SummerDataEl : _source.WinterDataEl;

        foreach (var entry in data)
        {
            FilteredElectricityPrices.Add(entry);
            ElPoints.Add(new ObservablePoint(entry.Timestamp.ToOADate(), entry.Value));

        }
    }

    public ISeries[] ElSeries =>
        new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = ElPoints,
                GeometrySize = 0, 
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
              LabelsPaint = new SolidColorPaint(SKColors.Black),
        TicksPaint = new SolidColorPaint(SKColors.Black),
        NamePaint = new SolidColorPaint(SKColors.Black)
        }
    };

    public Axis[] YAxes => new Axis[]
    {
        new Axis
        {
            Name = "Electricity Price (DKK/MWh)",
              LabelsPaint = new SolidColorPaint(SKColors.Black),
        TicksPaint = new SolidColorPaint(SKColors.Black),
        NamePaint = new SolidColorPaint(SKColors.Black)
        }
    };

}
