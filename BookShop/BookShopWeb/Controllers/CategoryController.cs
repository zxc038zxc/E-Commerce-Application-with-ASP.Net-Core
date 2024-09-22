using BookShopWeb.Data;
using BookShopWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Controllers
{
	public class CategoryController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		public CategoryController(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public IActionResult Index()
		{
			var objCategoryList = _dbContext.Categories.ToList();
			return View(objCategoryList);
		}


		#region CreateBtn
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Category obj)
		{
			//if(obj.Name == obj.DisplayOrder.ToString())
			//{
			//	ModelState.AddModelError("Name", "The Display order can't exactly match the Name");
			//}
			if (ModelState.IsValid)
			{
				_dbContext.Categories.Add(obj);
				_dbContext.SaveChanges();
				TempData["success"] = "Category created successfully!";
				return RedirectToAction("Index");
			}
			return View();
		}
		#endregion

		#region EditBtn
		public IActionResult Edit(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}

			// 這邊有三種寫法
			//var categoryFormDbById = _dbContext.Categories.Find(id);
			//var categoryFormDbById = _dbContext.Categories.Where(u=>u.Id==id).FirstOrDefault();
			var categoryFormDbById = _dbContext.Categories.FirstOrDefault(u => u.Id == id);

			if (categoryFormDbById == null)
			{
				return NotFound();
			}
			return View(categoryFormDbById);
		}

		[HttpPost]
		public IActionResult Edit(Category obj)
		{
			if (ModelState.IsValid)
			{
				_dbContext.Categories.Update(obj);
				_dbContext.SaveChanges();
				TempData["success"] = "Category updated successfully!";
				return RedirectToAction("Index");
			}
			return View();
		}
		#endregion

		#region DeleteBtn
		public IActionResult Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}

			var categoryFormDbById = _dbContext.Categories.FirstOrDefault(u => u.Id == id);
			if (categoryFormDbById == null)
			{
				return NotFound();
			}
			return View(categoryFormDbById);
		}

		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePost(int? id)
		{
			var obj = _dbContext.Categories.FirstOrDefault(u => u.Id == id);
			if (obj == null)
			{
				return NotFound();
			}

			_dbContext.Categories.Remove(obj);
			_dbContext.SaveChanges();
			TempData["success"] = "Category deleted successfully!";
			return RedirectToAction("Index");
		}
		#endregion
	}
}
