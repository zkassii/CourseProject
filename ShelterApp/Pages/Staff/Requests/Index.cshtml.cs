using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Staff.Requests;

[Authorize(Roles = RoleNames.Staff + "," + RoleNames.Admin)]
public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    public IndexModel(ShelterDbContext db) => _db = db;

    public int InShelter { get; set; }
    public int FreePlaces { get; set; }
    public int NewRequests { get; set; }
    public int InTreatment { get; set; }
    public List<AdoptionRequest> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        // счётчики (плитки сверху)
        InShelter = await _db.Animals.CountAsync(a => a.Status != AnimalStatus.Adopted);
        var capacity = await _db.Enclosures.SumAsync(e => (int?)e.Capacity) ?? 0;
        var occupied = await _db.Animals.CountAsync(a => a.EnclosureId != null);
        FreePlaces = Math.Max(0, capacity - occupied);
        NewRequests = await _db.AdoptionRequests.CountAsync(r => r.Status == RequestStatus.New);
        InTreatment = await _db.Animals.CountAsync(a => a.Status == AnimalStatus.Treatment);

        // таблица заявок: новые сверху
        Items = await _db.AdoptionRequests.AsNoTracking()
            .Include(r => r.Animal)
            .Include(r => r.User)
            .OrderByDescending(r => r.RequestId)
            .ToListAsync();
    }

    // заявка не выбрана в таблице
    private IActionResult? NoSelection(int id)
    {
        if (id != 0) return null;
        TempData["Error"] = "Выберите заявку в таблице";
        return RedirectToPage();
    }

    private Task<AdoptionRequest?> FindAsync(int id) =>
        _db.AdoptionRequests.Include(r => r.Animal).FirstOrDefaultAsync(r => r.RequestId == id);

    // принять: заявка одобрена, животное забронировано
    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var ns = NoSelection(id); if (ns != null) return ns;
        var r = await FindAsync(id);
        if (r == null) return NotFound();

        if (r.Status != RequestStatus.New)
            TempData["Error"] = "Принять можно только новую заявку";
        else if (r.Animal!.Status != AnimalStatus.Ready)
            TempData["Error"] = $"Животное сейчас не готово к пристройству (статус: {r.Animal.Status})";
        else
        {
            r.Status = RequestStatus.Approved;
            r.Animal.Status = AnimalStatus.Reserved;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Заявка принята, животное забронировано";
        }
        return RedirectToPage();
    }

    // отклонить: если заявка была одобрена, животное снова доступно
    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var ns = NoSelection(id); if (ns != null) return ns;
        var r = await FindAsync(id);
        if (r == null) return NotFound();

        if (r.Status != RequestStatus.New && r.Status != RequestStatus.Approved)
            TempData["Error"] = "Эту заявку отклонить нельзя";
        else
        {
            if (r.Status == RequestStatus.Approved && r.Animal!.Status == AnimalStatus.Reserved)
                r.Animal.Status = AnimalStatus.Ready;

            r.Status = RequestStatus.Rejected;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Заявка отклонена";
        }
        return RedirectToPage();
    }

    // оформить передачу новому владельцу
    public async Task<IActionResult> OnPostTransferAsync(int id)
    {
        var ns = NoSelection(id); if (ns != null) return ns;
        var r = await FindAsync(id);
        if (r == null) return NotFound();

        if (r.Status != RequestStatus.Approved)
            TempData["Error"] = "Передачу можно оформить только по одобренной заявке";
        else
        {
            r.Status = RequestStatus.Transferred;
            r.TransferDate = DateOnly.FromDateTime(DateTime.Today);

            r.Animal!.Status = AnimalStatus.Adopted;
            AnimalRules.Apply(r.Animal);           // дата выбытия, вольер освобождается

            // остальные открытые заявки на это животное отклоняются
            var others = await _db.AdoptionRequests
                .Where(x => x.AnimalId == r.AnimalId && x.RequestId != r.RequestId &&
                            (x.Status == RequestStatus.New || x.Status == RequestStatus.Approved))
                .ToListAsync();
            foreach (var o in others) o.Status = RequestStatus.Rejected;

            await _db.SaveChangesAsync();
            TempData["Ok"] = "Передача оформлена, животное пристроено";
        }
        return RedirectToPage();
    }
}