using System;
using CsvHelper.Configuration.Attributes;
using InfluxDB.Client.Core;

namespace BatMon;

[Measurement("battery_state")]
public class BatteryState
{
    [Column(IsTimestamp = true)]
    [Name("Time")]
    [Index(0)]
    public DateTime Time;

    [Column("voltage")]
    [Name("Voltage")]
    [Index(1)]
    public float Voltage { get; set; }

    [Column("current")]
    [Name("Current")]
    [Index(2)]
    public float Current { get; set; }
}