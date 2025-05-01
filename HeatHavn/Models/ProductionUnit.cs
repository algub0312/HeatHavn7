namespace HeatHavnAppProject.Models;

public class ProductionUnit
{
    public string Name { get; set; } = string.Empty;
    public double MaxHeat { get; set; }
    public double MaxElectricity { get; set; }
    public double ProductionCosts { get; set; }
    public double CO2Emissions { get; set; }
    public string EnergyType { get; set; } = string.Empty;
    public double EnergyConsumption { get; set; }
    public double OilConsumption { get; set; }
    public double GasConsumption { get; set; }
}
