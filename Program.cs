using System;
using NureBot.Classes;
using NureBot.Handlers;
using NureBot.Parsers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

GroupParser.Init();
TeacherParser.Init();
string botToken = File.ReadAllText("D:\\NureBot\\Nurebot\\Token");
TelegramBotClient BotClient = new TelegramBotClient(botToken);

using CancellationTokenSource cts = new ();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = new UpdateType[]
    {
        UpdateType.Message,
        UpdateType.ChatMember,
        UpdateType.CallbackQuery
    }
};

BotClient.StartReceiving(
    updateHandler: (bot, update, cancellationToken) => UpdateHandler.HandleUpdateAsync(BotClient, update, cancellationToken),
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);


var me = await BotClient.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

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