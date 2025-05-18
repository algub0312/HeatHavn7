using System;
using System.Collections.Generic;
using Xunit;
using HeatHavnAppProject.ViewModels; // Adjust this if needed
using HeatHavnAppProject.Models;     // Needed for RawSourceDataEntry

public class SourceDataManagerViewModelTests
{
    [Fact]
    public void LoadSummerDataHeat_ShouldPopulateSummerDataCollection()
    {Console.WriteLine("source data manager -> LoadSummerDataHeat_ShouldPopulateSummerDataCollection");

        // Arrange
        var mockData = new List<RawSourceDataEntry>
        {
            new RawSourceDataEntry
            {
                TimeFrom = new DateTime(2024, 1, 1, 12, 0, 0),
                TimeTo = new DateTime(2024, 1, 1, 13, 0, 0),
                HeatDemand = 123.45,
                ElectricityPrice = 0
            }
        };

        var viewModel = new MockSourceDataManagerViewModel(mockData);

        // Act
        viewModel.LoadSummerDataHeat("fake.csv"); // Filename is ignored due to override

        Assert.Single(viewModel.SummerDataHeat);
        Assert.Equal(123.45, viewModel.SummerDataHeat[0].Value);
        Assert.Equal(new DateTime(2024, 1, 1, 12, 0, 0), viewModel.SummerDataHeat[0].Timestamp);
    }

    // Inner mock class that overrides LoadCsvRaw
    private class MockSourceDataManagerViewModel : SourceDataManagerViewModel
    {
        private readonly List<RawSourceDataEntry> _mockData;

        public MockSourceDataManagerViewModel(List<RawSourceDataEntry> mockData)
        {
            _mockData = mockData;
        }

        protected override List<RawSourceDataEntry> LoadCsvRaw(string path)
        {
            return _mockData;
        }
    }
}
