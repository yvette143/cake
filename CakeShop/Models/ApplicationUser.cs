using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CakeShop.Models
{
    // 繼承 IdentityUser 來擴充使用者資訊
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "姓名為必填項")]
        [StringLength(50)]
        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "地址為必填項")]
        [StringLength(200)]
        [Display(Name = "地址")]
        public string Address { get; set; } = string.Empty;

        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<ShoppingCartItem>? ShoppingCartItems { get; set; }
    }
}
