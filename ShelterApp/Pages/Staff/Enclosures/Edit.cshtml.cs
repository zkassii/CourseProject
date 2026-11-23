using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;

namespace ShelterApp.Pages.Staff.Enclosures;

[Authorize(Roles = RoleNames.Staff + "," + RoleNames.Admin)]
public class EditModel : PageModel
{
    private readonly ShelterDbContext _db;
    public EditModel(ShelterDbContext db) => _db = db;

    public class EditInput
    {
        [Required(ErrorMessage = "Введите номер")]
        [StringLength(10)]
        [Display(Name = "Номер вольера")]
        public string Number { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Вместимость от 1 до 100")]
        [Display(Name = "Вместимость")]
        public int Capacity { get; set; }
    }

    [BindProperty] public EditInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var e = await _db.Enclosures.AsNoTracking().FirstOrDefaultAsync(x => x.EnclosureId == id);
        if (e == null) return NotFound();
        Input = new EditInput { Number = e.Number, Capacity = e.Capacity };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var e = await _db.Enclosures.FirstOrDefaultAsync(x => x.EnclosureId == id);
        if (e == null) return NotFound();

        var number = (Input.Number ?? "").Trim();
        if (number != "" && await _db.Enclosures.AnyAsync(x => x.Number == number && x.EnclosureId != id))
            ModelState.AddModelError("Input.Number", "Вольер с таким номером уже есть");

        var occupied = await _db.Animals.CountAsync(a => a.EnclosureId == id);
        if (Input.Capacity < occupied)
            ModelState.AddModelError("Input.Capacity", $"В вольере сейчас {occupied} животных, вместимость не может быть меньше");

        if (!ModelState.IsValid) return Page();

        e.Number = number;
        e.Capacity = Input.Capacity;
        await _db.SaveChangesAsync();
        return RedirectToPage("/Staff/Enclosures/Index");
    }
}