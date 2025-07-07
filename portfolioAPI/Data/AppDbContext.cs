
using Microsoft.EntityFrameworkCore;
using portfolioAPI.Models;

namespace portfolioAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ContactMessage> ContactMessages { get; set; }
    }
}