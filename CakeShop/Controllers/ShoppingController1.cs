using CakeShop.Data;
using CakeShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CakeShop.Controllers
{
    [Authorize] // 整個 Controller 都需要登入
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 取得目前登入使用者的 ID
        private string GetUserId()
        {
            // User 是 Controller 內建的屬性，代表目前的使用者 Principal
            // FindFirstValue 會找到第一個符合 ClaimTypes.NameIdentifier 的 Claim (也就是 User ID)
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        }

        // GET: ShoppingCart (顯示購物車內容)
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                // 理論上 [Authorize] 會處理，但多一層防護
                return Challenge(); // 要求登入
            }

            // 找出屬於該使用者的購物車項目，並包含相關的 Cake 資料
            var cartItems = await _context.ShoppingCartItems
                                          .Where(c => c.UserId == userId)
                                          .Include(c => c.Cake) // 載入關聯的 Cake 實體
                                          .ToListAsync();

            // 計算總金額 (可以在 View 或 Controller 計算)
            decimal total = cartItems.Sum(item => (item.Cake?.Price ?? 0) * item.Quantity);
            ViewBag.CartTotal = total;

            return View(cartItems);
        }

        // POST: ShoppingCart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int cakeId, int quantity)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            if (quantity <= 0)
            {
                // 可以用 TempData 顯示錯誤訊息
                TempData["CartMessage"] = "數量必須大於 0";
                return RedirectToAction("Details", "Cakes", new { id = cakeId }); // 或返回來源頁
            }

            // 檢查購物車是否已有此商品
            var cartItem = await _context.ShoppingCartItems
                                         .FirstOrDefaultAsync(c => c.UserId == userId && c.CakeId == cakeId);

            var cake = await _context.Cakes.FindAsync(cakeId);
            if (cake == null) return NotFound("找不到此蛋糕");


            if (cartItem == null)
            {
                // 新增項目
                cartItem = new ShoppingCartItem
                {
                    CakeId = cakeId,
                    UserId = userId,
                    Quantity = quantity
                };
                _context.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                // 更新數量
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
            TempData["CartMessage"] = $"已將 {quantity} 個「{cake.Name}」加入購物車！"; // 成功訊息

            // 返回購物車頁面或來源頁面
            // string returnUrl = Request.Headers["Referer"].ToString(); // 取得上一頁 URL
            // if (!string.IsNullOrEmpty(returnUrl))
            // {
            //      return Redirect(returnUrl);
            // }
            return RedirectToAction(nameof(Index)); // 預設跳回購物車列表
        }


        // POST: ShoppingCart/RemoveFromCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id) // id 是 ShoppingCartItem 的 ID
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var cartItem = await _context.ShoppingCartItems
                                  .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["CartMessage"] = "已成功移除商品";
            }
            else
            {
                TempData["CartMessage"] = "找不到要移除的商品或權限不足";
                return NotFound(); // 或返回錯誤頁
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ShoppingCart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity) // id 是 ShoppingCartItem 的 ID
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Challenge();

            if (quantity <= 0)
            {
                // 如果數量小於等於 0，直接移除該項目
                return await RemoveFromCart(id);
            }

            var cartItem = await _context.ShoppingCartItems
                                   .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
                TempData["CartMessage"] = "購物車數量已更新";
            }
            else
            {
                TempData["CartMessage"] = "找不到要更新的商品或權限不足";
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}