using Cakeshop.Models;
using CakeShop.Data;
using CakeShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CakeShop.Models
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // 如果資料庫已經有蛋糕資料，就不需要再新增
            if (context.Cakes.Any())
            {
                return; // DB 已經初始化過了
            }

            var cakes = new Cake[]
            {
                new Cake { Name = "漆黑可可樂章", Description = "濃郁滑順的黑巧克力甘納許", Price = 550.00m, ImageUrl = "/images/3.jpg" },
                new Cake { Name = "荔枝茶語初夏風", Description = "清爽荔枝搭配清香伯爵茶", Price = 620.00m, ImageUrl = "/images/2.jpg" },
                new Cake { Name = "翠玉脆語茉莉心", Description = "開心果脆匠心搭配茉莉花奶餡", Price = 680.00m, ImageUrl = "/images/6.jpg" },
                new Cake { Name = "春日櫻語緋花境", Description = "櫻花香緹與草莓奶凍交織", Price = 580.00m, ImageUrl = "/images/1.jpg" },
                new Cake { Name = "海洋之心藍夢境", Description = "海鹽香草奶霜搭配藍莓果凍內餡", Price = 660.00m, ImageUrl = "/images/4.jpg" },
            };

            foreach (var cake in cakes)
            {
                context.Cakes.Add(cake);
            }

            context.SaveChanges();
        }
    }
}
