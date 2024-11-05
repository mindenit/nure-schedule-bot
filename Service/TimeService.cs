using Newtonsoft.Json;
using NureBot.Classes;
using NureBot.Contexts;
using NureBot.Parsers;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;

namespace NureBot.Service;

public class TimeService
{
    public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }
}