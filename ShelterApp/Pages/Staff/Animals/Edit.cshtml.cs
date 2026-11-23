using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Staff.Animals;

[Authorize(Roles = RoleNames.Staff + "," + RoleNames.Admin)]
public class EditModel : PageModel
{
    private readonly ShelterDbContext _db;
    private readonly IWebHostEnvironment _env;
    public EditModel(ShelterDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty] public AnimalInput Input { get; set; } = new();
    [BindProperty] public IFormFile? Photo { get; set; }

    public string? CurrentPhoto { get; set; }
    public List<SelectListItem> Enclosures { get; set; } = new();
    public bool IsNew => Input.AnimalId == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id != null)
        {
            var a = await _db.Animals.AsNoTracking().FirstOrDefaultAsync(x => x.AnimalId == id);
            if (a == null) return NotFound();

            Input = new AnimalInput
            {
                AnimalId = a.AnimalId,
                Name = a.Name,
                Species = a.Species,
                Breed = a.Breed,
                Gender = a.Gender,
                BirthDate = a.BirthDate,
                Status = a.Status,
                IntakeDate = a.IntakeDate,
                EnclosureId = a.EnclosureId
            };
            CurrentPhoto = a.PhotoPath;
        }
        Enclosures = await EnclosureHelper.GetSelectListAsync(_db);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Animal? a = null;
        if (Input.AnimalId != 0)
        {
            a = await _db.Animals.FirstOrDefaultAsync(x => x.AnimalId == Input.AnimalId);
            if (a == null) return NotFound();
            CurrentPhoto = a.PhotoPath;
        }

        // дополнительные проверки
        if (Input.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("Input.BirthDate", "Дата рождения не может быть в будущем");
        if (!AnimalStatus.All.Contains(Input.Status))
            ModelState.AddModelError("Input.Status", "Выберите статус из списка");
        if (Input.Gender != "Самец" && Input.Gender != "Самка")
            ModelState.AddModelError("Input.Gender", "Выберите пол из списка");

        if (Input.Status != AnimalStatus.Adopted)
        {
            var err = await EnclosureHelper.CheckCapacityAsync(_db, Input.EnclosureId, a?.AnimalId);
            if (err != null) ModelState.AddModelError("Input.EnclosureId", err);
        }

        if (Photo != null)
        {
            var perr = PhotoHelper.Validate(Photo);
            if (perr != null) ModelState.AddModelError("Photo", perr);
        }

        if (!ModelState.IsValid)
        {
            Enclosures = await EnclosureHelper.GetSelectListAsync(_db);
            return Page();
        }

        if (a == null)
        {
            a = new Animal();
            _db.Animals.Add(a);
        }

        a.Name = Input.Name.Trim();
        a.Species = Input.Species.Trim();
        a.Breed = string.IsNullOrWhiteSpace(Input.Breed) ? null : Input.Breed.Trim();
        a.Gender = Input.Gender;
        a.BirthDate = Input.BirthDate;
        a.Status = Input.Status;
        a.IntakeDate = Input.IntakeDate;
        a.EnclosureId = Input.EnclosureId;
        AnimalRules.Apply(a);

        if (Photo != null)
        {
            PhotoHelper.Delete(a.PhotoPath, _env.WebRootPath);   // старое фото удаляем
            a.PhotoPath = await PhotoHelper.SaveAsync(Photo, _env.WebRootPath);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("/Staff/Animals/Index");
    }
}