using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Newtonsoft.Json;
using Nurebot;
using NureBot.Classes;
using NureBot.Handlers;
using NureBot.Service;
using NureBot.Contexts;
using NureBot.Parsers;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.UpdatingMessages;

var client = new BotClient(EnviromentManager.ReadBotToken());

var updates = client.GetUpdates();

EnviromentManager.Setup();

var context = new Context();
if (!context.Groups.Any())
{
    GroupParser.Init();
}

if (!context.Teachers.Any())
{
    TeacherParser.Init();
}

Thread thread1 = new Thread(new ThreadStart(dbService.dbActualizer(client)));
Thread thread2 = new Thread(new ThreadStart(TimeService.LessonNotification(client)));
thread1.Start();
thread2.Start();

while (true)
{
    try
    {
        if (updates.Any())
        {
            foreach (var update in updates)
            {
                if (update.Message is not null)
                {
                    Console.WriteLine(update.Message.Chat.Id);
                    HandleUpdates.Handle(update, client);
                }
            }

            var offset = updates.Last().UpdateId + 1;
            updates = client.GetUpdates(offset);
        }
        else
        {
            updates = client.GetUpdates();
        }
    }
    catch (Exception e)
    {
        Thread.Sleep(1000);
        client.SendMessage("-1002108311720", $"Сталася помилка: \n \n {JsonConvert.SerializeObject(e)}");
    }
}