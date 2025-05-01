using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public class CustomDateTimeConverter : DateTimeConverter
{
    private readonly string _format = "dd/MM/yyyy HH:mm";

    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (DateTime.TryParseExact(text, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        return base.ConvertFromString(text, row, memberMapData);
    }
}
