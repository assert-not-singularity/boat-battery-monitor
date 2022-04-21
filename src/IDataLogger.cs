using System;

namespace BatMon;

public interface IDataLogger : IDisposable
{
    void WriteValues(float voltage, float current);
}