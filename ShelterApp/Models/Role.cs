using System.ComponentModel.DataAnnotations;

namespace ShelterApp.Models;

public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    public List<User> Users { get; set; } = new();
}