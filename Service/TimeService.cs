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

    public static void LessonNotification(BotClient bot)
    {
        try
        {
            while (true)
            {
                using (Context db = new Context())
                {
                    Dictionary<string, List<Event>> Schedule = new Dictionary<string, List<Event>>();
                    List<string?> CistNames = new List<string?>();
                    List<string> ValidCistNames = new List<string>();
                    DateTime NowUTC = DateTime.UtcNow;
                    TimeZoneInfo kyivZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
                    DateTime NowKyiv = TimeZoneInfo.ConvertTimeFromUtc(NowUTC, kyivZone);
                    long NowKyivStamp = (long)NowKyiv.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                    if (NowKyiv.Hour == 6 && NowKyiv.Minute == 0 && NowKyiv.Second == 0)
                    {
                        CistNames = db.Customers.Where(c => c.CistName != null)
                            .Select(c => c.CistName).Distinct().ToList();
                        foreach (var cistName in CistNames)
                        {
                            string Type;
                            long? Cistid;
                            if (cistName.Split().Length == 1)
                            {
                                Type = "group";
                                Cistid = db.Groups.ToList().Find(x => x.Name == cistName).Id;
                            }
                            else
                            {
                                Type = "teacher";
                                Cistid = db.Teachers.ToList().Find(x => x.ShortName == cistName).Id;
                            }

                            DateTime startDateTime = TimeZoneInfo.ConvertTime(DateTime.Now.Date, kyivZone);
                            long startTime = (long)startDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                            long endTime = startTime + 86400;
                            List<Event>? ScheduleEvent = EventParser.GetSchedule(Type, Cistid, startTime, endTime);
                            if ((ScheduleEvent != null) && (ScheduleEvent.Count != 0))
                            {
                                ScheduleEvent = ScheduleEvent.OrderBy(c => c.StartTime).ToList();
                                Schedule[cistName] = ScheduleEvent;
                                ValidCistNames.Add(cistName);
                            }
                        }
                    }

                    if (ValidCistNames != null)
                    {
                        foreach (var cistName in ValidCistNames)
                        {
                            if ((Schedule[cistName][0].StartTime - NowKyivStamp) <= 300 &&
                                ((Schedule[cistName][0].StartTime - NowKyivStamp) > 0))
                            {
                                List<long> ChatIds = db.Customers.Where(c => c.CistName == cistName)
                                    .Select(c => c.ChatId).ToList();
                                var lesson = Schedule[cistName][0];
                                foreach (var chatId in ChatIds)
                                {
                                    bot.SendMessage(chatId,
                                        $"Нагадування! До пари з \"{lesson.Subject.Title}\" ({lesson.Type}) залишилося менше 5 хвилин.");
                                    Thread.Sleep(500);
                                }
                                Schedule[cistName].Remove(lesson);
                                if (Schedule[cistName] == null)
                                {
                                    ValidCistNames.Remove(cistName);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Thread.Sleep(1000);
            bot.SendMessage("-1002108311720", $"Сталася помилка: \n \n {JsonConvert.SerializeObject(e)}");
        }
    }
}