using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookShop.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; }
		[Required] // 避免null
		public string Title { get; set; }
		public string Description { get; set; }
		[Required]
		public string Author { get; set; }
		[Required]
		[Display(Name = "List Price")]
		[Range(1, 1000)]
		public int ListPrice { get; set; }
		[Required]
		[Display(Name = "List Price for 1-50")]
		[Range(1, 1000)]
		public int Price { get; set; }
		[Required]
		[Display(Name = "Price for 50+")]
		[Range(1, 1000)]
		public int Price50 { get; set; }
		[Required]
		[Display(Name = "Price for 100+")]
		[Range(1, 1000)]
		public int Price100 { get; set; }
		[Required]
		[ValidateNever]
		public string ImageUrl { get; set; }


		#region ForeignKey
		public int CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		[ValidateNever]
		public Category Category { get; set; }
		#endregion
	}
}
