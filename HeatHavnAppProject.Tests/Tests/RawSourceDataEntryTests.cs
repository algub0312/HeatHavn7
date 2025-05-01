using System;
using Xunit;
using HeatHavnAppProject.Models;
using HeatHavnAppProject.ViewModels;

public class RawSourceDataEntryTests
{
    [Fact]
    public void Should_Assign_And_Retrieve_Properties_Correctly()
    {
        // Arrange
        var now = DateTime.Now;
        var entry = new RawSourceDataEntry
        {
            TimeFrom = now,
            TimeTo = now.AddHours(1),
            HeatDemand = 123.45,
            ElectricityPrice = 567.89
        };

        // Act & Assert
        Assert.Equal(now, entry.TimeFrom);
        Assert.Equal(now.AddHours(1), entry.TimeTo);
        Assert.Equal(123.45, entry.HeatDemand);
        Assert.Equal(567.89, entry.ElectricityPrice);
    }

    [Fact]
    public void Should_Have_Default_Values_When_Initialized()
    {
        // Arrange
        var entry = new RawSourceDataEntry();

        // Act & Assert
        Assert.Equal(default(DateTime), entry.TimeFrom);
        Assert.Equal(default(DateTime), entry.TimeTo);
        Assert.Equal(0, entry.HeatDemand);
        Assert.Equal(0, entry.ElectricityPrice);
    }
}
