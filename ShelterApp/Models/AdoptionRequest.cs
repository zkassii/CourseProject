using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class AdoptionRequest
{
    [Key]
    public int RequestId { get; set; }

    public int AnimalId { get; set; }
    public Animal? Animal { get; set; }

    public int UserId { get; set; }          // заявитель
    public User? User { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = RequestStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateOnly? TransferDate { get; set; }
}