namespace BatMon;

public interface IDataLogger
{
    void WriteValues(float voltage, float current);
}