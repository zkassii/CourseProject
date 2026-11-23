using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class AnimalInput
{
    public int AnimalId { get; set; }

    [Required(ErrorMessage = "Введите кличку")]
    [StringLength(50)]
    [Display(Name = "Кличка")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Введите вид")]
    [StringLength(50)]
    [Display(Name = "Вид")]
    public string Species { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Порода")]
    public string? Breed { get; set; }

    [Required(ErrorMessage = "Выберите пол")]
    [Display(Name = "Пол")]
    public string Gender { get; set; } = "";

    [Display(Name = "Дата рождения")]
    public DateOnly? BirthDate { get; set; }

    [Required(ErrorMessage = "Выберите статус")]
    [Display(Name = "Статус")]
    public string Status { get; set; } = AnimalStatus.Quarantine;

    [Required(ErrorMessage = "Укажите дату поступления")]
    [Display(Name = "Дата поступления")]
    public DateOnly IntakeDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Вольер")]
    public int? EnclosureId { get; set; }
}