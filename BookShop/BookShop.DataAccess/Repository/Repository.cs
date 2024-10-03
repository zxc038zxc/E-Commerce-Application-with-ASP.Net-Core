using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookShop.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _dbContext;
		internal DbSet<T> _dbSet;

		public Repository(ApplicationDbContext db)
		{
			_dbContext = db;
			_dbSet = db.Set<T>();
		}
		public void Add(T entity)
		{
			_dbSet.Add(entity);
		}

		public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
		{
			IQueryable<T> query = tracked ? _dbSet : _dbSet.AsNoTracking();
			query = query.Where(filter);

			if (!string.IsNullOrEmpty(includeProperties))
			{
				var properties = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries);
				foreach (var property in properties)
				{
					query = query.Include(property);
				}
			}
			return query.FirstOrDefault();
		}

		// Include的用法是EF的功能，類似於SQL的Inner Join
		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
		{
			IQueryable<T> query = _dbSet;

			if (filter != null)
			{
				query = query.Where(filter);
			}
			if (!string.IsNullOrEmpty(includeProperties))
			{
				var properties = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries);
				foreach (var property in properties)
				{
					query = query.Include(property);
				}
			}
			return query.ToList();
		}

		public void Remove(T entity)
		{
			_dbSet.Remove(entity);
		}

		public void DeleteRange(IEnumerable<T> entitys)
		{
			_dbSet.RemoveRange(entitys);
		}
	}
}
