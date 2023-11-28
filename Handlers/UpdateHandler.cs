using System.Globalization;
using NureBot.Classes;
using NureBot.Contexts;
using NureBot.Parsers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NureBot.Service;

namespace NureBot.Handlers;

public class UpdateHandler
{
    public static async Task HandleUpdateAsync(TelegramBotClient bot, Update update,
        CancellationToken cancellationToken)
    {
        var message = update.Message;
        TimeZoneInfo kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        CultureInfo cultureInfo = new CultureInfo("uk-UA");
        string DonateHtml = "\n" + "<a href=\"https://t.me/nure_dev\">Канал з інфою</a> | " +
                            "<a href=\"https://send.monobank.ua/jar/5tHDuV8dfg\">Підтримати розробку</a> | " +
                            "<a href=\"https://t.me/ketronix_dev\">Адмін</a> | " +
                            "<a href=\"https://github.com/nure-dev/nure-bot\">Код</a>" + "\n";
    await using(Context db = new Context())
        {
            if (db.Customers.Find(message.Chat.Id) == null)
            {
                Customer customer = new Customer();
                if (message.Chat.Type.ToString() == "Private")
                {
                    customer.ChatId = message.Chat.Id;
                    customer.FirstName = message.Chat.FirstName;
                    customer.LastName = message.Chat.LastName;
                    customer.Username = message.Chat.Username;
                }
                else
                {
                    customer.ChatId = message.Chat.Id;
                    customer.FirstName = message.Chat.Title;
                }

                db.Customers.Add(customer);
                db.SaveChanges();
            }
        }
        try
        {
            if (update.Type == UpdateType.Message)
            {
                if (message is not null && message.Text is not null)
                {
                    string[] words = message.Text.Split();
                    string command = words[0];
                    if (message.Text.Contains("/choose"))
                    {
                        try
                        {
                            using (Context context = new Context())
                            {
                                var existingCustomer = context.Customers.Find(message.Chat.Id);
                                if (existingCustomer.ChatType == null)
                                {
                                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                                    existingCustomer.CistName = context.Groups.ToList()
                                        .Find(x => x.Name.ToUpper() == words[1].ToUpper()).Name;
                                    existingCustomer.CistId = context.Groups.ToList().Find(x =>
                                        x.Name.ToUpper() == existingCustomer.CistName.ToUpper()).Id;
                                    existingCustomer.ChatType = message.Chat.Type.ToString();
                                }
                                else
                                {
                                    existingCustomer.CistName = context.Groups.ToList()
                                        .Find(x => x.Name.ToUpper() == words[1].ToUpper()).Name;
                                    existingCustomer.CistId = context.Groups.ToList()
                                        .Find(x => x.Name.ToUpper() == existingCustomer.CistName.ToUpper()).Id;
                                }

                                bot.SendTextMessageAsync(message.Chat.Id, $"Дякую за реєстрацію!");
                                context.SaveChanges();
                            }
                        }
                        catch (Exception e)
                        {
                            bot.SendTextMessageAsync(message.Chat.Id, $"Вибачте, але група вказана неправильно");
                            Console.WriteLine($"Error: {e}");

                        }
                    }
                    else if (message.Text.Contains("/day"))
                    {
                        using (Context context = new Context())
                            {

                                if (context.Customers.ToList().Find(x => x.ChatId == message.Chat.Id).ChatType != null)
                                {
                                    DateTime startDateTime = TimeZoneInfo.ConvertTime(DateTime.Now.Date, kyivTimeZone);
                                    long? Cistid = context.Customers.ToList()
                                        .Find(x => x.ChatId == message.Chat.Id).CistId;
                                    long startTime =
                                        (long)startDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                    long endTime = startTime + 86400;
                                    List<Event> scheduleEvent =
                                        EventParser.GetSchedule("group", Cistid, startTime, endTime);
                                    scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();

                                    string TextDay = $"Розклад на {startDateTime.ToString("dd.MM.yyyy")}:\n";
                                    if (scheduleEvent.Count == 0)
                                    {
                                        TextDay += "Пар нема. Відпочивайте!\n";
                                    }
                                    else
                                    {
                                        foreach (var Event in scheduleEvent)
                                        {
                                            DateTime Start = TimeZoneInfo.ConvertTime(TimeService.UnixTimeStampToDateTime(Event.StartTime), kyivTimeZone);
                                            DateTime End = TimeZoneInfo.ConvertTime(TimeService.UnixTimeStampToDateTime(Event.EndTime), kyivTimeZone);
                                            string Teacher;
                                            if (Event.Teachers.Count == 0)
                                            {
                                                Teacher = "Не визначено";
                                            }
                                            else
                                            {
                                                Teacher = Event.Teachers.First().ShortName;
                                            }

                                            TextDay +=
                                                $"{Start.ToString("HH:mm")} - {End.ToString("HH:mm")} | <b> {Event.Subject.Brief} - {Event.Type} </b> | {Teacher}\n";
                                        }
                                    }

                                    bot.SendTextMessageAsync(message.Chat.Id, TextDay + DonateHtml, parseMode: ParseMode.Html, disableWebPagePreview: true);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(message.Chat.Id, "Вибачте, але ви не зареєструвались");
                                }
                            }
                    } else if (message.Text.Contains("/week"))
                    {
                        using (Context context = new Context())
                            {
                                if (context.Customers.ToList().Find(x => x.ChatId == message.Chat.Id).ChatType != null)
                                {
                                long? Cistid = context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistId;
                                DateTime mondayDateTime = TimeZoneInfo.ConvertTime(DateTime.Today.AddDays(DayOfWeek.Monday - DateTime.Today.DayOfWeek),
                                    kyivTimeZone);
                                int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mondayDateTime,
                                    CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                                    CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
                                string weekText =
                                    $"Розклад {mondayDateTime.ToString("dd.MM")} - {mondayDateTime.AddDays(6).ToString("dd.MM")} ({weekOfYear}):\n";
                                for (int i = 0; i < 6; i++)
                                {
                                    DateTime startDateTime = mondayDateTime.AddDays(i);
                                    DateTime endDateTime = mondayDateTime.AddDays(i + 1);
                                    long startTime = (long)startDateTime.Subtract(new DateTime(1970, 1, 1))
                                        .TotalSeconds;
                                    long endTime = startTime + 86400;
                                    List<Event> scheduleEvent =
                                        EventParser.GetSchedule("group", Cistid, startTime, endTime);
                                    scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();
                                    string TextDay = $"{startDateTime.ToString("dd.MM.yyyy")} ({startDateTime.ToString("dddd", cultureInfo)}):\n";
                                    TextDay = cultureInfo.TextInfo.ToTitleCase(TextDay);
                                    if (scheduleEvent.Count == 0)
                                    {
                                        weekText += TextDay + "Пар нема. Відпочивайте!\n\n";
                                    }
                                    else
                                    {
                                        foreach (var Event in scheduleEvent)
                                        {
                                            DateTime Start = TimeZoneInfo.ConvertTime(TimeService.UnixTimeStampToDateTime(Event.StartTime), kyivTimeZone);
                                            DateTime End = TimeZoneInfo.ConvertTime(TimeService.UnixTimeStampToDateTime(Event.EndTime), kyivTimeZone);
                                            string Teacher;
                                            if (Event.Teachers.Count == 0)
                                            {
                                                Teacher = "Не визначено";
                                            }
                                            else
                                            {
                                                Teacher = Event.Teachers.First().ShortName;
                                            }

                                            TextDay +=
                                                $"{Start.ToString("HH:mm")} - {End.ToString("HH:mm")} | <b> {Event.Subject.Brief} - {Event.Type} </b> | {Teacher}\n";
                                        }

                                        weekText += TextDay + "\n";
                                    }
                                }

                                bot.SendTextMessageAsync(message.Chat.Id, weekText + DonateHtml, parseMode: ParseMode.Html, disableWebPagePreview: true);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(message.Chat.Id, "Вибачте, але ви не зареєструвались");
                                }
                            }
                    } else if (message.Text.Contains("/next_day"))
                    {
                        using (Context context = new Context())
                            {
                                if (context.Customers.ToList().Find(x => x.ChatId == message.Chat.Id).ChatType != null)
                                {
                                DateTime startDateTime = TimeZoneInfo.ConvertTime(DateTime.Today.AddDays(1),
                                    kyivTimeZone);
                                long? Cistid = context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistId;
                                long startTime = (long)startDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                long endTime = startTime + 86400;
                                List<Event> scheduleEvent = EventParser.GetSchedule("group", Cistid, startTime, endTime);
                                scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();

                                string TextDay = $"Розклад на {startDateTime.ToString("dd.MM.yyyy")}:\n";
                                if (scheduleEvent.Count == 0)
                                {
                                    TextDay += "Пар нема. Відпочивайте!\n";
                                }
                                else
                                {
                                    foreach (var Event in scheduleEvent)
                                    {
                                        DateTime Start = TimeZoneInfo.ConvertTime(TimeService.UnixTimeStampToDateTime(Event.StartTime), kyivTimeZone);
                                        DateTime End = TimeZoneInfo.ConvertTime(TimeService.UnixTimeStampToDateTime(Event.EndTime), kyivTimeZone);
                                        string Teacher;
                                        if (Event.Teachers.Count == 0)
                                        {
                                            Teacher = "Не визначено";
                                        }
                                        else
                                        {
                                            Teacher = Event.Teachers.First().ShortName;
                                        }

                                        TextDay +=
                                            $"{Start.ToString("HH:mm")} - {End.ToString("HH:mm")} | <b> {Event.Subject.Brief} - {Event.Type} </b> | {Teacher}\n";
                                    }
                                }

                                bot.SendTextMessageAsync(message.Chat.Id, TextDay + DonateHtml, parseMode: ParseMode.Html, disableWebPagePreview: true);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(message.Chat.Id, "Вибачте, але ви не зареєструвались");
                                }
                            }
                    } else if (message.Text.Contains("/next_week"))
                    {
                        using (Context context = new Context())
                        {
                            if (context.Customers.ToList().Find(x => x.ChatId == message.Chat.Id).ChatType != null)
                            {
                                long? Cistid = context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistId;

                                DateTime mondayDateTime = TimeZoneInfo.ConvertTime(
                                    DateTime.Today.AddDays(DayOfWeek.Monday - DateTime.Today.DayOfWeek).AddDays(7),
                                    kyivTimeZone);

                                int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mondayDateTime,
                                    CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                                    CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
                                string weekText =
                                    $"Розклад {mondayDateTime.ToString("dd.MM")} - {mondayDateTime.AddDays(6).ToString("dd.MM")} ({weekOfYear}):\n";
                                for (int i = 0; i < 6; i++)
                                {
                                    DateTime startDateTime = mondayDateTime.AddDays(i);
                                    DateTime endDateTime = mondayDateTime.AddDays(i + 1);
                                    long startTime = (long)startDateTime.Subtract(new DateTime(1970, 1, 1))
                                        .TotalSeconds;
                                    long endTime = startTime + 86400;
                                    List<Event> scheduleEvent =
                                        EventParser.GetSchedule("group", Cistid, startTime, endTime);
                                    scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();
                                    string TextDay =
                                        $"{startDateTime.ToString("dd.MM.yyyy")} ({startDateTime.ToString("dddd", cultureInfo)}):\n";
                                    TextDay = cultureInfo.TextInfo.ToTitleCase(TextDay);
                                    if (scheduleEvent.Count == 0)
                                    {
                                        weekText += TextDay + "Пар нема. Відпочивайте!\n\n";
                                    }
                                    else
                                    {
                                        foreach (var Event in scheduleEvent)
                                        {
                                            DateTime Start = TimeZoneInfo.ConvertTime(
                                                TimeService.UnixTimeStampToDateTime(Event.StartTime), kyivTimeZone);
                                            DateTime End = TimeZoneInfo.ConvertTime(
                                                TimeService.UnixTimeStampToDateTime(Event.EndTime), kyivTimeZone);
                                            string Teacher;
                                            if (Event.Teachers.Count == 0)
                                            {
                                                Teacher = "Не визначено";
                                            }
                                            else
                                            {
                                                Teacher = Event.Teachers.First().ShortName;
                                            }

                                            TextDay +=
                                                $"{Start.ToString("HH:mm")} - {End.ToString("HH:mm")} | <b> {Event.Subject.Brief} - {Event.Type} </b> | {Teacher}\n";
                                        }

                                        weekText += TextDay + "\n";
                                    }
                                }

                                bot.SendTextMessageAsync(message.Chat.Id, weekText + DonateHtml,
                                    parseMode: ParseMode.Html, disableWebPagePreview: true);
                            }
                            else
                            {
                                bot.SendTextMessageAsync(message.Chat.Id, "Вибачте, але ви не зареєструвались");
                            }
                        }
                    } else if (message.Text.Contains("/notify"))
                    {
                        string filePath = "D:\\NureBot\\NureBot\\Admin_id";
                        var numbers = new List<long>();
                        using (var reader = new StreamReader(filePath))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                // Try to parse the line as an integer and add it to the list
                                if (long.TryParse(line, out long number))
                                {
                                    numbers.Add(number);
                                }
                            }
                        }

                        if (numbers.Contains(message.Chat.Id))
                        {
                            using (Context context = new Context())
                            {
                                string text = message.Text.Replace("/notify ", "");
                                List<long> chatIds = context.Customers.Select(c => c.ChatId).ToList();
                                foreach (long chatId in chatIds)
                                {
                                    bot.SendTextMessageAsync(chatId, text);
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        else
                        {
                            bot.SendTextMessageAsync(message.Chat.Id,
                                "Вибачте, але ви не маєте доступ до цієї команди");
                        }
                    } else if (message.Text.Contains("/statistics"))
                    {
                        string Path = "D:\\NureBot\\NureBot\\Admin_id";
                        var Admin_ids = new List<long>();
                        using (var reader = new StreamReader(Path))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                // Try to parse the line as an integer and add it to the list
                                if (long.TryParse(line, out long id))
                                {
                                    Admin_ids.Add(id);
                                }
                            }
                        }

                        if (Admin_ids.Contains(message.Chat.Id))
                        {
                            using (Context context = new Context())
                            {
                                long privateCount = context.Customers.Count(c => c.ChatType == "Private");
                                long groupCount = context.Customers.Count(c =>
                                    c.ChatType == "Group" || c.ChatType == "Supergroup");
                                long noneCount = context.Customers.Count(c => c.ChatType == null);
                                bot.SendTextMessageAsync(message.Chat.Id,
                                    $"Statistics:\nPrivate = {privateCount}\nGroup = {groupCount}\nNot Registered = {noneCount}");
                            }
                        }
                        else
                        {
                            bot.SendTextMessageAsync(message.Chat.Id,
                                "Вибачте, але ви не маєте доступ до цієї команди");
                        }
                    } else if (message.Text.Contains("/help"))
                    {
                        await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                        await bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Цей бот має низку команд, за допомогою яких ви можете отримати розклад для себе, " +
                            "і своєї групи. Нижче буде список цих команд, із коротким описом, і прикладом. \n \n" +
                            "Список команд бота: \n \n" +
                            "\t <code>/choose group</code> - зміна групи у чаті, замість group треба написати назву вашої групи. " +
                            "Наприклад: <code>/choose КІУКІ-22-7</code>, <code>/choose кіукі-22-7</code> і тд. Увага! Назву групи бот розуміє лише " +
                            "якщо та була введена українською, через те шо він звіряє назву із реєстром на сайті cist.nure.ua." +
                            " Якщо у вас виникла помилка зміни групи, перевірте щоб назва була українською мовою, " +
                            "і відповідала тій що на cist.nure.ua. \n" +
                            "\t <code>/help</code> - вам відправиться це повідомлення. \n" +
                            "\t <code>/day</code> - вам відправиться розклад для вашої групи на поточний день. \n" +
                            "\t <code>/week</code> - вам відправиться розклад для вашої групи на поточний тиждень.\n" +
                            "\t <code>/next_day</code> - відправляє розклад на наступний день. \n" +
                            "\t <code>/next_week</code> - відправить розклад на наступний тиждень. \n \n",
                            parseMode: ParseMode.Html);
                    }
                    else
                    {
                        if (command[0] == '/')
                        {
                            bot.SendTextMessageAsync(message.Chat.Id,
                                "Вибачте, ви написали команду не правильно, зверніться до команди /help");
                        }
                    }
                }
                if (message is not null && message.NewChatMembers is not null)
                {
                    NewMemberHandler.HandleNewMember(bot, message.NewChatMembers, message.Chat.Id);
                }
            }
        }   catch (Exception e)
        {
            Console.WriteLine($"Error: {e}");
        }
    }
}