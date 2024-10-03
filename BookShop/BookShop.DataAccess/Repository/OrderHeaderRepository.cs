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

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus)
		{
			var orderFormDb = _dbContext.OrderHeaders.FirstOrDefault(u=>u.Id== id);
			if (orderFormDb != null)
			{
				orderFormDb.OrderStatus = orderStatus;
				if (!string.IsNullOrEmpty(paymentStatus))
				{
					orderFormDb.PaymentStatus = paymentStatus;
				}
			}
		}

		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderFormDb = _dbContext.OrderHeaders.FirstOrDefault(u => u.Id == id);
			if (!string.IsNullOrEmpty(sessionId))
			{
				orderFormDb.SessionId = sessionId;
			}
			if (!string.IsNullOrEmpty(paymentIntentId))
			{
				orderFormDb.PaymentIntentId= paymentIntentId;
				orderFormDb.PaymentDate = DateTime.Now;
			}
		}
	}
}
