using System;
using Xunit;
using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Models;
using System.Collections.ObjectModel;
using System.Linq;


public class ElectricityPricesViewModelTests
{
    private SourceDataManagerViewModel CreateMockSource()
    {
        var mock = new SourceDataManagerViewModel();

        mock.SummerDataEl.Clear();
        mock.SummerDataEl.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 8, 1, 10, 0, 0), Value = 100 });
        mock.SummerDataEl.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 8, 1, 11, 0, 0), Value = 120 });

        mock.WinterDataEl.Clear();
        mock.WinterDataEl.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 3, 1, 10, 0, 0), Value = 80 });
        mock.WinterDataEl.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 3, 1, 11, 0, 0), Value = 90 });

        return mock;
    }

    [Fact]
    public void Should_Load_Summer_Data_When_Season_Is_Summer()
    {Console.WriteLine("electricity prices -> Should_Load_Summer_Data_When_Season_Is_Summer");

        var mockSource = CreateMockSource();
        var viewModel = new ElectricityPricesViewModel(mockSource);

        viewModel.SelectedSeason = "Summer";

        Assert.Equal(2, viewModel.FilteredElectricityPrices.Count);
        Assert.Equal(2, viewModel.ElPoints.Count);
    }

    [Fact]
    public void Should_Load_Winter_Data_When_Season_Is_Winter()
    {Console.WriteLine("electricity prices -> Should_Load_Winter_Data_When_Season_Is_Winter");

        var mockSource = CreateMockSource();
        var viewModel = new ElectricityPricesViewModel(mockSource);

        viewModel.SelectedSeason = "Winter";

        Assert.Equal(2, viewModel.FilteredElectricityPrices.Count);
        Assert.Equal(2, viewModel.ElPoints.Count);
    }

    [Fact]
    public void ElSeries_Should_Not_Be_Null()
    {Console.WriteLine("electricity prices -> ElSeries_Should_Not_Be_Null");

        var mockSource = CreateMockSource();
        var viewModel = new ElectricityPricesViewModel(mockSource);

        Assert.NotNull(viewModel.ElSeries);
        Assert.Single(viewModel.ElSeries);
    }
}
