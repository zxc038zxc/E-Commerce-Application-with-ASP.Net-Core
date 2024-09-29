using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repository
{
	public class ProductRepository : Repository<Product>, IProductRepository
	{
		public ApplicationDbContext _dbContext;

		public ProductRepository(ApplicationDbContext db) : base(db)
		{
			_dbContext = db;
		}
		public void Update(Product obj)
		{
			var objFromDb = _dbContext.Products.FirstOrDefault(x => x.Id == obj.Id);
			if (objFromDb != null)
			{
				obj.Title = objFromDb.Title;
				obj.Description = objFromDb.Description;
				obj.Price = objFromDb.Price;
				obj.Price50 = objFromDb.Price50;
				obj.Price100 = objFromDb.Price100;
				obj.ListPrice = objFromDb.ListPrice;
				obj.CategoryId = objFromDb.CategoryId;

				if (obj.ImageUrl != null)
				{
					objFromDb.ImageUrl = obj.ImageUrl;
				}
			}
		}
	}
}
