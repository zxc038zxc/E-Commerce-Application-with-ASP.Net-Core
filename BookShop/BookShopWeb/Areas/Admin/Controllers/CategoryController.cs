using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unit)
        {
            _unitOfWork = unit;
        }

        public IActionResult Index()
        {
            var objCategoryList = _unitOfWork.CategoryRepo.GetAll();
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
                _unitOfWork.CategoryRepo.Add(obj);
                _unitOfWork.Save();
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
            var categoryFormDbById = _unitOfWork.CategoryRepo.Get(u => u.Id == id);

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
                _unitOfWork.CategoryRepo.Update(obj);
                _unitOfWork.Save();
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

            var categoryFormDbById = _unitOfWork.CategoryRepo.Get(u => u.Id == id);
            if (categoryFormDbById == null)
            {
                return NotFound();
            }
            return View(categoryFormDbById);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.CategoryRepo.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.CategoryRepo.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
        #endregion
    }
}
