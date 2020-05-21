using System.Diagnostics;


public class Timer
{
    public enum Type : int
    {
        SECOND = 1,
        MILLISECOND = 1000,
        MICROSECOND = 1000000
    }

    public static long getElapsedTime(long currentTimestamp, long previousTimestamp, Type type)
    {
        return (long)(((currentTimestamp - previousTimestamp) / (float)Stopwatch.Frequency) * (int)type);
    }

    public static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }
}