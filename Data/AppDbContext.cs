using Microsoft.EntityFrameworkCore;
using MyContacts.Models;

namespace MyContacts.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts { get; set; } = null!;
}
