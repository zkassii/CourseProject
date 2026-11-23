namespace ShelterApp.Models;

public static class AnimalStatus
{
    public const string Quarantine = "На карантине";
    public const string Treatment = "На лечении";
    public const string Ready = "Готов к пристройству";
    public const string Reserved = "Забронирован";
    public const string Adopted = "Пристроен";

    public static readonly string[] All = { Quarantine, Treatment, Ready, Reserved, Adopted };
}