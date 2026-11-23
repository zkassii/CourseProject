using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages;

[Authorize(Roles = RoleNames.User)]
public class MyRequestsModel : PageModel
{
    private readonly ShelterDbContext _db;
    public MyRequestsModel(ShelterDbContext db) => _db = db;

    public List<AdoptionRequest> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var uid = User.GetUserId();
        Items = await _db.AdoptionRequests.AsNoTracking()
            .Include(r => r.Animal)
            .Where(r => r.UserId == uid)
            .OrderBy(r => r.CreatedAt).ThenBy(r => r.RequestId)
            .ToListAsync();
    }

    // отменить можно только новую заявку
    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var uid = User.GetUserId();
        var r = await _db.AdoptionRequests.FirstOrDefaultAsync(x => x.RequestId == id && x.UserId == uid);

        if (r != null && r.Status == RequestStatus.New)
        {
            _db.AdoptionRequests.Remove(r);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Заявка отменена";
        }
        else
        {
            TempData["Error"] = "Эту заявку отменить нельзя";
        }
        return RedirectToPage();
    }
}