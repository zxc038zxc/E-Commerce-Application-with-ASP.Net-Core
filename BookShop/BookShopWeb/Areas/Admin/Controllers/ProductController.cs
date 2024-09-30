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
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductController(IUnitOfWork unit, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unit;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			var repoDatas = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category").ToList();

			return View(repoDatas);
		}


		#region CreateBtn
		public IActionResult UpSert(int? id)
		{
			// 這是用於 ASP.NET MVC 中，用來表示Html <Select> 選項的一個類別
			//ViewBag.CategoryList = categoryList;
			//ViewData["CategoryList"] = categoryList;
			ProductViewModel vm = new ProductViewModel
			{
				CategoryList = _unitOfWork.CategoryRepo.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				Product = new Product(),
			};
			if (id == null || id == 0)
			{
				// Create
				return View(vm);
			}
			else
			{
				// Update
				vm.Product = _unitOfWork.ProductRepo.Get(u => u.Id == id);
				return View(vm);
			}
		}

		[HttpPost]
		public IActionResult UpSert(ProductViewModel productVM, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				/// WebHostEnvironment 用來取得伺服器的環境資訊。
				/// WebRootPath 表示應用程式的根目錄，也就是wwwroot文件夾的絕對路徑。
				/// 所有靜態檔案(圖片、Css、Javascript等)存放位置
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					// Path.GetExtension 會取得檔案的副檔名
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\product", fileName);

					if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
					{
						// 刪除舊圖
						var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}
					// 將檔案保存到伺服器(FileStream)
					using (var fileStream = new FileStream(productPath, FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					productVM.Product.ImageUrl = @"\images\product\" + fileName;
				}

				if (productVM.Product.Id == 0)
				{
					_unitOfWork.ProductRepo.Add(productVM.Product);
				}
				else
				{
					_unitOfWork.ProductRepo.Update(productVM.Product);
				}
				_unitOfWork.Save();
				TempData["success"] = "Product created successfully!";
				return RedirectToAction("Index");
			}
			else
			{
				productVM = new ProductViewModel
				{
					CategoryList = _unitOfWork.CategoryRepo.GetAll().Select(u => new SelectListItem
					{
						Text = u.Name,
						Value = u.Id.ToString()
					}),
				};
				return View(productVM);
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

		//	var productFormDbById = _unitOfWork.ProductRepo.Get(u => u.Id == id);
		//	if (productFormDbById == null)
		//	{
		//		return NotFound();
		//	}
		//	return View(productFormDbById);
		//}

		//[HttpPost, ActionName("Delete")]
		//public IActionResult DeletePost(int? id)
		//{
		//	var obj = _unitOfWork.ProductRepo.Get(u => u.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}

		//	_unitOfWork.ProductRepo.Remove(obj);
		//	_unitOfWork.Save();
		//	TempData["success"] = "Product deleted successfully!";
		//	return RedirectToAction("Index");
		//}
		#endregion

		#region APICalls
		// HttpGet是一種屬性，當使用者偷過GET方法請求這個動作方法時，伺服器才會執行該方法
		// 這樣的GetAll方法允許前端使用AJAX請求來獲取產品清單，而不需要重整整個網頁
		[HttpGet]
		public IActionResult GetAll()
		{
			var repoDatas = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = repoDatas });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productToBeDeleted = _unitOfWork.ProductRepo.Get(u => u.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			// 刪除舊圖
			var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}

			_unitOfWork.ProductRepo.Remove(productToBeDeleted);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Delete Successful" });
		}
		#endregion
	}
}
