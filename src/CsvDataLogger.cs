using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon;

public class CsvDataLogger : DataLogger
{
    private IConfiguration _configuration;
    private ILogger _logger;
    private Stream? _stream;
    private StreamWriter? _writer;
    private CsvWriter? _csv;

    public CsvDataLogger(IConfiguration configuration, ILogger logger, SensorReader sensorReader) : base(configuration, logger, sensorReader)
    {
        _configuration = configuration.GetSection("CSV");
        _logger = logger;

        var logFile = _configuration.GetValue("LogFile", "log.csv");
        var newFile = false;

        // Create file if not already exists
        if (!File.Exists(logFile))
        {
            _logger.LogInformation($"Data log file {logFile} not found. Creating new one...");
            newFile = true;
        }

        _logger.LogInformation($"Using CSV file {logFile}.");

        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = true,
            NewLine = Environment.NewLine
        };

        try
        {
            _stream = File.Open(logFile, FileMode.Append);
            _writer = new StreamWriter(_stream);
            _csv = new CsvWriter(_writer, csvConfig);
            _csv.Context.RegisterClassMap<CsvMap>();

            if (newFile)
            {
                _csv.WriteHeader<BatteryState>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not open file: {ex}");

            _csv?.Dispose();
            _writer?.Dispose();
            _stream?.Dispose();
        }
    }

    public override void WriteValues(float voltage, float current)
    {
        if (_csv is null)
        {
            return;
        }

        var record = new BatteryState()
        {
            Time = DateTime.Now,
            Voltage = voltage,
            Current = current
        };

        _csv.WriteRecord(record);
        _csv.NextRecord();
        _csv.Flush();
    }

    public override void Dispose()
    {
        _csv?.Dispose();
        _writer?.Dispose();
        _stream?.Dispose();
    }
}