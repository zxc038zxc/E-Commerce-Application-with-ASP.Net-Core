using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		public ApplicationDbContext _dbContext;
		public ICategoryRepository CategoryRepo { get; private set; }

		public UnitOfWork(ApplicationDbContext db)
		{
			_dbContext = db;
			CategoryRepo = new CategoryRepository(_dbContext);
		}

		public void Save()
		{
			_dbContext.SaveChanges();
		}
	}
}
