namespace ShelterApp.Services;

public static class PhotoHelper
{
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxSize = 5 * 1024 * 1024; // 5 МБ

    public static string? Validate(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return "Допустимы только файлы JPG, PNG и WEBP";
        if (file.Length > MaxSize) return "Размер файла не должен превышать 5 МБ";
        return null;
    }

    // сохраняет файл в wwwroot/uploads и возвращает путь для базы данных
    public static async Task<string> SaveAsync(IFormFile file, string webRoot)
    {
        var dir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(dir);
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        using var fs = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(fs);
        return "/uploads/" + name;
    }

    public static void Delete(string? path, string webRoot)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/")) return;
        var full = Path.Combine(webRoot, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(full)) File.Delete(full);
    }
}