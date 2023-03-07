using Unreliable_Service.Models;

namespace Unreliable_Service.Data;
using Microsoft.EntityFrameworkCore;

public class SampleDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite(@"Data Source=SampleDbContext.db;");
    }

    public DbSet<ProductContract> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductContract>().HasData(

            new ProductContract()
            {
                Id = Guid.NewGuid(), Description = "Toothpaste",
                Category = "Cosmetics",
                SubCategory = "Dental",
                Price = 4.56m,
                ItemCode = "T67650005"
            });
        modelBuilder.Entity<ProductContract>().HasData(

            new ProductContract()
            {
                Id = Guid.NewGuid(), Description = "Rice Oil",
                Category = "Oils and spices",
                SubCategory = "Oils",
                Price = 24.56m,
                ItemCode = "T6389764576"
            });
        modelBuilder.Entity<ProductContract>().HasData(

            new ProductContract()
            {
                Id = Guid.NewGuid(), Description = "Cruskits",
                Category = "Health",
                SubCategory = "Biscuits",
                Price = 5.65m,
                ItemCode = "T46567545"
            });
        modelBuilder.Entity<ProductContract>().HasData(

            new ProductContract()
            {
                Id = Guid.NewGuid(), Description = "Sponge",
                Category = "Homeware",
                SubCategory = "Washing",
                Price = 2.00m,
                ItemCode = "T4534333"
            });
        modelBuilder.Entity<ProductContract>().HasData(

            new ProductContract()
            {
                Id = Guid.NewGuid(), Description = "Chicken Breast - 1 KG",
                Category = "Meats",
                SubCategory = "Chicken",
                Price = 14.55m,
                ItemCode = "T4353455534"
            });
    }
}