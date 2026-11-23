using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ShelterDbContext _db;
    public LoginModel(ShelterDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Введите почту")]
        [EmailAddress(ErrorMessage = "Некорректная почта")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim().ToLower();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        var ok = user != null &&
                 new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, Input.Password)
                     != PasswordVerificationResult.Failed;
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Неверная почта или пароль");
            return Page();
        }

        await AuthHelper.SignInAsync(HttpContext, user!, user!.Role!.Name);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToPage("/Index");
    }
}