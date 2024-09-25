using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
	public class Category
	{
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Test")] // 避免null
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1,100, ErrorMessage ="Range need to be 1 to 100")]
        public int DisplayOrder { get; set; }
    }
}
