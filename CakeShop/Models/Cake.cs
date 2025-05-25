using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cakeshop.Models
{
    public class Cake
    {
        public int Id { get; set; }  // 修正 get: 為 get;

        [Required(ErrorMessage = "蛋糕名稱為必填項")]
        [StringLength(100)]
        [Display(Name = "蛋糕名稱")]  // 修正雙引號錯誤（中文雙引號改為英文）
        public string Name { get; set; } = string.Empty;

        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "價格為必填項")]
        [Range(0.01, 10000.00, ErrorMessage = "價格必須介於 0.01 和 10000 之間")]
        [Column(TypeName = "decimal(18, 2)")] // 資料庫中的精確數字類型
        [Display(Name = "價格")]
        public decimal Price { get; set; }  // 修正 get: 為 get;

        [Display(Name = "圖片路徑")]
        public string? ImageUrl { get; set; } // 圖片路徑（例如 /images/chocolate.jpg）
    }
}
