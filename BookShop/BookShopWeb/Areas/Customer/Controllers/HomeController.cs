using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new ShoppingCart
            {
                Product = _unitOfWork.ProductRepo.Get(u => u.Id == productId, "Category"),
                ProductId = productId,
                Count = 1,
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;
            
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepo.Get(u => u.ApplicationUserId == userId && u.ProductId == cart.ProductId);
            if(cartFromDb!=null)
            {
                cartFromDb.Count += cart.Count;

                // 下面這一行其實不用寫，因為EF在取得物件後會持續Track，所以依然會更動到值，但這樣就會造成有時候的麻煩
                _unitOfWork.ShoppingCartRepo.Update(cartFromDb);
            }
            else
			{
				_unitOfWork.ShoppingCartRepo.Add(cart);
			}

            TempData["success"] = "Cart updated successfully";

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
