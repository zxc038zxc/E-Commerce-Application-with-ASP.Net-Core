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
		public IProductRepository ProductRepo { get; private set; }
		public ICompanyRepository CompanyRepo { get; private set; }
		public IShoppingCartRepository ShoppingCartRepo { get; private set; }
		public IApplicationUserRepository ApplicationUserRepo { get; private set; }

        public IOrderHeaderRepository OrderHeaderRepo { get; private set; }

        public IOrderDetailRepository OrderDetailRepo { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
		{
			_dbContext = db;
			CategoryRepo = new CategoryRepository(_dbContext);
			ProductRepo = new ProductRepository(_dbContext);
			CompanyRepo = new CompanyRepository(_dbContext);
			ShoppingCartRepo = new ShoppingCartRepository(_dbContext);
			ApplicationUserRepo = new ApplicationUserRepository(_dbContext);
            OrderHeaderRepo= new OrderHeaderRepository(_dbContext);
			OrderDetailRepo= new OrderDetailRepository(_dbContext);
		}

		public void Save()
		{
			_dbContext.SaveChanges();
		}
	}
}
