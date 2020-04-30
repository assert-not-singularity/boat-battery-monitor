using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Adc;

namespace BatMon
{
    class Program
    {
        const int R1 = 99_920;
        const int R2 = 21_990;
        const float ReferenceVoltage = 2.502f;

        static void Main(string[] args)
        {
            var gpio = new GpioController();

            var spiSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 10_000
            };

            using (var spi = SpiDevice.Create(spiSettings))
            {
                var mcp = new Mcp3204(spi);

                while (true)
                {
                    float rawValue = mcp.Read(0);
                    float voltageCh0 = rawValue / 4096f * ReferenceVoltage;
                    float voltageBatt = voltageCh0 * (R1 + R2) / R2;

                    Console.WriteLine($"raw: {rawValue}, value: {voltageCh0.ToString("f4")} V, calc: {voltageBatt.ToString("f3")} V");

                    Thread.Sleep(200);
                }
            }
        }
    }
}
