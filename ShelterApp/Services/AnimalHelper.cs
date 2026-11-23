using ShelterApp.Models;

namespace ShelterApp.Services;

public static class AnimalHelper
{
    // возраст словами: "5 мес.", "2 года"
    public static string AgeText(DateOnly? birth)
    {
        if (birth is null) return "возраст неизвестен";

        var today = DateOnly.FromDateTime(DateTime.Today);
        var b = birth.Value;
        int months = (today.Year - b.Year) * 12 + today.Month - b.Month;
        if (today.Day < b.Day) months--;
        if (months < 0) months = 0;

        if (months < 12) return months == 0 ? "меньше месяца" : $"{months} мес.";

        int years = months / 12;
        return $"{years} {YearWord(years)}";
    }

    private static string YearWord(int n)
    {
        int m100 = n % 100, m10 = n % 10;
        if (m100 is >= 11 and <= 14) return "лет";
        if (m10 == 1) return "год";
        if (m10 is >= 2 and <= 4) return "года";
        return "лет";
    }

    // цвет плашки статуса (классы из site.css)
    public static string StatusClass(string status) => status switch
    {
        AnimalStatus.Ready => "status-ok",
        AnimalStatus.Reserved => "status-pink",
        AnimalStatus.Quarantine => "status-warn",
        AnimalStatus.Treatment => "status-warn",
        _ => "status-ok"
    };
}