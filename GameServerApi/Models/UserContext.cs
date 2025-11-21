using Microsoft.EntityFrameworkCore;

namespace GameServerApi.Models;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=user.db");
    }

    public DbSet<User> Users { get; set; } = null!;
}