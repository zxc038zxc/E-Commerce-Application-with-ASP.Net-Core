using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShopWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public CompanyController(IUnitOfWork unit)
		{
			_unitOfWork = unit;
		}

		public IActionResult Index()
		{
			var repoDatas = _unitOfWork.CompanyRepo.GetAll().ToList();

			return View(repoDatas);
		}


		#region CreateBtn
		public IActionResult UpSert(int? id)
		{
			// 這是用於 ASP.NET MVC 中，用來表示Html <Select> 選項的一個類別
			//ViewBag.CategoryList = categoryList;
			//ViewData["CategoryList"] = categoryList;
			if (id == null || id == 0)
			{
				// Create
				return View(new Company());
			}
			else
			{
				// Update
				Company Company = _unitOfWork.CompanyRepo.Get(u => u.Id == id);
				return View(Company);
			}
		}

		[HttpPost]
		public IActionResult UpSert(Company company, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				if (company.Id == 0)
				{
					_unitOfWork.CompanyRepo.Add(company);
				}
				else
				{
					_unitOfWork.CompanyRepo.Update(company);
				}
				_unitOfWork.Save();
				TempData["success"] = "Company created successfully!";
				return RedirectToAction("Index");
			}
			else
			{
				return View(company);
			}
		}
		#endregion

		#region DeleteBtn
		//public IActionResult Delete(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}

		//	var productFormDbById = _unitOfWork.CompanyRepo.Get(u => u.Id == id);
		//	if (productFormDbById == null)
		//	{
		//		return NotFound();
		//	}
		//	return View(productFormDbById);
		//}

		//[HttpPost, ActionName("Delete")]
		//public IActionResult DeletePost(int? id)
		//{
		//	var obj = _unitOfWork.CompanyRepo.Get(u => u.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}

		//	_unitOfWork.CompanyRepo.Remove(obj);
		//	_unitOfWork.Save();
		//	TempData["success"] = "Company deleted successfully!";
		//	return RedirectToAction("Index");
		//}
		#endregion

		#region APICalls
		// HttpGet是一種屬性，當使用者偷過GET方法請求這個動作方法時，伺服器才會執行該方法
		// 這樣的GetAll方法允許前端使用AJAX請求來獲取產品清單，而不需要重整整個網頁
		[HttpGet]
		public IActionResult GetAll()
		{
			var repoDatas = _unitOfWork.CompanyRepo.GetAll().ToList();
			return Json(new { data = repoDatas });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productToBeDeleted = _unitOfWork.CompanyRepo.Get(u => u.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			_unitOfWork.CompanyRepo.Remove(productToBeDeleted);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Delete Successful" });
		}
		#endregion
	}
}
