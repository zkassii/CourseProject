using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ShelterDbContext _db;
    public RegisterModel(ShelterDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Введите ФИО")]
        [StringLength(100)]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Введите почту")]
        [EmailAddress(ErrorMessage = "Некорректная почта")]
        [StringLength(100)]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = "";

        [Phone(ErrorMessage = "Некорректный телефон")]
        [StringLength(20)]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не короче 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Повторите пароль")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Повтор пароля")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError("Input.Email", "Пользователь с такой почтой уже зарегистрирован");
            return Page();
        }

        var role = await _db.Roles.FirstAsync(r => r.Name == RoleNames.User);
        var user = new User
        {
            FullName = Input.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim(),
            RoleId = role.RoleId
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, Input.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await AuthHelper.SignInAsync(HttpContext, user, role.Name);
        return RedirectToPage("/Index");
    }
}