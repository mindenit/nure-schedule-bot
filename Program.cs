using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Newtonsoft.Json;
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

Thread dbActualizer = new Thread(() =>
{
    while (true)
    {
        if(DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
        {
            var ToDelete = new List<Customer>();
            foreach (var customer in context.Customers)
            {
                try
                {
                    var message = client.SendMessage(customer.ChatId, "This chat is registered?",
                        disableNotification: true);
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
});

dbActualizer.Start();

while (true)
{
    try
    {
        if (updates.Any())
        {
            foreach (var update in updates)
            {
                Console.WriteLine(update.Message.Chat.Id);
                HandleUpdates.Handle(update, client);
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
        client.SendMessage("-1002108311720", "Сталася помилка: \n \n <code>" + JsonConvert.SerializeObject(e)+"</code>", parseMode: "HTML");
    }
}