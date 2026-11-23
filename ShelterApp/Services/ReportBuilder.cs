using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ShelterApp.Data;
using ShelterApp.Models;

namespace ShelterApp.Services;

public static class ReportBuilder
{
    public const string Movement = "movement";
    public const string Count = "count";
    public const string Vaccination = "vaccination";

    // виды отчётов для выпадающего списка
    public static readonly (string Value, string Text)[] Types =
    {
        (Movement, "Поступившие и выбывшие за период"),
        (Count, "Текущее количество животных"),
        (Vaccination, "Вакцинация животных за период")
    };

    public static async Task<byte[]> BuildAsync(ShelterDbContext db, string type, DateOnly from, DateOnly to)
    {
        using var wb = new XLWorkbook();
        switch (type)
        {
            case Movement: await MovementAsync(db, wb, from, to); break;
            case Count: await CountAsync(db, wb); break;
            default: await VaccinationAsync(db, wb, from, to); break;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ---------- 1. поступившие и выбывшие ----------
    private static async Task MovementAsync(ShelterDbContext db, XLWorkbook wb, DateOnly from, DateOnly to)
    {
        var period = $"с {from:dd.MM.yyyy} по {to:dd.MM.yyyy}";

        var intake = await db.Animals.AsNoTracking()
            .Where(a => a.IntakeDate >= from && a.IntakeDate <= to)
            .OrderBy(a => a.IntakeDate).ThenBy(a => a.Name)
            .ToListAsync();

        var ws = wb.Worksheets.Add("Поступившие");
        Title(ws, "Поступившие животные " + period);
        Header(ws, 3, "№", "Кличка", "Вид", "Порода", "Пол", "Дата поступления", "Статус");
        int r = 4, n = 1;
        foreach (var a in intake)
        {
            Put(ws, r, 1, n++); Put(ws, r, 2, a.Name); Put(ws, r, 3, a.Species); Put(ws, r, 4, a.Breed);
            Put(ws, r, 5, a.Gender); Put(ws, r, 6, a.IntakeDate); Put(ws, r, 7, a.Status);
            r++;
        }
        Total(ws, r, "Всего поступило", intake.Count);
        ws.Columns().AdjustToContents();

        var released = await db.Animals.AsNoTracking()
            .Where(a => a.ReleaseDate != null && a.ReleaseDate >= from && a.ReleaseDate <= to)
            .OrderBy(a => a.ReleaseDate).ThenBy(a => a.Name)
            .ToListAsync();

        // новый владелец берётся из заявки со статусом "Передано"
        var ids = released.Select(a => a.AnimalId).ToList();
        var transfers = await db.AdoptionRequests.AsNoTracking().Include(x => x.User)
            .Where(x => ids.Contains(x.AnimalId) && x.Status == RequestStatus.Transferred)
            .ToListAsync();
        var owner = transfers.GroupBy(x => x.AnimalId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.TransferDate).First().User?.FullName);

        var ws2 = wb.Worksheets.Add("Выбывшие");
        Title(ws2, "Выбывшие животные " + period);
        Header(ws2, 3, "№", "Кличка", "Вид", "Порода", "Дата выбытия", "Новый владелец");
        r = 4; n = 1;
        foreach (var a in released)
        {
            Put(ws2, r, 1, n++); Put(ws2, r, 2, a.Name); Put(ws2, r, 3, a.Species); Put(ws2, r, 4, a.Breed);
            Put(ws2, r, 5, a.ReleaseDate); Put(ws2, r, 6, owner.GetValueOrDefault(a.AnimalId));
            r++;
        }
        Total(ws2, r, "Всего выбыло", released.Count);
        ws2.Columns().AdjustToContents();
    }

    // ---------- 2. текущее количество ----------
    private static async Task CountAsync(ShelterDbContext db, XLWorkbook wb)
    {
        var alive = await db.Animals.AsNoTracking()
            .Where(a => a.Status != AnimalStatus.Adopted)
            .ToListAsync();

        var ws = wb.Worksheets.Add("Текущее количество");
        Title(ws, $"Животные в приюте на {DateTime.Today:dd.MM.yyyy}");

        Header(ws, 3, "Вид", "Количество");
        int r = 4;
        foreach (var g in alive.GroupBy(a => a.Species).OrderBy(g => g.Key))
        {
            Put(ws, r, 1, g.Key); Put(ws, r, 2, g.Count());
            r++;
        }
        Total(ws, r, "Всего", alive.Count, 1);

        r += 2;
        Header(ws, r, "Статус", "Количество");
        r++;
        foreach (var st in AnimalStatus.All.Where(s => s != AnimalStatus.Adopted))
        {
            Put(ws, r, 1, st); Put(ws, r, 2, alive.Count(a => a.Status == st));
            r++;
        }
        ws.Columns().AdjustToContents();
    }

    // ---------- 3. вакцинация ----------
    private static async Task VaccinationAsync(ShelterDbContext db, XLWorkbook wb, DateOnly from, DateOnly to)
    {
        var rows = await db.VetRecords.AsNoTracking()
            .Include(v => v.Animal).Include(v => v.User)
            .Where(v => v.RecordType == "Вакцинация" && v.RecordDate >= from && v.RecordDate <= to)
            .OrderBy(v => v.RecordDate).ThenBy(v => v.Animal!.Name)
            .ToListAsync();

        var ws = wb.Worksheets.Add("Вакцинация");
        Title(ws, $"Вакцинация животных с {from:dd.MM.yyyy} по {to:dd.MM.yyyy}");
        Header(ws, 3, "№", "Дата", "Кличка", "Вид", "Порода", "Ветеринар", "Примечание");
        int r = 4, n = 1;
        foreach (var v in rows)
        {
            Put(ws, r, 1, n++); Put(ws, r, 2, v.RecordDate); Put(ws, r, 3, v.Animal?.Name);
            Put(ws, r, 4, v.Animal?.Species); Put(ws, r, 5, v.Animal?.Breed);
            Put(ws, r, 6, v.User?.FullName); Put(ws, r, 7, v.Notes);
            r++;
        }
        Total(ws, r, "Всего вакцинаций", rows.Count);
        Total(ws, r + 1, "Привито животных", rows.Select(v => v.AnimalId).Distinct().Count());
        ws.Columns().AdjustToContents();
    }

    // ---------- оформление листа ----------
    private static void Title(IXLWorksheet ws, string text)
    {
        var c = ws.Cell(1, 1);
        c.Value = text;
        c.Style.Font.Bold = true;
        c.Style.Font.FontSize = 14;
    }

    private static void Header(IXLWorksheet ws, int row, params string[] heads)
    {
        for (int i = 0; i < heads.Length; i++)
        {
            var c = ws.Cell(row, i + 1);
            c.Value = heads[i];
            c.Style.Font.Bold = true;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml("#E9E2F5");
            c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }

    private static void Put(IXLWorksheet ws, int row, int col, object? value)
    {
        var c = ws.Cell(row, col);
        switch (value)
        {
            case null: c.Value = ""; break;
            case DateOnly d:
                c.Value = d.ToDateTime(TimeOnly.MinValue);
                c.Style.DateFormat.Format = "dd.MM.yyyy";
                break;
            case int i: c.Value = i; break;
            default: c.Value = value.ToString() ?? ""; break;
        }
        c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static void Total(IXLWorksheet ws, int row, string label, int count, int labelCol = 2)
    {
        ws.Cell(row, labelCol).Value = label;
        ws.Cell(row, labelCol).Style.Font.Bold = true;
        ws.Cell(row, labelCol + 1).Value = count;
        ws.Cell(row, labelCol + 1).Style.Font.Bold = true;
    }
}