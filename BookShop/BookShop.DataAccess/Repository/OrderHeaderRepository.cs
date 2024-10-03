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
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		public ApplicationDbContext _dbContext;

		public OrderHeaderRepository(ApplicationDbContext db) : base(db)
		{
			_dbContext = db;
		}
		public void Update(OrderHeader obj)
		{
			_dbContext.OrderHeaders.Update(obj);
		}
	}
}
