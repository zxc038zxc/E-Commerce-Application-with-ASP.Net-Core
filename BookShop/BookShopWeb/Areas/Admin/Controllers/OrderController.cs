using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModel;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BookShopWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM OrderVM { get; set; }

		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}

		// 來源自AJAX的設定
		public IActionResult Details(int orderId)
		{
			OrderVM = new OrderVM
			{
				OrderHeader = _unitOfWork.OrderHeaderRepo.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetails = _unitOfWork.OrderDetailRepo.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product"),
			};
			return View(OrderVM);
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id);
			orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.City = OrderVM.OrderHeader.City;
			orderHeaderFromDb.State = OrderVM.OrderHeader.State;
			orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
			{
				orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			}
			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
			{
				orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}

			_unitOfWork.OrderHeaderRepo.Update(orderHeaderFromDb);
			_unitOfWork.Save();

			TempData["success"] = "Order Details Updated Successfully";

			return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
		{
			_unitOfWork.OrderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();

			TempData["success"] = "Order Details Updated Successfully";
			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult ShipOrder()
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id);
			orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			orderHeaderFromDb.OrderStatus = SD.StatusShipped;
			orderHeaderFromDb.ShippingDate = DateTime.Now;
			if(orderHeaderFromDb.PaymentStatus== SD.PaymentStatusDelayedPayment)
			{
				orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
			}

			_unitOfWork.OrderHeaderRepo.Update(orderHeaderFromDb);
			_unitOfWork.Save();

			TempData["success"] = "Order Shipped Successfully";
			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
		}


		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult CancelOrder()
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id);
			if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeaderFromDb.PaymentIntentId,
				};

				var service = new RefundService();
				Refund refund = service.Create(options);

				_unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
			}

			_unitOfWork.Save();
			TempData["success"] = "Order Cancelled Successfully";
			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
		}

		[ActionName("Details")]
		[HttpPost]
		public IActionResult Details_PAY_NOW()
		{
			OrderVM.OrderHeader = _unitOfWork.OrderHeaderRepo.Get(u=>u.Id == OrderVM.OrderHeader.Id , includeProperties:"ApplicationUser");
			OrderVM.OrderDetails = _unitOfWork.OrderDetailRepo.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

			// Stripe Logic
			var domain = "https://localhost:7063/";
			var options = new SessionCreateOptions
			{
				SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
				CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
			};

			foreach (var item in OrderVM.OrderDetails)
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

			_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
			_unitOfWork.Save();
			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}


		public IActionResult PaymentConfirmation(int orderHeaderId)
		{
			var orderHeader = _unitOfWork.OrderHeaderRepo.Get(u => u.Id == orderHeaderId);
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				// Order by Company
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}

			return View(orderHeaderId);
		}


		#region APICalls
		// HttpGet是一種屬性，當使用者偷過GET方法請求這個動作方法時，伺服器才會執行該方法
		// 這樣的GetAll方法允許前端使用AJAX請求來獲取產品清單，而不需要重整整個網頁
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;

			if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
				orderHeaders = _unitOfWork.OrderHeaderRepo.GetAll(includeProperties: "ApplicationUser").ToList();
			}
			else
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

				orderHeaders = _unitOfWork.OrderHeaderRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
			}

			switch (status)
			{
				case "pending":
					orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
					break;
				case "inprocess":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
					break;
				case "approved":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				default:
					break;
			}
			return Json(new { data = orderHeaders });
		}
		#endregion
	}
}
