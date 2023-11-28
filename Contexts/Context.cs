using Microsoft.EntityFrameworkCore;
using NureBot.Classes;

namespace NureBot.Contexts;

public class Context: DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<Teacher> Teachers {get; set;}
    public DbSet<Customer> Customers { get; set; }
    public Context() => Database.EnsureCreated();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            File.ReadAllText("D:\\NureBot\\NureBot\\dbConnection"));
    }
}