using NureBot.Classes;

namespace NureBot.Parsers;
using Newtonsoft.Json;

public class EventParser
{
    public static List<Event>? GetSchedule(string type, long? id, long startTime, long endTime)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                if (type == "group")
                {
                    HttpResponseMessage Response = httpClient
                        .GetAsync(
                            $"https://api.mindenit.org/schedule/groups/{id}?start={startTime}&end={endTime}")
                        .Result;
                    if (Response.IsSuccessStatusCode)
                    {
                        List<Event> Schedule = JsonConvert.DeserializeObject<List<Event>>(Response.Content.ReadAsStringAsync().Result);
                        return Schedule;
                    }
                }
                else if (type == "teacher")
                {
                    HttpResponseMessage Response = httpClient
                        .GetAsync(
                            $"https://api.mindenit.org/schedule/teachers/{id}?start={startTime}&end={endTime}")
                        .Result;
                    if (Response.IsSuccessStatusCode)
                    {
                        List<Event> Schedule = JsonConvert.DeserializeObject<List<Event>>(Response.Content.ReadAsStringAsync().Result);
                        return Schedule;
                    }
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
