using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.WaterMark.Models;

namespace RabbitMQWeb.WaterMark.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
    }

    public DbSet<Product> Products  { get; set; }
}