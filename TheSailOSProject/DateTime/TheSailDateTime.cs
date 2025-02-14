using System;

namespace TheSailOSProject.DateTime;

public static class TheSailDateTime
{
    public static void ShowDate()
    {
        Console.WriteLine(Cosmos.HAL.RTC.DayOfTheMonth + "-" + Cosmos.HAL.RTC.Month + "-" + Cosmos.HAL.RTC.Year);
    }

    public static void ShowTime()
    {
        Console.WriteLine(Cosmos.HAL.RTC.Hour + ":" + Cosmos.HAL.RTC.Minute + ":" + Cosmos.HAL.RTC.Second);
    }
}