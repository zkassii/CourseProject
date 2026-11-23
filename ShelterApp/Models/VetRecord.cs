using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class VetRecord
{
    public int VetRecordId { get; set; }

    public int AnimalId { get; set; }
    public Animal? Animal { get; set; }

    public int UserId { get; set; }          // ветеринар
    public User? User { get; set; }

    [Required, MaxLength(30)]
    public string RecordType { get; set; } = "";

    public DateOnly RecordDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}