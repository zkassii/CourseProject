using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class VetRecordInput
{
    public int VetRecordId { get; set; }

    [Required(ErrorMessage = "Выберите животное")]
    [Display(Name = "Животное")]
    public int? AnimalId { get; set; }

    [Required(ErrorMessage = "Выберите тип записи")]
    [Display(Name = "Тип записи")]
    public string RecordType { get; set; } = "";

    [Required(ErrorMessage = "Укажите дату")]
    [Display(Name = "Дата")]
    public DateOnly RecordDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(1000, ErrorMessage = "Не более 1000 символов")]
    [Display(Name = "Примечание")]
    public string? Notes { get; set; }
}