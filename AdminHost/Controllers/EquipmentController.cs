using System.Reflection;
using AdminHost.Models.Interface;
using Microsoft.AspNetCore.Mvc;
using NewCore.Base.Interface.Main;
using DataBaseConfiguration;
using DataBaseConfiguration.Services.Device;

namespace AdminHost.Controllers
{
  [Route("api/equipment")]
  [ApiController]
  public class EquipmentController : ControllerBase
  {
    /// <summary>
    /// Возвращает список всех устройств и их категорий.
    /// </summary>
    [HttpGet("devices")]
    public IActionResult GetDevices()
    {
      var context = DataBaseConfig.Context;

      // Таблицы, которые содержат устройства
      var entitySets = new (string Name, IQueryable<object> Query)[]
      {
                ("ChassisManagers", context.ChassisManagers),
                ("RelaySwitchModules", context.RelaySwitchModules),
                ("PowerSourceModules", context.PowerSourceModules),
                ("SwitchingDevices", context.SwitchingDevices),
                ("PrecisionMeters", context.PrecisionMeters),
                ("FastMeters", context.FastMeters),
                ("BreakdownTesters", context.BreakdownTesters),
                ("Rack", context.Rack)
      };

      var categories = entitySets
          .Select(e => new { category = e.Name })
          .ToList();

      var devices = new List<object>();

      foreach (var set in entitySets)
      {
        foreach (dynamic entity in set.Query)
        {
          devices.Add(new
          {
            key = $"{set.Name}_{entity.Id}",
            name = entity.Name ?? "Без названия",
            category = set.Name,
            interfaceType = entity.DeviceClass ?? ""
          });
        }
      }

      return Ok(new { categories, devices });
    }

    /// <summary>
    /// Возвращает полные данные об устройстве и список режимов.
    /// </summary>
    [HttpGet("device/full/{deviceKey}")]
    public IActionResult GetFullDevice(string deviceKey)
    {
      if (string.IsNullOrWhiteSpace(deviceKey))
        return BadRequest(new { message = "Ключ устройства не задан." });

      var parts = deviceKey.Split('_', 2);
      if (parts.Length != 2)
        return BadRequest(new { message = "Некорректный формат ключа устройства." });

      string categoryName = parts[0];
      if (!int.TryParse(parts[1], out int id))
        return BadRequest(new { message = "Некорректный идентификатор устройства." });

      var services = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
      {
        ["BreakdownTesters"] = new BreakdownTesterServices(),
        ["ChassisManagers"] = new ChassisManagerServices(),
        ["FastMeters"] = new FastMeterServices(),
        ["PrecisionMeters"] = new PrecisionMeterServices(),
        ["PowerSourceModules"] = new PowerSourceModuleServices(),
        ["RelaySwitchModules"] = new RelaySwitchModuleServices(),
        ["SwitchingDevices"] = new SwitchingDeviceServices(),
        ["Rack"] = new RackServices()
      };

      if (!services.TryGetValue(categoryName, out var service))
        return NotFound(new { message = $"Категория '{categoryName}' не поддерживается." });

      object? entity = service switch
      {
        BreakdownTesterServices s => s.GetById(id),
        ChassisManagerServices s => s.GetById(id),
        FastMeterServices s => s.GetById(id),
        PrecisionMeterServices s => s.GetById(id),
        PowerSourceModuleServices s => s.GetById(id),
        RelaySwitchModuleServices s => s.GetById(id),
        SwitchingDeviceServices s => s.GetById(id),
        RackServices s => s.GetById(id),
        _ => null
      };

      if (entity == null)
        return NotFound(new { message = $"Устройство с Id={id} не найдено." });

      var deviceClassName = entity.GetType().GetProperty("DeviceClass")?.GetValue(entity)?.ToString();
      if (string.IsNullOrWhiteSpace(deviceClassName))
        return Ok(new { device = entity, modes = Array.Empty<object>() });

      // Ищем тип класса устройства
      Type? deviceClassType = AppDomain.CurrentDomain
          .GetAssemblies()
          .Select(a => a.GetType(deviceClassName))
          .FirstOrDefault(t => t != null);

      if (deviceClassType == null)
        return Ok(new
        {
          device = entity,
          modes = new[] {
                    new { Name = "Ошибка", Description = $"Тип {deviceClassName} не найден." }
                }
        });

      // Ищем интерфейс, реализуемый этим классом
      Type? deviceInterface = deviceClassType
          .GetInterfaces()
          .FirstOrDefault(i =>
              i.Namespace?.Contains("NewCore.Base.Interface.Main", StringComparison.OrdinalIgnoreCase) == true);

      if (deviceInterface == null)
        return Ok(new
        {
          device = entity,
          modes = new[] {
                    new { Name = "Режимы будут добавлены позже", Description = "Интерфейс устройства не найден." }
                }
        });

      // Находим режимы
      var asm = Assembly.GetExecutingAssembly();
      var modes = asm.GetTypes()
          .Where(t => typeof(IDeviceMode).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
          .Select(t => (IDeviceMode)Activator.CreateInstance(t)!)
          .Where(m => m.DeviceInterface == deviceInterface)
          .Select(m => new { m.Name, m.Description })
          .ToList();

      if (!modes.Any())
        modes.Add(new { Name = "Режимы будут добавлены позже", Description = "Для данного устройства пока не определено ни одного режима." });

      var entityType = entity.GetType();
      var dto = new
      {
        Id = entityType.GetProperty("Id")?.GetValue(entity),
        Name = entityType.GetProperty("Name")?.GetValue(entity),
        Description = entityType.GetProperty("Description")?.GetValue(entity),
        DeviceClass = entityType.GetProperty("DeviceClass")?.GetValue(entity),
        ConnectionDetails = entityType.GetProperty("ConnectionDetails")?.GetValue(entity)
      };

      return Ok(new
      {
        device = new
        {
          entity = dto,
          __category = categoryName
        },
        modes
      });
    }

    /// <summary>
    /// Заглушка для панели управления.
    /// </summary>
    [HttpGet("control/{deviceCategory}/{modeName}")]
    public IActionResult GetControlPanel(string deviceCategory, string modeName)
    {
      return Ok(new
      {
        title = $"Режим {modeName}",
        description = $"Панель управления для категории {deviceCategory}.",
        buttons = new[] { "Пуск", "Стоп", "Обновить" }
      });
    }
  }
}
