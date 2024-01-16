namespace NureBot.Service;

public class TimeService
{
    public static DateTime UnixTimeStampToDateTime( long unixTimeStamp )
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }
}