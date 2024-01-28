namespace NureBot.Classes;

public class Event
{
    public int NumberPair { get; set; }
    public Subject? Subject { get; set; }
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public string? Type { get; set; }
    public List<Teacher>? Teachers { get; set; }
}

public class Subject
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public string? Brief { get; set; }
}