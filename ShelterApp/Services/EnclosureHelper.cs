using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;

namespace ShelterApp.Services;

public static class EnclosureHelper
{
    // список вольеров для выпадающего списка: "Вольер 1 (занято 2 из 3)"
    public static async Task<List<SelectListItem>> GetSelectListAsync(ShelterDbContext db)
    {
        var rows = await db.Enclosures.AsNoTracking()
            .OrderBy(e => e.Number)
            .Select(e => new { e.EnclosureId, e.Number, e.Capacity, Count = e.Animals.Count })
            .ToListAsync();

        return rows.Select(r => new SelectListItem
        {
            Value = r.EnclosureId.ToString(),
            Text = $"Вольер {r.Number} (занято {r.Count} из {r.Capacity})"
        }).ToList();
    }

    // проверка вместимости: возвращает текст ошибки или null, если места хватает
    public static async Task<string?> CheckCapacityAsync(ShelterDbContext db, int? enclosureId, int? excludeAnimalId)
    {
        if (enclosureId == null) return null;

        var e = await db.Enclosures.AsNoTracking().FirstOrDefaultAsync(x => x.EnclosureId == enclosureId);
        if (e == null) return "Вольер не найден";

        var count = await db.Animals.CountAsync(a =>
            a.EnclosureId == enclosureId && (excludeAnimalId == null || a.AnimalId != excludeAnimalId));

        return count >= e.Capacity ? $"Вольер {e.Number} заполнен ({count} из {e.Capacity})" : null;
    }
}