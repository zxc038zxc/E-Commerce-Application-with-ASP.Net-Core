using BookShop.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShop.DataAccess.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>().HasData(
				new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
				new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
				new Category { Id = 3, Name = "History", DisplayOrder = 3 }
				);
			modelBuilder.Entity<Product>().HasData(
				new Product
				{
					Id = 1,
					Title = "A 書",
					Author = "小明",
					Description = "小明寫的 ",
					ListPrice = 99,
					Price = 90,
					Price50 = 85,
					Price100 = 80,
					CategoryId = 1,
					ImageUrl = ""
				},
				new Product
				{
					Id = 2,
					Title = "B 書",
					Author = "小白",
					Description = "小白寫的 ",
					ListPrice = 40,
					Price = 30,
					Price50 = 25,
					Price100 = 20,
					CategoryId = 2,
					ImageUrl = ""
				},
				new Product
				{
					Id = 3,
					Title = "C 書",
					Author = "小紅",
					Description = "小紅寫的 ",
					ListPrice = 55,
					Price = 50,
					Price50 = 40,
					Price100 = 35,
					CategoryId = 3,
					ImageUrl = ""
				},
				new Product
				{
					Id = 4,
					Title = "D 書",
					Author = "小成",
					Description = "小成寫的 ",
					ListPrice = 70,
					Price = 65,
					Price50 = 60,
					Price100 = 55,
					CategoryId = 1,
					ImageUrl = ""
				},
				new Product
				{
					Id = 5,
					Title = "E 書",
					Author = "小劉",
					Description = "小劉寫的 ",
					ListPrice = 30,
					Price = 27,
					Price50 = 25,
					Price100 = 20,
					CategoryId = 2,
					ImageUrl = ""
				},
				new Product
				{
					Id = 6,
					Title = "F 書",
					Author = "小郭",
					Description = "小郭寫的 ",
					ListPrice = 25,
					Price = 23,
					Price50 = 22,
					Price100 = 20,
					CategoryId = 3,
					ImageUrl = ""
				});
		}
	}
}
