using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon
{
    public class InfluxDataLogger : IDataLogger
    {
        private const string database = "tests";
        private const string retentionPolicy = "autogen";

        private IConfiguration _configuration;
        private ILogger _logger;

        private readonly InfluxDBClient _client;

        public InfluxDataLogger(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration.GetSection("InfluxDB");
            _logger = logger;

            _logger.LogInformation("Connecting to InfluxDB...");

            var user = _configuration.GetValue<string>("Username");
            var pass = _configuration.GetValue<string>("Password");

            _client = InfluxDBClientFactory.CreateV1("http://diskstation.local:8086", user, pass.ToCharArray(), database, retentionPolicy);
        }

        public void WriteValues(float voltage, float current)
        {
            using (var writeApi = _client.GetWriteApi())
            {
                var measurement = new BatteryState
                {
                    Voltage = voltage,
                    Current = current,
                    Time = DateTime.Now
                };

                writeApi.WriteMeasurement<BatteryState>(WritePrecision.Ms, measurement);
            }
        }
    }
}