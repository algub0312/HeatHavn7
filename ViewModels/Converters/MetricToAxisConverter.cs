using System;
using Avalonia.Data.Converters;
using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using HeatHavnAppProject.ViewModels;

namespace HeatHavnAppProject.Converters
{
    public class MetricToAxisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Optimizer.MetricType metric)
            {
                return new Axis[]
                {
                    new Axis
                    {
                        Name = metric == Optimizer.MetricType.Cost ? "Cost (DKK)" : "CO₂ Emissions (kg)"
                    }
                };
            }

            return new Axis[] { new Axis { Name = "Unknown" } };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotSupportedException();
    }
}
