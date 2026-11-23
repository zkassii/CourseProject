using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;

namespace ShelterApp.Pages.Vet.Records;

[Authorize(Roles = RoleNames.Vet)]
public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    public IndexModel(ShelterDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int? AnimalId { get; set; }
    [BindProperty(SupportsGet = true)] public string? RecordType { get; set; }

    public List<VetRecord> Items { get; set; } = new();
    public List<SelectListItem> AnimalList { get; set; } = new();

    public async Task OnGetAsync()
    {
        AnimalList = await _db.Animals.AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem
            {
                Value = a.AnimalId.ToString(),
                Text = a.Name + " (" + a.Species + ")"
            }).ToListAsync();

        var q = _db.VetRecords.AsNoTracking()
            .Include(v => v.Animal)
            .Include(v => v.User)
            .AsQueryable();

        if (AnimalId != null) q = q.Where(v => v.AnimalId == AnimalId);
        if (!string.IsNullOrEmpty(RecordType)) q = q.Where(v => v.RecordType == RecordType);

        Items = await q.OrderByDescending(v => v.RecordDate).ThenByDescending(v => v.VetRecordId).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var r = await _db.VetRecords.FirstOrDefaultAsync(v => v.VetRecordId == id);
        if (r != null)
        {
            _db.VetRecords.Remove(r);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}