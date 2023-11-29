using System.Text;
using Tomlyn;

namespace NureBot.Service
{
    public class EnviromentManager
    {
        public static void Setup()
        {
            while (true)
            {
                Console.WriteLine(
                    "A token is necessary for the work of the bot. To get the token, you need to create a bot here: @BotFather\nThe mysql database is also required for work.");
                Console.Write("Please enter the token from the bot: ");
                string? tokenBot = Console.ReadLine();
                
                Console.Write("Postgres connection string: ");
                string? dbString = Console.ReadLine();
                
                Console.Write("Enter id of 1 admin: ");
                string? admin1 = Console.ReadLine();
                
                Console.Write("Enter id of 2 admin: ");
                string? admin2 = Console.ReadLine();

                if (tokenBot != null && dbString != null && admin1 != null && admin2 != null)
                {
                    using (FileStream fstream = new FileStream("config-bot.toml", FileMode.OpenOrCreate))
                    {
                        string configText =
                            String.Format(
                                "botToken = '{0}'\ndbString = '{1}'\nadmin1 = '{2}'\nadmin2 = '{3}'",
                                tokenBot,
                                dbString,
                                admin1,
                                admin2);
                        Console.WriteLine(configText);
                        byte[] buffer = Encoding.Default.GetBytes(configText);
                        fstream.Write(buffer, 0, buffer.Length);
                        break;
                    }
                }
            }
        }
        public static string ReadBotToken()
        {
            string token;
            using (FileStream fstream = File.OpenRead("config-bot.toml"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                string textFromFile = Encoding.Default.GetString(buffer);

                var model = Toml.ToModel(textFromFile);
                token = (string)model["botToken"];
            }
            return token;
        }
        public static string ReadAdmins()
        {
            string key1, key2;
            using (FileStream fstream = File.OpenRead("config-bot.toml"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                string textFromFile = Encoding.Default.GetString(buffer);

                var model = Toml.ToModel(textFromFile);
                key1 = (string)model["admin1"];
                key2 = (string)model["admin2"];
            }
            return key1 + "," + key2;
        }
        public static string ReadDbString()
        {
            string key;
            using (FileStream fstream = File.OpenRead("config-bot.toml"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                string textFromFile = Encoding.Default.GetString(buffer);

                var model = Toml.ToModel(textFromFile);
                key = (string)model["dbString"];
            }
            return key;
        }
    }
}