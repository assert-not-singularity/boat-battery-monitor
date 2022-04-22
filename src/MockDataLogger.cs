using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BatMon;

public class MockDataLogger : DataLogger
{
    public MockDataLogger(IConfiguration configuration, ILogger logger, SensorReader sensorReader) : base(configuration, logger, sensorReader)
    {
    }

    public override void WriteValues(float voltage, float current)
    {
    }

    public override void Dispose()
    {
    }
}