using System;

public class SensorReaderEventArgs : EventArgs
{
    public SensorReaderEventArgs(float voltage, float current)
    {
        Current = current;
        Voltage = voltage;
    }

    public float Current { get; }
    public float Voltage { get; }
}