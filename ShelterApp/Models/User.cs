using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;

namespace ShelterApp.Models;

public class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(100)]
    public string Email { get; set; } = "";

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [MaxLength(20)]
    public string? Phone { get; set; }

    public List<VetRecord> VetRecords { get; set; } = new();
    public List<AdoptionRequest> AdoptionRequests { get; set; } = new();
}