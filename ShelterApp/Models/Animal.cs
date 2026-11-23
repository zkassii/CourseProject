using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class Animal
{
    public int AnimalId { get; set; }

    public int? EnclosureId { get; set; }
    public Enclosure? Enclosure { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    [Required, MaxLength(50)]
    public string Species { get; set; } = "";

    [MaxLength(100)]
    public string? Breed { get; set; }

    [Required, MaxLength(10)]
    public string Gender { get; set; } = "";

    public DateOnly? BirthDate { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = AnimalStatus.Quarantine;

    public DateOnly IntakeDate { get; set; }
    public DateOnly? ReleaseDate { get; set; }

    [MaxLength(255)]
    public string? PhotoPath { get; set; }

    public List<VetRecord> VetRecords { get; set; } = new();
    public List<AdoptionRequest> AdoptionRequests { get; set; } = new();
}