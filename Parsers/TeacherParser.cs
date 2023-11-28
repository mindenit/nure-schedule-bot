using Newtonsoft.Json;
using NureBot.Classes;
using NureBot.Contexts;

namespace NureBot.Parsers;

public class TeacherParser
{
    public static void Init()
    {
        List<Teacher> teachers = JsonConvert.DeserializeObject<List<Teacher>>(Download());
        using (Context context = new Context())
        {
            context.Teachers.AddRange(teachers);
            context.SaveChanges();
        }
    }
    
    public static string? Download()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage Response = httpClient.GetAsync($"https://api.mindenit.tech/teachers").Result;
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