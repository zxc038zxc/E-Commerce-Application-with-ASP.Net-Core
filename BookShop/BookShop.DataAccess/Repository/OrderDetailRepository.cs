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
	public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
		public ApplicationDbContext _dbContext;

		public OrderDetailRepository(ApplicationDbContext db) : base(db)
		{
			_dbContext = db;
		}
		public void Update(OrderDetail obj)
		{
			_dbContext.OrderDetails.Update(obj);
		}
	}
}
