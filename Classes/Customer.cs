using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NureBot.Classes;

public class Customer
{
    [Key]
    public long ChatId { get; set; }
    public string? CistName { get; set; }
    public long? CistId { get; set; }
    public string? ChatType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
}