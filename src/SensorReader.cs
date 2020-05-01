using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Timers;
using Iot.Device.Adc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon
{
    public class SensorReader : IDisposable
    {
        private readonly int R1 = 100_000;
        private readonly int R2 = 22_000;
        private readonly float VoltageReference = 2.5f;

        private IConfiguration _configuration;
        private ILogger _logger;

        private SpiDevice _spi;
        private Mcp3204 _mcp;
        private Timer _timer;

        public SensorReader(IConfiguration configuration, ILogger logger, int intervalMs)
        {
            _configuration = configuration.GetSection("SensorReader");
            _logger = logger;

            R1 = _configuration.GetValue<int>("Resistor1");
            R2 = _configuration.GetValue<int>("Resistor2");
            VoltageReference = _configuration.GetValue<float>("VoltageReference");

            _logger.LogInformation($"Initialized sensor reader with R1 = {R1} Ω, R2 = {R2} Ω, Vref = {VoltageReference} V.");

            var gpio = new GpioController();

            var spiSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = _configuration.GetValue<int>("ClockFrequency")
            };

            _spi = SpiDevice.Create(spiSettings);
            _mcp = new Mcp3204(_spi);

            _timer = new Timer()
            {
                AutoReset = true,
                Enabled = true,
                Interval = intervalMs
            };

            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                float rawValue = _mcp.Read(0);
                float voltageCh0 = rawValue / 4096f * VoltageReference;
                float voltageBatt = voltageCh0 * (R1 + R2) / R2;

                Console.WriteLine($"raw: {rawValue}, value: {voltageCh0.ToString("f4")} V, calc: {voltageBatt.ToString("f3")} V");
            }
            catch (System.IO.IOException ex)
            {
                _logger.LogError($"Could not read from device: {ex.Message}");
                _timer.Stop();
            }
        }

        public void Dispose()
        {
            _mcp.Dispose();
            _spi.Dispose();
        }
    }
}