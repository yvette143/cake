using CakeShop.Data;
using CakeShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CakeShop.Controllers
{
    [Authorize] // 需要登入才能訪問訂單相關頁面
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";


        // GET: Orders/Checkout (顯示結帳頁面，預填用戶資料)
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var user = await _userManager.GetUserAsync(User); // 取得目前登入的使用者物件
            if (user == null) return NotFound("找不到使用者資訊");

            // 取得購物車內容，確認有東西才顯示結帳頁
            var cartItems = await _context.ShoppingCartItems
                                          .Where(c => c.UserId == userId)
                                          .Include(c => c.Cake)
                                          .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "您的購物車是空的，無法結帳。";
                return RedirectToAction("Index", "ShoppingCart");
            }

            // 計算總金額
            decimal total = cartItems.Sum(item => (item.Cake?.Price ?? 0) * item.Quantity);

            // 建立一個 ViewModel 來傳遞預設的訂單資訊
            var checkoutViewModel = new CheckoutViewModel
            {
                RecipientName = user.Name,
                ShippingAddress = user.Address,
                RecipientPhone = user.PhoneNumber ?? "", // User 可能沒有電話
                CartItems = cartItems, // 把購物車內容也傳過去顯示
                TotalAmount = total
            };

            return View(checkoutViewModel); // 將 ViewModel 傳遞給 View
        }


        // POST: Orders/PlaceOrder (處理結帳，建立訂單)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel checkoutModel) // 接收從 View POST 回來的 ViewModel
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // 重新取得購物車內容 (避免 Session 過期或資料不一致)
            var cartItems = await _context.ShoppingCartItems
                                         .Where(c => c.UserId == userId)
                                         .Include(c => c.Cake)
                                         .ToListAsync();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "您的購物車是空的。");
            }

            // 驗證 ViewModel 的資料 (例如必填欄位)
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound("找不到使用者");

                // 1. 建立 Order 物件
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow, // 使用 UTC 時間較佳
                    TotalAmount = cartItems.Sum(item => (item.Cake?.Price ?? 0) * item.Quantity),
                    ShippingAddress = checkoutModel.ShippingAddress, // 使用表單提交的地址
                    RecipientName = checkoutModel.RecipientName,
                    RecipientPhone = checkoutModel.RecipientPhone,
                    Status = OrderStatus.Pending // 初始狀態
                };

                // 2. 建立 OrderDetail 物件 (從購物車項目轉換)
                foreach (var item in cartItems)
                {
                    if (item.Cake == null) continue; // 防呆

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id, // EF Core 會自動關聯
                        CakeId = item.CakeId,
                        Quantity = item.Quantity,
                        Price = item.Cake.Price // 記錄下單當時的價格
                    };
                    order.OrderDetails.Add(orderDetail);
                }

                // 3. 將 Order 和 OrderDetails 存入資料庫
                _context.Orders.Add(order);

                // 4. 清空該使用者的購物車
                _context.ShoppingCartItems.RemoveRange(cartItems);

                // 5. 儲存所有變更 (一起存比較安全，用交易)
                await _context.SaveChangesAsync();

                // 6. 重導向到訂單成功頁面或訂單歷史頁面
                TempData["SuccessMessage"] = $"訂單 #{order.Id} 已成功建立！";
                return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
            }

            // 如果模型驗證失敗，重新顯示結帳頁面，並保留使用者輸入的資料
            // 需要重新載入購物車內容和總金額給 View
            checkoutModel.CartItems = cartItems;
            checkoutModel.TotalAmount = cartItems.Sum(item => (item.Cake?.Price ?? 0) * item.Quantity);
            return View("Checkout", checkoutModel);
        }


        // GET: Orders/OrderConfirmation/5 (顯示訂單成功訊息)
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var userId = GetUserId();
            var order = await _context.Orders
                                      .Include(o => o.OrderDetails) // 包含訂單明細
                                      .ThenInclude(od => od.Cake) // 包含明細中的蛋糕資訊
                                      .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound("找不到訂單或您無權查看此訂單");
            }
            ViewBag.SuccessMessage = TempData["SuccessMessage"]; // 顯示成功訊息
            return View(order);
        }


        // GET: Orders (顯示使用者自己的訂單歷史)
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var orders = await _context.Orders
                                       .Where(o => o.UserId == userId)
                                       .OrderByDescending(o => o.OrderDate) // 依照日期排序
                                       .ToListAsync();
            return View(orders);
        }


        // GET: Orders/Details/5 (顯示特定訂單的詳細內容)
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var order = await _context.Orders
                                     .Include(o => o.OrderDetails) // 包含訂單明細
                                     .ThenInclude(od => od.Cake) // 包含明細中的蛋糕資訊
                                     .Include(o => o.User) // 包含下單使用者資訊 (雖然通常不需要在詳情頁顯示自己)
                                     .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId); // 確保只能看自己的訂單

            if (order == null)
            {
                return NotFound("找不到訂單或您無權查看此訂單");
            }

            return View(order);
        }
    }

    // 可以獨立建立一個 ViewModel 檔案 (例如在 ViewModels 資料夾)
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "收件人姓名為必填項")]
        [StringLength(50)]
        [Display(Name = "收件人姓名")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "收貨地址為必填項")]
        [StringLength(200)]
        [Display(Name = "收貨地址")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "聯絡電話為必填項")]
        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        [Display(Name = "聯絡電話")]
        public string RecipientPhone { get; set; } = string.Empty;

        // 顯示用，不需要 Post 回來
        public IEnumerable<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        public decimal TotalAmount { get; set; }
    }
}