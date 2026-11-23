using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Admin.Users;

[Authorize(Roles = RoleNames.Admin)]
public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    public IndexModel(ShelterDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? RoleId { get; set; }

    public List<Models.User> Items { get; set; } = new();
    public List<SelectListItem> RoleList { get; set; } = new();
    public int CurrentUserId { get; set; }

    public async Task OnGetAsync()
    {
        CurrentUserId = User.GetUserId();

        RoleList = await _db.Roles.AsNoTracking().OrderBy(r => r.RoleId)
            .Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.Name })
            .ToListAsync();

        var q = _db.Users.AsNoTracking().Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(u => u.FullName.Contains(s) || u.Email.Contains(s));
        }
        if (RoleId != null) q = q.Where(u => u.RoleId == RoleId);

        Items = await q.OrderBy(u => u.FullName).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (id == User.GetUserId())
        {
            TempData["Error"] = "Нельзя удалить самого себя";
            return RedirectToPage();
        }

        var u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (u == null) return RedirectToPage();

        if (await _db.VetRecords.AnyAsync(v => v.UserId == id) ||
            await _db.AdoptionRequests.AnyAsync(r => r.UserId == id))
        {
            TempData["Error"] = "Нельзя удалить: у пользователя есть ветеринарные записи или заявки";
            return RedirectToPage();
        }

        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Пользователь удален";
        return RedirectToPage();
    }
}