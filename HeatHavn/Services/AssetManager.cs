using System.Collections.Generic;
using HeatHavnAppProject.Models;

public class AssetManager
{
    public virtual List<ProductionUnit> GetAllUnits()
    {
        return new List<ProductionUnit>
        {
            new ProductionUnit
            {
                Name = "Gas Boiler 1",
                MaxHeat = 4.0,
                MaxElectricity = 0,
                ProductionCosts = 520,
                CO2Emissions = 175,
                EnergyType = "Gas",
                EnergyConsumption = 0,
                GasConsumption = 0.9,
            },
              new ProductionUnit
            {
                Name = "Gas Boiler 2",
                MaxHeat = 3.0,
                MaxElectricity = 0,
                ProductionCosts = 560,
                CO2Emissions = 130,
                EnergyType = "Gas",
                EnergyConsumption = 0,
                GasConsumption = 0.7,
            },

            new ProductionUnit
            {
                Name = "Oil Boiler 1",
                MaxHeat = 4.0,
                ProductionCosts = 670,
                CO2Emissions = 330,
                EnergyType = "Oil",
                EnergyConsumption = 0,
                OilConsumption = 1.5,
            },
            new ProductionUnit
            {
                Name = "Gas Motor 1",
                MaxHeat = 3.5,
                MaxElectricity = 2.6,
                ProductionCosts = 990,
                CO2Emissions = 650,
                EnergyType = "Gas",
                EnergyConsumption = 0,
                GasConsumption = 1.8,

            },
            new ProductionUnit
            {
                Name = "Heat Pump 1",
                MaxHeat = 6.0,
                MaxElectricity = -6.0,
                ProductionCosts = 60,
                EnergyType = "Electricity",


            }
        };
    }
}