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
	public class CompanyRepository : Repository<Company>, ICompanyRepository
	{
		public ApplicationDbContext _dbContext;

		public CompanyRepository(ApplicationDbContext db) : base(db)
		{
			_dbContext = db;
		}
		public void Update(Company obj)
		{
			_dbContext.Companies.Update(obj);
		}
	}
}
