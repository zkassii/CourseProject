using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Staff.Animals;

[Authorize(Roles = RoleNames.Staff + "," + RoleNames.Admin)]
public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    private readonly IWebHostEnvironment _env;
    public IndexModel(ShelterDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    public List<Animal> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var q = _db.Animals.AsNoTracking().Include(a => a.Enclosure).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(a => a.Name.Contains(s) || a.Species.Contains(s) || (a.Breed != null && a.Breed.Contains(s)));
        }
        if (!string.IsNullOrEmpty(Status)) q = q.Where(a => a.Status == Status);

        Items = await q.OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var error = await AnimalDeleter.DeleteAsync(_db, _env.WebRootPath, id);
        TempData[error == null ? "Ok" : "Error"] = error ?? "Животное удалено";
        return RedirectToPage();
    }
}