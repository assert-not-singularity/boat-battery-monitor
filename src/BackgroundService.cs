using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BatMon
{
    public class BackgroundService : IHostedService
    {
        private IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        private IDataLogger _dataLogger;
        private SensorReader _sensorReader;

        public BackgroundService(IConfiguration configuration, ILogger<BackgroundService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _configuration = configuration;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void Init()
        {
            var dataLoggerName = _configuration.GetValue("DataLogger", "");
            _dataLogger = dataLoggerName switch
            {
                "CSV" => _dataLogger = new CsvDataLogger(_configuration, _logger),
                "Influx" => _dataLogger = new InfluxDataLogger(_configuration, _logger),
                _ => _dataLogger = new MockDataLogger()
            };

            _sensorReader = new SensorReader(_configuration, _logger, 200);
            _sensorReader.OnValuesRead += (sender, e) => _dataLogger.WriteValues(e.Voltage, e.Current);
        }

        private void OnStarted()
        {
            _logger.LogInformation("Service started.");

            Init();
        }

        private void OnStopping()
        {
            _logger.LogInformation("Service stopping...");

            _sensorReader.Dispose();
        }

        private void OnStopped()
        {
            _logger.LogInformation("Service stopped.");
        }
    }
}