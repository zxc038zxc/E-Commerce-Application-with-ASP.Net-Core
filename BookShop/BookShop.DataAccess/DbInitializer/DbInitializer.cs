using BookShop.DataAccess.Data;
using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.DbInitializer
{
	public class DbInitializer : IDbInitializer
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _db;

		public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_db = db;
		}

		public void Initialize()
		{
			// Mirgration if not applied
			try
			{
				if(_db.Database.GetPendingMigrations().Count()>0)
				{
					_db.Database.Migrate();
				}
			}
			catch
			{

			}

			// Create Role if not create
			if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
			{
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

				// if Role create, create admin user
				_userManager.CreateAsync(new ApplicationUser
				{
					UserName = "admin@dotnet.com",
					Email = "admin@dotnet.com",
					Name = "GM Admin",
					PhoneNumber = "09123456789",
					StreetAddress = "Test 123 ",
					State = "TT",
					PostalCode = "123",
					City = "Taipei"
				}, "Admin123*").GetAwaiter().GetResult();

				var user = _db.ApplicationUser.FirstOrDefault(u => u.Email == "admin@dotnet.com");
				_userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
			}

			return;
		}
	}
}
