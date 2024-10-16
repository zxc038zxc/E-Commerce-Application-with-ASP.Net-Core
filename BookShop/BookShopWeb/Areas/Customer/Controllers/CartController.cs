using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookShopWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]     // 只有登入的用戶可以訪問
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }

		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new ShoppingCartVM
			{
				ShoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()
			};

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
			return View(ShoppingCartVM);
		}

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new ShoppingCartVM
			{
				ShoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepo.Get(u => u.Id == userId);
			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
			ApplicationUser user = _unitOfWork.ApplicationUserRepo.Get(u => u.Id == userId);

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}

			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				// 一般帳號
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}

			_unitOfWork.OrderHeaderRepo.Update(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				var orderDetial = new OrderDetail
				{
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					ProductId = cart.ProductId,
					Price = cart.Price,
					Count = cart.Count,
				};
				_unitOfWork.OrderDetailRepo.Add(orderDetial);
				_unitOfWork.Save();
			}

			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				// 一般帳號
				var domain = "https://localhost:7063/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + "customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in ShoppingCartVM.ShoppingCartList)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),  // $20.50 => 2050
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title,
							}
						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);
				}

				var service = new SessionService();
				Session session = service.Create(options);

				if (session == null || string.IsNullOrEmpty(session.Id))
				{
					throw new Exception("Failed to create Stripe session.");
				}

				_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}
			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			var orderHeader = _unitOfWork.OrderHeaderRepo.Get(u => u.Id == id, "ApplicationUser");
			if(orderHeader.PaymentStatus!=SD.PaymentStatusDelayedPayment)
			{
				// Order by customer
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				if(session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeaderRepo.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
				HttpContext.Session.Clear();
			}

			var shoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u=>u.ApplicationUserId== orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCartRepo.DeleteRange(shoppingCartList);
			_unitOfWork.Save();

			return View(id);
		}

		public IActionResult Plus(int cartId)
		{
			var cartFormDb = _unitOfWork.ShoppingCartRepo.Get(u => u.Id == cartId);
			cartFormDb.Count++;
			_unitOfWork.ShoppingCartRepo.Update(cartFormDb);
			_unitOfWork.Save();

			HttpContext.Session.SetInt32(SD.SessionCart,
				_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == cartFormDb.ApplicationUserId).Count());
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cartFormDb = _unitOfWork.ShoppingCartRepo.Get(u => u.Id == cartId);
			if (cartFormDb.Count > 1)
			{
				cartFormDb.Count--;
				_unitOfWork.ShoppingCartRepo.Update(cartFormDb);
			}
			else
			{
				_unitOfWork.ShoppingCartRepo.Remove(cartFormDb);
			}

			_unitOfWork.Save();

			HttpContext.Session.SetInt32(SD.SessionCart,
				_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == cartFormDb.ApplicationUserId).Count());
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cartFormDb = _unitOfWork.ShoppingCartRepo.Get(u => u.Id == cartId);
			_unitOfWork.ShoppingCartRepo.Remove(cartFormDb);
			_unitOfWork.Save();

			HttpContext.Session.SetInt32(SD.SessionCart,
				_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == cartFormDb.ApplicationUserId).Count());
			return RedirectToAction(nameof(Index));
		}

		#region Private Method
		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Count <= 50)
			{
				return shoppingCart.Product.Price;
			}
			else if (shoppingCart.Count <= 100)
			{
				return shoppingCart.Product.Price50;
			}
			else
			{
				return shoppingCart.Product.Price100;
			}
		}
		#endregion
	}
}
