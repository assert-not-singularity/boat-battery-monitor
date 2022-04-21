using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Linq;
using System.Timers;
using Iot.Device.Adc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon
{
    public class SensorReader : IDisposable
    {
        public event EventHandler<SensorReaderEventArgs>? OnValuesRead;

        public float Current => _samples > 1 ? _currents.Average() : _currents.SingleOrDefault();
        public float Voltage => _samples > 1 ? _voltages.Average() : _voltages.SingleOrDefault();

        private readonly int R1 = 100_000;
        private readonly int R2 = 22_000;
        private readonly int R3 = 10_000;
        private readonly int R4 = 3_600;
        private readonly float CurrentSensorVoltageOffset = 0f;

        private readonly float ReferenceVoltage = 2.5f;
        private readonly float VoltageOffset = 0f;

        private readonly Queue<float> _currents;
        private readonly Queue<float> _voltages;
        private readonly int _samples;

        private IConfiguration _configuration;
        private ILogger _logger;

        private SpiDevice _spi;
        private Mcp3204 _mcp;
        private Timer _timer;

        public SensorReader(IConfiguration configuration, ILogger logger, int intervalMs)
        {
            _configuration = configuration.GetSection("SensorReader");
            _logger = logger;

            R1 = _configuration.GetValue("Resistance1", R1);
            R2 = _configuration.GetValue("Resistance2", R2);
            R3 = _configuration.GetValue("Resistance3", R3);
            R4 = _configuration.GetValue("Resistance4", R4);
            CurrentSensorVoltageOffset = _configuration.GetValue("CurrentSensorVoltageOffset", CurrentSensorVoltageOffset);
            VoltageOffset = _configuration.GetValue("VoltageOffset", VoltageOffset);
            ReferenceVoltage = _configuration.GetValue("ReferenceVoltage", ReferenceVoltage);

            _samples = _configuration.GetValue("RollingAverageSamples", 1);
            _samples = _samples <= 0 ? 1 : _samples; // Turn off (= set to 1 sample) if 0 or negative

            _currents = new Queue<float>();
            _voltages = new Queue<float>();

            _logger.LogInformation($"Initialized sensor reader with R1 = {R1} Ω, R2 = {R2} Ω, R3 = {R3} Ω, R4 = {R4} Ω " +
                $"VoltageOffset = {VoltageOffset} V, CurrentSensorOffset = {CurrentSensorVoltageOffset} V, Vref = {ReferenceVoltage} V.");

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

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                // Get information from ADC
                float rawValueCh0 = _mcp.Read(0);
                float rawValueCh1 = _mcp.Read(1);
                float rawValueCh2 = _mcp.Read(2);
                float rawValueCh3 = _mcp.Read(3);

                // Calculate voltage of battery using voltage at voltage divider
                float voltageCh0 = rawValueCh0 / 4096f * ReferenceVoltage;
                float voltageBatt = voltageCh0 * (R1 + R2) / R2 + VoltageOffset;

                _voltages.Enqueue(voltageBatt);
                if (_voltages.Count > _samples)
                {
                    _voltages.Dequeue();
                }

                // Calculate current at hall sensor using voltage at voltage divider
                float voltageCh1 = rawValueCh1 / 4096f * ReferenceVoltage;
                float voltageSensor = voltageCh1 * (R3 + R4) / R4 + CurrentSensorVoltageOffset;
                float currentMotor = (voltageSensor - ReferenceVoltage) / 0.625f * 50f;

                _currents.Enqueue(currentMotor);
                if (_currents.Count > _samples)
                {
                    _currents.Dequeue();
                }

                _logger.LogInformation($"raw0: {rawValueCh0}, Measured Voltage: {voltageCh0:f4} V, Battery Voltage: {voltageBatt:f3} V, Rolling Voltage: {Voltage:f3}"
                    + $"\nraw1: {rawValueCh1}, Measured Current: {voltageSensor:f4} V, Motor Current: {currentMotor:f3} A, Rolling Current {Current:f3}");

                OnValuesRead?.Invoke(this, new SensorReaderEventArgs(Voltage, Current));
            }
            catch (System.IO.IOException ex)
            {
                _logger.LogError($"Could not read from device: {ex.Message}");
                _timer.Stop();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Invalid data received from device: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _mcp.Dispose();
            _spi.Dispose();
        }
    }
}