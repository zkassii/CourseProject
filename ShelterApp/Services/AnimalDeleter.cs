using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;

namespace ShelterApp.Services;

public static class AnimalDeleter
{
    // возвращает текст ошибки или null, если удаление прошло
    public static async Task<string?> DeleteAsync(ShelterDbContext db, string webRoot, int id)
    {
        var a = await db.Animals.FirstOrDefaultAsync(x => x.AnimalId == id);
        if (a == null) return "Животное не найдено";

        if (await db.VetRecords.AnyAsync(v => v.AnimalId == id) ||
            await db.AdoptionRequests.AnyAsync(r => r.AnimalId == id))
            return "Нельзя удалить: у животного есть ветеринарные записи или заявки";

        var photo = a.PhotoPath;
        db.Animals.Remove(a);
        await db.SaveChangesAsync();
        PhotoHelper.Delete(photo, webRoot);
        return null;
    }
}