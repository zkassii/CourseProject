using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;

namespace ShelterApp.Pages.Animals;

public class IndexModel : PageModel
{
    private readonly ShelterDbContext _db;
    public IndexModel(ShelterDbContext db) => _db = db;

    // статусы, которые видит гость в каталоге
    public static readonly string[] CatalogStatuses = { AnimalStatus.Ready, AnimalStatus.Reserved };

    // параметры фильтров берутся из адресной строки (метод GET)
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Species { get; set; }
    [BindProperty(SupportsGet = true)] public string? Breed { get; set; }
    [BindProperty(SupportsGet = true)] public string? Gender { get; set; }
    [BindProperty(SupportsGet = true)] public string? Age { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Sort { get; set; }

    public List<Animal> Items { get; set; } = new();
    public List<string> SpeciesList { get; set; } = new();
    public List<string> BreedList { get; set; } = new();

    public async Task OnGetAsync()
    {
        // все животные, доступные в каталоге
        var baseQuery = _db.Animals.AsNoTracking()
            .Where(a => CatalogStatuses.Contains(a.Status));

        // значения для выпадающих списков
        SpeciesList = await baseQuery.Select(a => a.Species).Distinct().OrderBy(x => x).ToListAsync();
        BreedList = await baseQuery.Where(a => a.Breed != null)
            .Select(a => a.Breed!).Distinct().OrderBy(x => x).ToListAsync();

        var q = baseQuery;

        // поиск по тексту: кличка, вид или порода
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(a => a.Name.Contains(s) || a.Species.Contains(s)
                          || (a.Breed != null && a.Breed.Contains(s)));
        }

        // фильтры
        if (!string.IsNullOrEmpty(Species)) q = q.Where(a => a.Species == Species);
        if (!string.IsNullOrEmpty(Breed)) q = q.Where(a => a.Breed == Breed);
        if (!string.IsNullOrEmpty(Gender)) q = q.Where(a => a.Gender == Gender);
        if (!string.IsNullOrEmpty(Status)) q = q.Where(a => a.Status == Status);

        // возраст: считаем границы дат рождения
        var today = DateOnly.FromDateTime(DateTime.Today);
        var y1 = today.AddYears(-1);
        var y3 = today.AddYears(-3);
        q = Age switch
        {
            "lt1" => q.Where(a => a.BirthDate != null && a.BirthDate > y1),
            "1to3" => q.Where(a => a.BirthDate != null && a.BirthDate <= y1 && a.BirthDate > y3),
            "gt3" => q.Where(a => a.BirthDate != null && a.BirthDate <= y3),
            _ => q
        };

        // сортировка (для возраста "по возрастанию" = от младших к старшим)
        q = Sort switch
        {
            "name_desc" => q.OrderByDescending(a => a.Name),
            "age_young" => q.OrderByDescending(a => a.BirthDate),
            "age_old" => q.OrderBy(a => a.BirthDate == null).ThenBy(a => a.BirthDate),
            "intake_new" => q.OrderByDescending(a => a.IntakeDate),
            "intake_old" => q.OrderBy(a => a.IntakeDate),
            _ => q.OrderBy(a => a.Name)
        };

        Items = await q.ToListAsync();
    }
}