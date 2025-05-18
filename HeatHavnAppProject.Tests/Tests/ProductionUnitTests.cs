using Xunit;
using HeatHavnAppProject.Models;
using HeatHavnAppProject.ViewModels;

namespace HeatHavnAppProject.Tests
{
    public class ProductionUnitTests
    {
        [Fact]
        public void ProductionUnit_Should_Have_Expected_DefaultValues()
        {
            // Arrange
            var unit = new ProductionUnit
            {
                Name = "Test Boiler",
                MaxHeat = 4.0,
                MaxElectricity = 0,
                ProductionCosts = 500,
                CO2Emissions = 200,
                EnergyType = "Gas"
            };
            

            // Assert
            Assert.Equal("Test Boiler", unit.Name);
            Assert.Equal(4.0, unit.MaxHeat);
            Assert.Equal(0, unit.MaxElectricity);
            Assert.Equal(500, unit.ProductionCosts);
            Assert.Equal(200, unit.CO2Emissions);
            Assert.Equal("Gas", unit.EnergyType);
        }
    }
}
