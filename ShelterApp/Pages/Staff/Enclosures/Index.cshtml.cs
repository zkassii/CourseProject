using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;

namespace ShelterApp.Pages.Staff.Enclosures;

[Authorize(Roles = RoleNames.Staff + "," + RoleNames.Admin)]
public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    public IndexModel(ShelterDbContext db) => _db = db;

    public class Row
    {
        public int EnclosureId { get; set; }
        public string Number { get; set; } = "";
        public int Capacity { get; set; }
        public int Occupied { get; set; }
    }

    public class NewInput
    {
        [Required(ErrorMessage = "Введите номер")]
        [StringLength(10)]
        [Display(Name = "Номер вольера")]
        public string Number { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Вместимость от 1 до 100")]
        [Display(Name = "Вместимость")]
        public int Capacity { get; set; } = 1;
    }

    public List<Row> Items { get; set; } = new();

    [BindProperty] public NewInput Input { get; set; } = new();

    public async Task OnGetAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        Items = await _db.Enclosures.AsNoTracking()
            .OrderBy(e => e.Number)
            .Select(e => new Row
            {
                EnclosureId = e.EnclosureId,
                Number = e.Number,
                Capacity = e.Capacity,
                Occupied = e.Animals.Count
            }).ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var number = (Input.Number ?? "").Trim();
        if (number != "" && await _db.Enclosures.AnyAsync(e => e.Number == number))
            ModelState.AddModelError("Input.Number", "Вольер с таким номером уже есть");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        _db.Enclosures.Add(new Enclosure { Number = number, Capacity = Input.Capacity });
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var e = await _db.Enclosures.FirstOrDefaultAsync(x => x.EnclosureId == id);
        if (e != null)
        {
            if (await _db.Animals.AnyAsync(a => a.EnclosureId == id))
                TempData["Error"] = "Нельзя удалить вольер, в котором есть животные";
            else
            {
                _db.Enclosures.Remove(e);
                await _db.SaveChangesAsync();
            }
        }
        return RedirectToPage();
    }
}