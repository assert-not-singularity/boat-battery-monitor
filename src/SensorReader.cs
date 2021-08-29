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
        public event EventHandler<SensorReaderEventArgs> OnValuesRead;

        private readonly int R1 = 100_000;
        private readonly int R2 = 22_000;
        private readonly int R3 = 3_600;
        private readonly int R4 = 10_000;
        private readonly float ReferenceVoltage = 2.5f;

        private IConfiguration _configuration;
        private ILogger _logger;

        private SpiDevice _spi;
        private Mcp3204 _mcp;
        private Timer _timer;

        public SensorReader(IConfiguration configuration, ILogger logger, int intervalMs)
        {
            _configuration = configuration.GetSection("SensorReader");
            _logger = logger;

            R1 = _configuration.GetValue<int>("Resistance1", R1);
            R2 = _configuration.GetValue<int>("Resistance2", R2);
            R3 = _configuration.GetValue<int>("Resistance3", R3);
            R4 = _configuration.GetValue<int>("Resistance4", R4);
            ReferenceVoltage = _configuration.GetValue<float>("ReferenceVoltage", ReferenceVoltage);

            _logger.LogInformation($"Initialized sensor reader with R1 = {R1} Ω, R2 = {R2} Ω, R3 = {R3} Ω, R4 = {R4} Ω, Vref = {ReferenceVoltage} V.");

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
                float rawValueCh0 = _mcp.Read(0);
                float rawValueCh1 = _mcp.Read(1);

                float voltageCh0 = rawValueCh0 / 4096f * ReferenceVoltage;
                float voltageBatt = voltageCh0 * (R1 + R2) / R2;

                float voltageCh1 = rawValueCh1 / 4096f * ReferenceVoltage;
                float voltageSensor = voltageCh1 * (R3 + R4) / R4;
                float currentMotor = (voltageSensor - ReferenceVoltage) / 0.625f * 50f;

                _logger.LogInformation($"raw0: {rawValueCh0}, Measured Voltage: {voltageCh0.ToString("f4")} V, Battery Voltage: {voltageBatt.ToString("f3")} V"
                    + $"\nraw1: {rawValueCh1}, Measured Current: {voltageSensor.ToString("f4")} A, Motor Current: {currentMotor.ToString("f3")} A");

                OnValuesRead?.Invoke(this, new SensorReaderEventArgs(voltageBatt, currentMotor));
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