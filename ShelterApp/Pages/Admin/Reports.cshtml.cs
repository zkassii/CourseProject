using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShelterApp.Data;
using ShelterApp.Models;
using ShelterApp.Services;

namespace ShelterApp.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class ReportsModel : PageModel
{
    private readonly ShelterDbContext _db;
    public ReportsModel(ShelterDbContext db) => _db = db;

    [BindProperty] public string ReportType { get; set; } = ReportBuilder.Movement;
    [BindProperty] public DateOnly? From { get; set; }
    [BindProperty] public DateOnly? To { get; set; }

    public void OnGet()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        From = new DateOnly(today.Year, today.Month, 1);   // с начала месяца
        To = today;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ReportBuilder.Types.Any(t => t.Value == ReportType))
            ModelState.AddModelError("ReportType", "Выберите вид отчета");

        // для отчета "Текущее количество" период не нужен
        if (ReportType != ReportBuilder.Count)
        {
            if (From == null) ModelState.AddModelError("From", "Укажите начало периода");
            if (To == null) ModelState.AddModelError("To", "Укажите конец периода");
            if (From != null && To != null && From > To)
                ModelState.AddModelError("To", "Начало периода не может быть позже конца");
        }

        if (!ModelState.IsValid) return Page();

        var from = From ?? DateOnly.FromDateTime(DateTime.Today);
        var to = To ?? DateOnly.FromDateTime(DateTime.Today);

        var bytes = await ReportBuilder.BuildAsync(_db, ReportType, from, to);
        var name = $"report_{ReportType}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
    }
}