using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Services
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Service> Services { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Services = await _context.Service.OrderBy(s => s.NomService).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var service = await _context.Service.FindAsync(id);

            if (service != null)
            {
                _context.Service.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}