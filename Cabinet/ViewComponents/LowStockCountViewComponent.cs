using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Cabinet.Data;

namespace Cabinet.ViewComponents
{
    public class LowStockCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public LowStockCountViewComponent(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var count = await _cache.GetOrCreateAsync("LowStockCount", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return await _context.Stocks
                    .Where(s => s.Quantite <= s.Alarme)
                    .CountAsync();
            });

            return View(count);
        }
    }
}