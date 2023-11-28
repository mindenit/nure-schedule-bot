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
                HttpResponseMessage Response = httpClient
                    .GetAsync(
                        $"https://api.mindenit.tech/schedule?type={type}&id={id}&start_time={startTime}&end_time={endTime}")
                    .Result;
                if (Response.IsSuccessStatusCode)
                {
                    List<Event> Schedule =
                        JsonConvert.DeserializeObject<List<Event>>(Response.Content.ReadAsStringAsync().Result);
                    return Schedule;
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
