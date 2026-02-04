using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cabinet.Data;

namespace Cabinet.ViewComponents
{
    public class LowStockCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LowStockCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Calculate count of items at or below alarm level
            var count = await _context.Stocks
                .Where(s => s.Quantite <= s.Alarme)
                .CountAsync();

            return View(count);
        }
    }
}