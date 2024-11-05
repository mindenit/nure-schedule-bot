using Telegram.BotAPI;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.AvailableMethods;

namespace NureBot.Handlers
{
    public class NewMemberHandler
    {
        public static void HandleNewMember(TelegramBotClient bot, User[] users, long ChatId)
        {
            var me = bot.GetMe();
            foreach (var user in users)
            {
                if (user.Id == me.Id)
                {
                    bot.SendChatAction(ChatId, ChatActions.Typing);
                    bot.SendMessage(
                        ChatId,
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
                        parseMode: "HTML");
                }
            }
        }
    }
}