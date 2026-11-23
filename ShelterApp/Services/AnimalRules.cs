using ShelterApp.Models;

namespace ShelterApp.Services;

public static class AnimalRules
{
    // правила, которые зависят от статуса
    public static void Apply(Animal a)
    {
        if (a.Status == AnimalStatus.Adopted)
        {
            a.ReleaseDate ??= DateOnly.FromDateTime(DateTime.Today); // дата выбытия
            a.EnclosureId = null;                                     // вольер освобождается
        }
        else
        {
            a.ReleaseDate = null;
        }
    }
}