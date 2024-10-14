using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models.ViewModel
{
	public class OrderVM
	{
		public OrderHeader OrderHeader { get; set; }
		public IEnumerable<OrderDetail> OrderDetails { get; set; }
	}
}
