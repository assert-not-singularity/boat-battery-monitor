using CsvHelper.Configuration;

namespace BatMon;

public sealed class CsvMap : ClassMap<BatteryState>
{
    public CsvMap()
    {
        Map(m => m.Time).TypeConverterOption.Format("o");
        Map(m => m.Voltage);
        Map(m => m.Current);
    }
}