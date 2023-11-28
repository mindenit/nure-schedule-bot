using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureBot.Handlers
{
    public class NewMemberHandler
    {
        public static async Task HandleNewMember(ITelegramBotClient bot, User[] users, long ChatId)
        {
            var me = await bot.GetMeAsync();
            foreach (var user in users)
            {
                if (user.Id == me.Id)
                {
                    bot.SendChatActionAsync(ChatId, ChatAction.Typing);
                    bot.SendTextMessageAsync(
                        ChatId,
                        "Цей бот має низку команд, за допомогою яких ви можете отримати розклад для себе, " +
                        "і своєї групи. Нижче буде список цих команд, із коротким описом, і прикладом. \n \n" +
                        "Список команд бота: \n \n" +
                        "\t <code>/choose group</code> - зміна групи у чаті, замість group треба написати назву вашої групи. " +
                        "Наприклад: <code>/choose КІУКІ-22-7</code>, <code>/choose кіукі-22-7</code> і тд.\nУвага! Назву групи бот розуміє лише " +
                        "якщо та була введена українською, через те шо він звіряє назву із реєстром на сайті cist.nure.ua." +
                        " Якщо у вас виникла помилка зміни групи, перевірте щоб назва була українською мовою, " +
                        "і відповідала тій що на cist.nure.ua. \n" +
                        "\t <code>/help</code> - вам відправиться це повідомлення. \n" +
                        "\t <code>/day</code> -  вам відправиться розклад для вашої групи на поточний день. \n" +
                        "\t <code>/week</code> - вам відправиться розклад для вашої групи на поточний тиждень. " +
                        "У неділю ця команда вам відправить розклад вже на наступний тиждень. \n" +
                        "\t <code>/next_day</code> - відправляє розклад на наступний день. \n" +
                        "\t <code>/next_week</code> - відправить розклад на наступний тиждень. \n \n",
                        parseMode: ParseMode.Html);
                }
            }
        }
    }
}