using Newtonsoft.Json;
using NureBot.Classes;
using NureBot.Contexts;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.UpdatingMessages;

namespace Nurebot;

public class dbService
{
    public static void dbActualizer(TelegramBotClient client)
    {
        var context = new Context();
        while (true)
        {
            DateTime NowUTC = DateTime.UtcNow;
            TimeZoneInfo kyivZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            DateTime NowKyiv = TimeZoneInfo.ConvertTimeFromUtc(NowUTC, kyivZone);
            if (NowKyiv.Hour == 0 && NowKyiv.Minute == 0 && NowKyiv.Second == 0)
            {
                var ToDelete = new List<Customer>();
                foreach (var customer in context.Customers)
                {
                    try
                    {
                        Thread.Sleep(300);
                        var message = client.SendMessage(customer.ChatId,
                            "Ми перевіряємо чи не заблокували ви нас. Все нормально.",
                            disableNotification: true);
                        Thread.Sleep(1000);
                        client.DeleteMessage(message.Chat.Id, message.MessageId);
                    }
                    catch (Exception e)
                    {
                        ToDelete.Add(customer);
                        Console.WriteLine(JsonConvert.SerializeObject(customer));
                    }
                }

                context.Customers.RemoveRange(ToDelete);
                context.SaveChanges();
            }
        }
    }
}