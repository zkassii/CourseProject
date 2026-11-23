using Microsoft.AspNetCore.Mvc.RazorPages;
using ShelterApp.Data;
using ShelterApp.Models;

namespace ShelterApp.Pages;

public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    public int ReadyCount { get; set; }

    public IndexModel(ShelterDbContext db) => _db = db;

    public void OnGet() =>
        ReadyCount = _db.Animals.Count(a => a.Status == AnimalStatus.Ready);
}