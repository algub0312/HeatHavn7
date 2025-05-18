using Xunit;
using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Models;
using System.Collections.ObjectModel;
using System;
using System.Linq;

public class HeatDemandViewModelTests
{
    private SourceDataManagerViewModel CreateMockSource()
    {
        var mock = new SourceDataManagerViewModel();

        mock.SummerDataHeat.Clear();
        mock.SummerDataHeat.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 8, 1, 10, 0, 0), Value = 50 });
        mock.SummerDataHeat.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 8, 1, 11, 0, 0), Value = 60 });

        mock.WinterDataHeat.Clear();
        mock.WinterDataHeat.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 3, 1, 10, 0, 0), Value = 80 });
        mock.WinterDataHeat.Add(new TimeSeriesEntry { Timestamp = new DateTime(2024, 3, 1, 11, 0, 0), Value = 90 });

        return mock;
    }

    [Fact]
    public void Should_Load_Summer_Data_When_Season_Is_Summer()
    {   Console.WriteLine("heatdemand -> Should_Load_Summer_Data_When_Season_Is_Summer");

        var mock = CreateMockSource();
        var vm = new HeatDemandViewModel(mock);

        vm.SelectedSeason = "Summer";

        Assert.Equal(2, vm.FilteredHeatDemand.Count);
        Assert.Equal(2, vm.HeatDemandPoints.Count);
    }

    [Fact]
    public void Should_Load_Winter_Data_When_Season_Is_Winter()
    {   Console.WriteLine("heatdemand -> Should_Load_Winter_Data_When_Season_Is_Winter");
        var mock = CreateMockSource();
        var vm = new HeatDemandViewModel(mock);

        vm.SelectedSeason = "Winter";

        Assert.Equal(2, vm.FilteredHeatDemand.Count);
        Assert.Equal(2, vm.HeatDemandPoints.Count);
    }

    [Fact]
    public void HeatSeries_Should_Be_Generated()
    {   Console.WriteLine("heatdemand -> HeatSeries_Should_Be_Generated");
        var mock = CreateMockSource();
        var vm = new HeatDemandViewModel(mock);

        Assert.NotNull(vm.HeatSeries);
        Assert.Single(vm.HeatSeries);
    }
}
