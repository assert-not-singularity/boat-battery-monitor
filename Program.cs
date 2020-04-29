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
        const int R2 = 22_000;

        static void Main(string[] args)
        {
            var gpio = new GpioController();

            var spiSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 50_000
            };

            using (var spi = SpiDevice.Create(spiSettings))
            {
                var mcp = new Mcp3204(spi);

                while (true)
                {
                    float value = mcp.Read(0) / 4096f * 3.318f;

                    // Voltage divider
                    float voltage = value * (R1 + R2) / R2;

                    Console.WriteLine(voltage);

                    Thread.Sleep(200);
                }
            }
        }
    }
}
