using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace HeatHavnAppProject.Tests
{
    public class OptimizerTests
    {
        private readonly SourceDataManagerViewModel _realSource;
        private readonly AssetManager _fakeAsset;
        private readonly ObservableCollection<TimeSeriesEntry> _winterHeat;
        private readonly ObservableCollection<TimeSeriesEntry> _winterEl;
        private readonly List<ProductionUnit> _units;

        class TestAssetManager : AssetManager
        {
            readonly List<ProductionUnit> _units;
            public TestAssetManager(List<ProductionUnit> units) => _units = units;
            public override List<ProductionUnit> GetAllUnits() => _units;
        }

        private AssetManagerViewModel GetStubAssetManagerViewModel()
        {
            var vm = new AssetManagerViewModel();
            foreach (var unit in _units)
            {
                vm.AllUnits.Add(unit);
            }
            return vm;
        }

        public OptimizerTests()
        {
            var timestamp = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);

            _winterHeat = new ObservableCollection<TimeSeriesEntry>
            {
                new TimeSeriesEntry { Timestamp = timestamp.DateTime, Value = 10 }
            };

            _winterEl = new ObservableCollection<TimeSeriesEntry>
            {
                new TimeSeriesEntry { Timestamp = timestamp.DateTime, Value = 5 }
            };

            _units = new List<ProductionUnit>
            {
                new ProductionUnit
                {
                    Name = "Gas Boiler 1",
                    EnergyType = "Gas",
                    MaxHeat = 100,
                    ProductionCosts = 1.0,
                    CO2Emissions = 0.5
                }
            };

            _realSource = new SourceDataManagerViewModel();
            _realSource.WinterDataHeat.Clear();
            foreach (var e in _winterHeat) _realSource.WinterDataHeat.Add(e);
            _realSource.SummerDataHeat.Clear();
            _realSource.WinterDataEl.Clear();
            foreach (var e in _winterEl) _realSource.WinterDataEl.Add(e);
            _realSource.SummerDataEl.Clear();

            _fakeAsset = new TestAssetManager(_units);
        }

        [Fact]
        public void StartDate_IsUpdated_WhenDayMonthHourChange()
        {
            var opt = new Optimizer(_realSource, _fakeAsset, GetStubAssetManagerViewModel());

            opt.StartMonth = "August";
            opt.StartDay = 15;
            opt.StartHour = 5;

            var expected = new DateTimeOffset(2024, 8, 15, 5, 0, 0, DateTimeOffset.Now.Offset);
            Assert.Equal(expected, opt.StartDate.Value);
        }

        [Fact]
        public void OptimizeCommand_GeneratesCsvAndTotals_ForScenario1Cost()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "test_optimizer.csv");

            var opt = new Optimizer(_realSource, _fakeAsset, GetStubAssetManagerViewModel())
            {
                StartMonth = "March",
                EndMonth = "March",
                StartDay = 1,
                EndDay = 1,
                StartHour = 0,
                EndHour = 1, // <= fix: should be exclusive end
                Scenario1Enabled = true,
                SelectedOptimization = "Cost"
            };

            typeof(Optimizer)
                .GetField("_lastOutputPath", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(opt, tempFile);

            opt.OptimizeCommand.Execute(null);

            var lines = File.ReadAllLines(tempFile);
            Assert.Equal("TimeFrom,GB1,GB2,OB1,HeatDemand,CO2,Cost", lines[0]);
            Assert.Equal(10, opt.TotalCost, 3);
            Assert.Equal(5, opt.TotalCO2, 3);
        }

        [Fact]
        public void LoadGraphFromCsv_PopulatesSeries_ForScenario1Csv()
        {
            var tempFile = Path.GetTempFileName();

            File.WriteAllLines(tempFile, new[]
            {
                "TimeFrom,GB1,GB2,OB1,HeatDemand,CO2",
                "2024-03-01 00:00,5,0,5,10,100"
            });

            var opt = new Optimizer(_realSource, _fakeAsset, GetStubAssetManagerViewModel());

            typeof(Optimizer)
                .GetMethod("LoadGraphFromCsv", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(opt, new object[] { tempFile });

            Assert.NotEmpty(opt.Series);
            var stacked = opt.Series.OfType<StackedAreaSeries<double>>().ToList();
            Assert.Equal("GB1", stacked[0].Name);
            var line = opt.Series.OfType<LineSeries<double>>().Single();
            Assert.Equal("Heat Demand", line.Name);
        }
    }
}
