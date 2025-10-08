using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdminHost
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // ✅ Сервисы
      builder.Services.AddControllers();
      builder.Services.AddDistributedMemoryCache();
      builder.Services.AddSession();

      var app = builder.Build();

      // ✅ Подключаем статику, маршруты и сессии
      app.UseStaticFiles();

      app.MapGet("/test-css", async context =>
      {
        var exists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css/style.css"));
        await context.Response.WriteAsync(exists ? "✅ CSS найден" : "❌ CSS не найден");
      });

      app.UseRouting();
      app.UseSession();

      // ✅ Подключаем контроллеры
      app.MapControllers();

      // ✅ Редирект с корня на страницу входа
      app.MapGet("/", context =>
      {
        context.Response.Redirect("/html/login.html");
        return Task.CompletedTask;
      });

      // ✅ Короткие пути
      app.MapGet("/login", context =>
      {
        context.Response.Redirect("/html/login.html");
        return Task.CompletedTask;
      });

      app.MapGet("/home", async context =>
      {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/html/index.html");
      });

      // === Проверка структуры wwwroot ===
      var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
      Console.WriteLine($"\n📁 Проверка структуры wwwroot: {rootPath}\n");

      if (!Directory.Exists(rootPath))
      {
        Console.WriteLine("❌ Папка wwwroot не найдена!");
      }
      else
      {
        var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
        foreach (var file in allFiles)
          Console.WriteLine("✅ " + file.Replace(rootPath, "").TrimStart('\\', '/'));

        Console.WriteLine("\n--- Проверяем конкретные файлы ---");

        string cssPath = Path.Combine(rootPath, "css", "style.css");
        string htmlPath = Path.Combine(rootPath, "html", "login.html");

        Console.WriteLine(File.Exists(cssPath)
            ? $"✅ CSS найден: {cssPath}"
            : $"❌ CSS отсутствует по пути: {cssPath}");

        Console.WriteLine(File.Exists(htmlPath)
            ? $"✅ HTML найден: {htmlPath}"
            : $"❌ HTML отсутствует по пути: {htmlPath}");
      }

      // === Проверка доступности CSS по HTTP ===
      app.MapGet("/debug-css", async context =>
      {
        var cssPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css/style.css");
        bool exists = File.Exists(cssPath);

        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(exists
            ? $"✅ CSS существует по пути {cssPath}\n\nПопробуй открыть: http://localhost:{context.Request.Host.Port}/css/style.css"
            : $"❌ CSS файл не найден по пути {cssPath}");
      });


      // ✅ Запуск приложения
      app.Run();
    }
  }
}
