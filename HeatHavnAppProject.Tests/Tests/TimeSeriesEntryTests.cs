using System;
using Xunit;
using HeatHavnAppProject.Models;
using HeatHavnAppProject.ViewModels;

public class TimeSeriesEntryTests
{
    [Fact]
    public void Should_Assign_And_Retrieve_Properties_Correctly()
    {
        // Arrange
        var timestamp = new DateTime(2024, 5, 1, 12, 0, 0);
        var value = 42.5;

        var entry = new TimeSeriesEntry
        {
            Timestamp = timestamp,
            Value = value
        };

        // Act & Assert
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.Equal(value, entry.Value);
    }

    [Fact]
    public void Should_Have_Default_Values_When_Initialized()
    {
        // Arrange
        var entry = new TimeSeriesEntry();

        // Act & Assert
        Assert.Equal(default(DateTime), entry.Timestamp);
        Assert.Equal(0, entry.Value);
    }
}
