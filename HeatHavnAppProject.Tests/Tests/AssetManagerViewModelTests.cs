using System.Linq;
using Xunit;
using HeatHavnAppProject.Models;

using HeatHavnAppProject.ViewModels;

public class AssetManagerViewModelTests
{
    [Fact]
    public void AllUnits_Should_Be_Initialized()
    {
        // Arrange
        var viewModel = new AssetManagerViewModel();

        // Act
        var units = viewModel.AllUnits;

        // Assert
        Assert.NotNull(units);
        Assert.NotEmpty(units); // You expect predefined units
    }

    [Fact]
    public void Should_Load_All_ProductionUnits_From_AssetManager()
    {
        // Arrange
        var expectedUnitsCount = new AssetManager().GetAllUnits().Count;
        var viewModel = new AssetManagerViewModel();

        // Act
        var actualUnitsCount = viewModel.AllUnits.Count;

        // Assert
        Assert.Equal(expectedUnitsCount, actualUnitsCount);
    }
}
