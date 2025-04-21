using System;
using CsvHelper.Configuration.Attributes;

public class RawSourceDataEntry
{
    [Name("Time from")]
[TypeConverter(typeof(CustomDateTimeConverter))]
public DateTime TimeFrom { get; set; }

    [Name("Time to")]
[TypeConverter(typeof(CustomDateTimeConverter))]
public DateTime TimeTo { get; set; }

    [Name("Heat Demand")]
    public double HeatDemand { get; set; }

    [Name("Electricity Price")]
    public double ElectricityPrice { get; set; }
}
