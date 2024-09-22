using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookShopWeb.Models
{
	public class Category
	{
        [Key]
        public int Id { get; set; }
        [Required] // 避免null
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }
    }
}
