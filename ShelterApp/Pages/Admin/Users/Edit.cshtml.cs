using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Admin.Users;

[Authorize(Roles = RoleNames.Admin)]
public class EditModel : PageModel
{
    private readonly ShelterDbContext _db;
    public EditModel(ShelterDbContext db) => _db = db;

    [BindProperty] public UserInput Input { get; set; } = new();

    public List<SelectListItem> RoleList { get; set; } = new();
    public bool IsNew => Input.UserId == 0;
    public bool IsSelf => Input.UserId != 0 && Input.UserId == User.GetUserId();

    private async Task LoadRolesAsync()
    {
        RoleList = await _db.Roles.AsNoTracking().OrderBy(r => r.RoleId)
            .Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.Name })
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id != null)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
            if (u == null) return NotFound();

            Input = new UserInput
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                RoleId = u.RoleId
            };
        }
        await LoadRolesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Models.User? u = null;
        if (Input.UserId != 0)
        {
            u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == Input.UserId);
            if (u == null) return NotFound();
        }

        bool isSelf = u != null && u.UserId == User.GetUserId();
        if (isSelf) Input.RoleId = u!.RoleId;      // свою роль менять нельзя
        ModelState.Remove("Input.RoleId");

        // при создании пароль обязателен
        if (u == null && string.IsNullOrWhiteSpace(Input.Password))
            ModelState.AddModelError("Input.Password", "Введите пароль");

        var email = (Input.Email ?? "").Trim().ToLower();
        if (email != "" && await _db.Users.AnyAsync(x => x.Email == email && x.UserId != Input.UserId))
            ModelState.AddModelError("Input.Email", "Пользователь с такой почтой уже есть");

        if (Input.RoleId == null || !await _db.Roles.AnyAsync(r => r.RoleId == Input.RoleId))
            ModelState.AddModelError("Input.RoleId", "Выберите роль");

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return Page();
        }

        if (u == null)
        {
            u = new Models.User();
            _db.Users.Add(u);
        }

        u.FullName = Input.FullName.Trim();
        u.Email = email;
        u.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();
        u.RoleId = Input.RoleId!.Value;

        if (!string.IsNullOrWhiteSpace(Input.Password))
            u.PasswordHash = new PasswordHasher<Models.User>().HashPassword(u, Input.Password);

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Данные сохранены";
        return RedirectToPage("/Admin/Users/Index");
    }
}