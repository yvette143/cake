using Cakeshop.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CakeShop.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        [Required]
        public int CakeId { get; set; } // 外鍵到 Cake

        [ForeignKey("CakeId")]
        public virtual Cake? Cake { get; set; }

        [Range(1, 100, ErrorMessage = "數量必須介於 1 和 100 之間")]
        [Display(Name = "數量")]
        public int Quantity { get; set; } = 1;  // 預設數量為1

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
