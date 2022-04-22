using System;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon;

public abstract class DataLogger : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    private Timer _timer;

    public DataLogger(IConfiguration configuration, ILogger logger, SensorReader sensorReader)
    {
        _configuration = configuration;
        _logger = logger;

        _timer = new Timer
        {
            AutoReset = true,
            Enabled = true,
            Interval = Math.Round(1000f / _configuration.GetValue("LogFrequencyHz", 1))
        };

        _timer.Elapsed += (sender, e) => WriteValues(sensorReader.Voltage, sensorReader.Current);
    }

    public abstract void WriteValues(float voltage, float current);

    public abstract void Dispose();
}