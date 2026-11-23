using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Animals;

public class DetailsModel : PageModel
{
    private readonly ShelterDbContext _db;
    private readonly IWebHostEnvironment _env;
    public DetailsModel(ShelterDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public Animal Item { get; set; } = null!;
    public List<Animal> EnclosureAnimals { get; set; } = new();
    public bool HasActiveRequest { get; set; }

    public bool CanManage => User.IsInRole(RoleNames.Staff) || User.IsInRole(RoleNames.Admin);
    public bool CanSeeHistory => CanManage || User.IsInRole(RoleNames.Vet);

    // куда ведёт ссылка "Назад к списку"
    public string BackUrl => CanManage ? "/Staff/Animals"
                           : User.IsInRole(RoleNames.Vet) ? "/Vet/Records"
                           : "/Animals";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var a = await _db.Animals.AsNoTracking()
            .Include(x => x.Enclosure)
            .Include(x => x.VetRecords.OrderByDescending(v => v.RecordDate))
                .ThenInclude(v => v.User)
            .FirstOrDefaultAsync(x => x.AnimalId == id);
        if (a == null) return NotFound();

        // гость и обычный пользователь видят только животных из каталога
        if (!CanSeeHistory && !IndexModel.CatalogStatuses.Contains(a.Status)) return NotFound();

        Item = a;

        // животные из того же вольера (для вкладки "Вольер")
        if (CanSeeHistory && a.EnclosureId != null)
        {
            EnclosureAnimals = await _db.Animals.AsNoTracking()
                .Where(x => x.EnclosureId == a.EnclosureId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        if (User.IsInRole(RoleNames.User))
        {
            var uid = User.GetUserId();
            HasActiveRequest = await _db.AdoptionRequests.AnyAsync(r =>
                r.AnimalId == id && r.UserId == uid &&
                (r.Status == RequestStatus.New || r.Status == RequestStatus.Approved));
        }
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(int id, string status)
    {
        if (!CanManage) return Forbid();
        if (!AnimalStatus.All.Contains(status)) return RedirectToPage(new { id });

        var a = await _db.Animals.FirstOrDefaultAsync(x => x.AnimalId == id);
        if (a == null) return NotFound();

        a.Status = status;
        AnimalRules.Apply(a);
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!CanManage) return Forbid();

        var error = await AnimalDeleter.DeleteAsync(_db, _env.WebRootPath, id);
        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToPage(new { id });
        }
        return RedirectToPage("/Staff/Animals/Index");
    }

    public async Task<IActionResult> OnPostApplyAsync(int id)
    {
        if (User.Identity?.IsAuthenticated != true) return Challenge();
        if (!User.IsInRole(RoleNames.User)) return Forbid();

        var a = await _db.Animals.AsNoTracking().FirstOrDefaultAsync(x => x.AnimalId == id);
        if (a == null) return NotFound();

        if (a.Status != AnimalStatus.Ready)
        {
            TempData["Error"] = "На это животное сейчас нельзя подать заявку";
            return RedirectToPage(new { id });
        }

        var uid = User.GetUserId();
        var exists = await _db.AdoptionRequests.AnyAsync(r =>
            r.AnimalId == id && r.UserId == uid &&
            (r.Status == RequestStatus.New || r.Status == RequestStatus.Approved));
        if (exists)
        {
            TempData["Error"] = "Вы уже подали заявку на это животное";
            return RedirectToPage(new { id });
        }

        _db.AdoptionRequests.Add(new AdoptionRequest
        {
            AnimalId = id,
            UserId = uid,
            Status = RequestStatus.New,
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        TempData["Ok"] = "Заявка отправлена. Сотрудник приюта рассмотрит её и свяжется с вами";
        return RedirectToPage("/MyRequests");
    }
}