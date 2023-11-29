using NureBot.Handlers;
using NureBot.Parsers;
using NureBot.Contexts;
using NureBot.Service;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;


if (!File.Exists("config-bot.toml"))
{
    EnviromentManager.Setup();
}

var context = new Context();
if (!context.Groups.Any())
{
    GroupParser.Init();
}

if (!context.Teachers.Any())
{
    TeacherParser.Init();
}

string botToken = EnviromentManager.ReadBotToken();
TelegramBotClient BotClient = new TelegramBotClient(botToken);

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = new UpdateType[]
    {
        UpdateType.Message,
        UpdateType.ChatMember,
        UpdateType.CallbackQuery
    }
};

while (true)
{
    try
    {
        BotClient.StartReceiving(
            updateHandler: (bot, update, cancellationToken) => UpdateHandler.HandleUpdateAsync(BotClient, update, cancellationToken),
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await BotClient.GetMeAsync();
        Console.WriteLine($"Start listening for @{me.Username}");

        // Замініть цю стрічку на ваш основний код бота.
        
        Console.ReadLine();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);

        await BotClient.SendTextMessageAsync(946530105, $"Error: <code>{e}</code>", parseMode: ParseMode.Html);

    }
    finally
    {
        // Відміна підключення бота
        cts.Cancel();
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    return Task.CompletedTask;
}
