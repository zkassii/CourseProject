using DocumentFormat.OpenXml.Presentation;
using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class Enclosure
{
    public int EnclosureId { get; set; }

    [Required, MaxLength(10)]
    public string Number { get; set; } = "";

    [Range(1, 100)]
    public int Capacity { get; set; }

    public List<Animal> Animals { get; set; } = new();
}