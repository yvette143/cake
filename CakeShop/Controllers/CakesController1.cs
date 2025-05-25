using CakeShop.Data;
using CakeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // 需要登入才能操作購物車
using System.Security.Claims; // 用來取得使用者 ID
using Microsoft.AspNetCore.Identity; // UserManager


namespace CakeShop.Controllers
{
    public class CakesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // 用於取得使用者資訊

        public CakesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cakes (蛋糕列表 - 所有人可看)
        [AllowAnonymous] // 明確標示允許匿名訪問
        public async Task<IActionResult> Index()
        {
            var cakes = await _context.Cakes.ToListAsync();
            return View(cakes);
        }

        // GET: Cakes/Details/5 (蛋糕詳情 - 所有人可看)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cake = await _context.Cakes.FirstOrDefaultAsync(m => m.Id == id);
            if (cake == null)
            {
                return NotFound();
            }
            // 可以建立一個 ViewModel 來傳遞蛋糕資訊和數量選擇
            ViewBag.Quantity = 1; // 預設數量
            return View(cake);
        }


        // --- 如果需要管理功能 ---
        // GET: Cakes/Create (需要管理員角色，此處簡化，先不做角色)
        // [Authorize] // 可以加上角色限制 [Authorize(Roles="Admin")]
        // public IActionResult Create() { ... }

        // POST: Cakes/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [Authorize]
        // public async Task<IActionResult> Create(...) { ... }

        // ... 其他 CRUD (Edit, Delete) 方法 ...
    }
}