using Newtonsoft.Json;
using NureBot.Classes;
using NureBot.Contexts;

namespace NureBot.Parsers;

public class GroupParser
{
    public static void Init()
    {
        List<Group> groups = JsonConvert.DeserializeObject<List<Group>>(Download());
        using (Context context = new Context())
        {
            context.Groups.AddRange(groups);
            context.SaveChanges();
        }
    }

    public static string? Download()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage Response = httpClient.GetAsync($"https://api.mindenit.org/lists/groups").Result;
                if (Response.IsSuccessStatusCode)
                {
                    return Response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
        return null;
    }
}