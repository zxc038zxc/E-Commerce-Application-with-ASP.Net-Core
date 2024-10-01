using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repository.IRepository
{
	public interface IUnitOfWork
	{
		ICategoryRepository CategoryRepo { get; }
		IProductRepository ProductRepo { get; }
		ICompanyRepository CompanyRepo { get; }
		void Save();
	}
}
