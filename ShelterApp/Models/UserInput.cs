using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class UserInput
{
    public int UserId { get; set; }

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

    [Required(ErrorMessage = "Выберите роль")]
    [Display(Name = "Роль")]
    public int? RoleId { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не короче 6 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }
}