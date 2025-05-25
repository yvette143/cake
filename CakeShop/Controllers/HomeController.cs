using CakeShop.Data; // 加入 using
using CakeShop.Models; // 加入 using
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 加入 using
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq; // 加入 using
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks; // 加入 using

namespace CakeShop.Controllers
{
    public class HomeController : Controller
    {
        /*   readonly (唯讀的):這是一個修飾詞，專門用於欄位它表示這個欄位的值
             只能在兩個地方被設定：
           1. 在宣告欄位的同時進行初始化?值：
           2. 在該類別的建構函式內部進行?值：
           _context 這個欄位只能在包含它的那個類別（例如 HomeController）內部被存取 (private)。
           _context 這個欄位的值只能在物件被建立時（在建構函式中）被賦值一次，之後就不能再改變它所引用的物件
           (readonly)。
         */
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // 注入 DbContext

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) // 修改建構函式
        {
            _logger = logger;
            _context = context; // 賦值
        }

        // 修改 Index 方法為 async 並查詢蛋糕
        public async Task<IActionResult> Index()
        {
            // 查詢一些蛋糕來顯示在輪播中 (例如：取前 5 筆，或未來可加上 IsFeatured 標籤篩選)
            // 確保 Cake 模型有 ImageUrl 屬性
            var carouselCakes = await _context.Cakes
                                            .Where(c => !string.IsNullOrEmpty(c.ImageUrl)) // 只選有圖片的
                                            .OrderBy(c => Guid.NewGuid()) // 隨機排序 (或按 ID, 名稱等)
                                            .Take(5) // 取最多 5 個
                                            .ToListAsync();

            // 將蛋糕列表傳遞給 View
            return View(carouselCakes);
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