using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Vet.Records;

[Authorize(Roles = RoleNames.Vet)]
public class EditModel : PageModel
{
    private readonly ShelterDbContext _db;
    public EditModel(ShelterDbContext db) => _db = db;

    [BindProperty] public VetRecordInput Input { get; set; } = new();

    public List<SelectListItem> AnimalList { get; set; } = new();
    public bool IsNew => Input.VetRecordId == 0;

    private async Task LoadListAsync()
    {
        AnimalList = await _db.Animals.AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem
            {
                Value = a.AnimalId.ToString(),
                Text = a.Name + " (" + a.Species + ")"
            }).ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync(int? id, int? animalId)
    {
        if (id != null)
        {
            var r = await _db.VetRecords.AsNoTracking().FirstOrDefaultAsync(v => v.VetRecordId == id);
            if (r == null) return NotFound();

            Input = new VetRecordInput
            {
                VetRecordId = r.VetRecordId,
                AnimalId = r.AnimalId,
                RecordType = r.RecordType,
                RecordDate = r.RecordDate,
                Notes = r.Notes
            };
        }
        else if (animalId != null)
        {
            Input.AnimalId = animalId;   // пришли из карточки животного
        }

        await LoadListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!VetRecordTypes.All.Contains(Input.RecordType))
            ModelState.AddModelError("Input.RecordType", "Выберите тип из списка");
        if (Input.RecordDate > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("Input.RecordDate", "Дата не может быть в будущем");
        if (Input.AnimalId != null && !await _db.Animals.AnyAsync(a => a.AnimalId == Input.AnimalId))
            ModelState.AddModelError("Input.AnimalId", "Животное не найдено");

        if (!ModelState.IsValid)
        {
            await LoadListAsync();
            return Page();
        }

        VetRecord? r;
        if (Input.VetRecordId == 0)
        {
            r = new VetRecord { UserId = User.GetUserId() };   // ветеринар = текущий пользователь
            _db.VetRecords.Add(r);
        }
        else
        {
            r = await _db.VetRecords.FirstOrDefaultAsync(v => v.VetRecordId == Input.VetRecordId);
            if (r == null) return NotFound();
        }

        r.AnimalId = Input.AnimalId!.Value;
        r.RecordType = Input.RecordType;
        r.RecordDate = Input.RecordDate;
        r.Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim();

        await _db.SaveChangesAsync();
        return RedirectToPage("/Vet/Records/Index");
    }
}