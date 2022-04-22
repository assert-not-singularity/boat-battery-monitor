using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon
{
    public class InfluxDataLogger : DataLogger
    {
        private const string database = "tests";
        private const string retentionPolicy = "autogen";

        private IConfiguration _configuration;
        private ILogger _logger;

        private readonly InfluxDBClient _client;
        private readonly WriteApi _writeApi;

        public InfluxDataLogger(IConfiguration configuration, ILogger logger, SensorReader sensorReader) : base(configuration, logger, sensorReader)
        {
            _configuration = configuration.GetSection("InfluxDB");
            _logger = logger;

            _logger.LogInformation("Connecting to InfluxDB...");

            var user = _configuration.GetValue<string>("Username");
            var pass = _configuration.GetValue<string>("Password");

            _client = InfluxDBClientFactory.CreateV1("http://diskstation.local:8086", user, pass.ToCharArray(), database, retentionPolicy);
            _writeApi = _client.GetWriteApi();
        }

        public override void WriteValues(float voltage, float current)
        {
            var measurement = new BatteryState
            {
                Voltage = voltage,
                Current = current,
                Time = DateTime.Now
            };

            _writeApi.WriteMeasurement<BatteryState>(WritePrecision.Ms, measurement);
        }

        public override void Dispose()
        {
            _writeApi.Dispose();
            _client.Dispose();
        }
    }
}