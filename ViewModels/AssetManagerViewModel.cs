using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Models;
using System.Collections.ObjectModel;

namespace HeatHavnAppProject.ViewModels
{
    public class AssetManagerViewModel : ViewModelBase
    {
        public ObservableCollection<ProductionUnit> ProductionUnits { get; } = new();

        public AssetManagerViewModel()
        {
            // Load dummy data (you can replace with file loading later)
            ProductionUnits.Add(new ProductionUnit
            {
                Name = "GB1",
                MaxHeat = 4.0,
                ProductionCosts = 520,
                CO2Emissions = 175,
                EnergyType = "Gas",
                EnergyConsumption = 0.9
            });
            ProductionUnits.Add(new ProductionUnit
            {
                Name = "GB2",
                MaxHeat = 3.0,
                ProductionCosts = 560,
                CO2Emissions = 130,
                EnergyType = "Gas",
                EnergyConsumption = 0.7
            });
            ProductionUnits.Add(new ProductionUnit
            {
                Name = "OB1",
                MaxHeat = 4.0,
                ProductionCosts = 670,
                CO2Emissions = 330,
                EnergyType = "Oil",
                OilConsumption = 1.5
            });

            ProductionUnits.Add(new ProductionUnit
            {
                Name = "GM1",
                MaxHeat = 3.5,
                MaxElectricity = 2.6,
                ProductionCosts = 990,
                CO2Emissions = 650,
                EnergyType = "Gas",
                EnergyConsumption = 1.8
            });

            ProductionUnits.Add(new ProductionUnit
            {
                Name = "HP1",
                MaxHeat = 6.0,
                MaxElectricity = -6.0,
                ProductionCosts = 60,
                EnergyType = "Electricity"
            });

            // Add GB2, OB1 etc. similarly
        }
    }
}

