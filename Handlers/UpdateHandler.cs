using System.Globalization;
using System.Reflection.Metadata;
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
        string DonateHtml = "\n" + "<a href=\"https://t.me/mindenit\">Канал з інфою</a> | " +
                            "<a href=\"https://send.monobank.ua/jar/5tHDuV8dfg\">Підтримати розробку</a> | " +
                            "<a href=\"https://t.me/ketronix_dev\">Адмін</a> | " +
                            "<a href=\"https://github.com/mindenit/nure-schedule-bot\">Код</a>" + "\n";
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
            Console.WriteLine(update);
            if (update.Type == UpdateType.Message)
            {
                if (message is not null && message.Text is not null)
                {
                    string[] words = message.Text.Split();
                    if (message.Text.Contains("/choose"))
                    {
                        if (words.Length != 1)
                        {
                            try
                            {
                                using (Context context = new Context())
                                {
                                    var existingCustomer = context.Customers.Find(message.Chat.Id);
                                    if (words.Length == 2)
                                    {
                                        if (existingCustomer.ChatType == null)
                                        {
                                            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                                            existingCustomer.CistName = context.Groups.ToList()
                                                .Find(x => x.Name.ToUpper() == words[1].ToUpper()).Name;
                                            existingCustomer.CistId = context.Groups.ToList().Find(x =>
                                                x.Name.ToUpper() == existingCustomer.CistName.ToUpper()).Id;
                                            existingCustomer.ChatType = message.Chat.Type.ToString();
                                            bot.SendTextMessageAsync(message.Chat.Id, $"Дякую за реєстрацію!");
                                        }
                                        else
                                        {
                                            existingCustomer.CistName = context.Groups.ToList()
                                                .Find(x => x.Name.ToUpper() == words[1].ToUpper()).Name;
                                            existingCustomer.CistId = context.Groups.ToList()
                                                .Find(x => x.Name.ToUpper() == existingCustomer.CistName.ToUpper()).Id;
                                            bot.SendTextMessageAsync(message.Chat.Id, $"Дякую за реєстрацію!");
                                        }
                                    } else if (words.Length == 4)
                                    {
                                        string SNM = words[1] + " " + words[2][0] + ". " + words[3][0] + ".";
                                        if (existingCustomer.ChatType == null)
                                        {
                                            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                                            existingCustomer.CistName = context.Teachers.ToList()
                                                .Find(x => x.ShortName.ToUpper() == SNM.ToUpper()).ShortName;
                                            existingCustomer.CistId = context.Teachers.ToList().Find(x =>
                                                x.ShortName.ToUpper() == existingCustomer.CistName.ToUpper()).Id;
                                            existingCustomer.ChatType = message.Chat.Type.ToString();
                                            bot.SendTextMessageAsync(message.Chat.Id, $"Дякую за реєстрацію!");
                                        }
                                        else
                                        {
                                            existingCustomer.CistName = context.Teachers.ToList()
                                                .Find(x => x.ShortName.ToUpper() == SNM.ToUpper()).ShortName;
                                            existingCustomer.CistId = context.Teachers.ToList()
                                                .Find(x => x.ShortName.ToUpper() == existingCustomer.CistName.ToUpper()).Id;
                                            bot.SendTextMessageAsync(message.Chat.Id, $"Дякую за реєстрацію!");
                                        }
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(message.Chat.Id, $"Вибачте, але назва вказана неправильно.");
                                    }
                                    context.SaveChanges();
                                }
                            }
                        catch (Exception e)
                        {
                            bot.SendTextMessageAsync(message.Chat.Id, $"Вибачте, але назва вказана неправильно.");
                            Console.WriteLine($"Error: {e}");

                        }
                        }
                        else
                        {
                            bot.SendTextMessageAsync(message.Chat.Id, $"Вибачте, але нічого не вказано");
                        }
                    }
                    else if (message.Text.Contains("/day"))
                    {
                        using (Context context = new Context())
                            {
                                if (context.Customers.ToList().Find(x => x.ChatId == message.Chat.Id).ChatType != null)
                                {
                                    string CistName =  context.Customers.ToList()
                                        .Find(x => x.ChatId == message.Chat.Id).CistName;
                                    string Type;
                                    if (CistName.Split().Length == 1)
                                    {
                                        Type = "group";
                                    }
                                    else
                                    {
                                        Type = "teacher";
                                    }
                                    DateTime startDateTime = TimeZoneInfo.ConvertTime(DateTime.Now.Date, kyivTimeZone);
                                    long? Cistid = context.Customers.ToList()
                                        .Find(x => x.ChatId == message.Chat.Id).CistId;
                                    long startTime =
                                        (long)startDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                    long endTime = startTime + 86400;
                                    List<Event> scheduleEvent =
                                        EventParser.GetSchedule(Type, Cistid, startTime, endTime);
                                    string TextDay = $"Розклад на {startDateTime.ToString("dd.MM.yyyy")}:\n";
                                    if ((scheduleEvent.Count == 0) || (scheduleEvent == null))
                                    {
                                        TextDay += "Пар нема. Відпочивайте!\n";
                                    }
                                    else
                                    {
                                        scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();
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
                                string CistName =  context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistName;
                                string Type;
                                if (CistName.Split().Length == 1)
                                {
                                    Type = "group";
                                }
                                else
                                {
                                    Type = "teacher";
                                }
                                if (context.Customers.ToList().Find(x => x.ChatId == message.Chat.Id).ChatType != null)
                                {
                                long? Cistid = context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistId;
                                int dayValue = ((int)DateTime.Today.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek;
                                DateTime mondayDateTime = TimeZoneInfo.ConvertTime(DateTime.Today.AddDays((int)DayOfWeek.Monday - dayValue),
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
                                        EventParser.GetSchedule(Type, Cistid, startTime, endTime);
                                    string TextDay = $"{startDateTime.ToString("dd.MM.yyyy")} ({startDateTime.ToString("dddd", cultureInfo)}):\n";
                                    TextDay = cultureInfo.TextInfo.ToTitleCase(TextDay);
                                    if ((scheduleEvent.Count == 0) || (scheduleEvent == null))
                                    {
                                        weekText += TextDay + "Пар нема. Відпочивайте!\n\n";
                                    }
                                    else
                                    {
                                        scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();
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
                                    string CistName =  context.Customers.ToList()
                                        .Find(x => x.ChatId == message.Chat.Id).CistName;
                                    string Type;
                                    if (CistName.Split().Length == 1)
                                    {
                                        Type = "group";
                                    }
                                    else
                                    {
                                        Type = "teacher";
                                    }
                                    DateTime startDateTime = TimeZoneInfo.ConvertTime(DateTime.Today.AddDays(1),
                                        kyivTimeZone);
                                    long? Cistid = context.Customers.ToList()
                                        .Find(x => x.ChatId == message.Chat.Id).CistId;
                                    long startTime = (long)startDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                    long endTime = startTime + 86400;
                                    List<Event> scheduleEvent = EventParser.GetSchedule(Type, Cistid, startTime, endTime);
                                    string TextDay = $"Розклад на {startDateTime.ToString("dd.MM.yyyy")}:\n";
                                    if ((scheduleEvent.Count == 0) || (scheduleEvent == null))
                                    {
                                        TextDay += "Пар нема. Відпочивайте!\n";
                                    }
                                    else
                                    {
                                        scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();
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
                                string CistName =  context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistName;
                                string Type;
                                if (CistName.Split().Length == 1)
                                {
                                    Type = "group";
                                }
                                else
                                {
                                    Type = "teacher";
                                }
                                long? Cistid = context.Customers.ToList()
                                    .Find(x => x.ChatId == message.Chat.Id).CistId;
                                int dayValue = ((int)DateTime.Today.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek;
                                DateTime mondayDateTime = TimeZoneInfo.ConvertTime(
                                    DateTime.Today.AddDays((int)DayOfWeek.Monday - dayValue).AddDays(7),
                                    kyivTimeZone);

                                int weekOfYear = cultureInfo.Calendar.GetWeekOfYear(mondayDateTime,
                                    cultureInfo.DateTimeFormat.CalendarWeekRule,
                                    cultureInfo.DateTimeFormat.FirstDayOfWeek);
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
                                        EventParser.GetSchedule(Type, Cistid, startTime, endTime);
                                    string TextDay =
                                        $"{startDateTime.ToString("dd.MM.yyyy")} ({startDateTime.ToString("dddd", cultureInfo)}):\n";
                                    TextDay = cultureInfo.TextInfo.ToTitleCase(TextDay);
                                    if ((scheduleEvent.Count == 0) || (scheduleEvent == null))
                                    {
                                        weekText += TextDay + "Пар нема. Відпочивайте!\n\n";
                                    }
                                    else
                                    {
                                        scheduleEvent = scheduleEvent.OrderBy(c => c.StartTime).ToList();
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
                        string inputString = EnviromentManager.ReadAdmins(); 
                        var numbers = new List<long>();
                        
                        string[] numberStrings = inputString.Split(',');
                        foreach (var numberString in numberStrings)
                        {
                            if (long.TryParse(numberString, out long number))
                            {
                                numbers.Add(number);
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
                                    try
                                    {
                                        bot.SendTextMessageAsync(chatId, text);
                                        Thread.Sleep(100);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        continue;
                                    }
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
                        string inputString = EnviromentManager.ReadAdmins(); 
                        var numbers = new List<long>();
                        
                        string[] numberStrings = inputString.Split(',');
                        foreach (var numberString in numberStrings)
                        {
                            if (long.TryParse(numberString, out long number))
                            {
                                numbers.Add(number);
                            }
                        }

                        if (numbers.Contains(message.Chat.Id))
                        {
                            using (Context context = new Context())
                            {
                                long privateCount = context.Customers.Count(c => c.ChatType == "Private");
                                long groupCount = context.Customers.Count(c =>
                                    c.ChatType == "Group" || c.ChatType == "Supergroup");
                                long noneCount = context.Customers.Count(c => c.ChatType == null);
                                long teachersCount = context.Customers
                                    .AsEnumerable()
                                    .Count(e => e.CistName != null && e.CistName.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length == 3);
                                bot.SendTextMessageAsync(message.Chat.Id,
                                    $"Statistics:\nPrivate = {privateCount}\nGroup = {groupCount}\nNot Registered = {noneCount}\nTeachers = {teachersCount}");
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
                            "\t <code>/choose group</code> - зміна групи у чаті, замість group треба написати назву вашої групи або ПІБ викладача. " +
                            "Наприклад: <code>/choose КІУКІ-22-7</code>, <code>/choose кіукі-22-7</code>, <code>/choose Кулак Е. М.</code>, <code>/choose Кулак Ельвіра Миколаївна</code> і тд.\n" +
                            "Увага! Назву бот розуміє лише якщо та була введена українською, через те шо він звіряє назву із реєстром на сайті cist.nure.ua." +
                            " Якщо у вас виникла помилка зміни групи, перевірте щоб назва була українською мовою, і відповідала тій що на cist.nure.ua." +
                            " Також після назви не пишіть нічого, інакше бот не зрозуміє.\n" +
                            "\t <code>/help</code> - вам відправиться це повідомлення. \n" +
                            "\t <code>/day</code> - вам відправиться розклад для вашої групи на поточний день. \n" +
                            "\t <code>/week</code> - вам відправиться розклад для вашої групи на поточний тиждень.\n" +
                            "\t <code>/next_day</code> - відправляє розклад на наступний день. \n" +
                            "\t <code>/next_week</code> - відправить розклад на наступний тиждень. \n \n",
                            parseMode: ParseMode.Html);
                    } else if (message.Text.Contains("/info"))
                    {
                        using (Context context = new Context())
                        {
                            var chat = context.Customers.Find(message.Chat.Id);
                            if (chat != null)
                            {
                                bot.SendTextMessageAsync(message.Chat.Id, 
                                $"ChatId: {chat.ChatId}\nCistId: {chat.CistId}\nCistName: {chat.CistName}\nChatType: {chat.ChatType}\nFirstname: {chat.FirstName}\nLastName: {chat.LastName}\nUsername: {chat.Username}");
                            }
                            else
                            {
                                bot.SendTextMessageAsync(message.Chat.Id, $"No chat with ChatId {message.Chat.Id} was found.");
                            }
                        }
                    }
                    else
                    {
                        if (words[0][0] == '/')
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
