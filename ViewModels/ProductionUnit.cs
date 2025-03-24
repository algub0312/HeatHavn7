namespace HeatHavnAppProject.Models
{
    public class ProductionUnit
    {
        public string Name { get; set; }
        public double MaxHeat { get; set; } // MW
        public double? MaxElectricity { get; set; } // MW (null if N/A)
        public double ProductionCosts { get; set; } // DKK / MWh(th)
        public double? CO2Emissions { get; set; } // kg / MWh(th)
        public string EnergyType { get; set; } // e.g., Gas, Oil, Electricity
        public double? EnergyConsumption { get; set; } // MWh / MWh(th)
        public double? OilConsumption { get; set; } // MWh / MWh(th) 
        public string? ImagePath { get; set; } // optional
    }
}
