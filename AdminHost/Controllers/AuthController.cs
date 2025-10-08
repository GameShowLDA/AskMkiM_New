using Microsoft.AspNetCore.Mvc;

namespace AdminHost.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public class AuthController : ControllerBase
  {
    private static string ValidLogin = "admin";
    private static string ValidPassword = "admin";

    /// <summary>
    /// Проверка логина и пароля.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
      if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        return BadRequest(new { success = false, message = "Введите логин и пароль" });

      if (model.Username == ValidLogin && model.Password == ValidPassword)
      {
        // создаём сессию
        HttpContext.Session.SetString("IsAuthorized", "true");
        return Ok(new { success = true });
      }

      return Unauthorized(new { success = false, message = "Неверный логин или пароль" });
    }

    /// <summary>
    /// Проверка авторизации (используется фронтом для редиректа).
    /// </summary>
    [HttpGet("check")]
    public IActionResult CheckAuth()
    {
      var isAuth = HttpContext.Session.GetString("IsAuthorized");
      return Ok(new { authorized = isAuth == "true" });
    }

    /// <summary>
    /// Выход из системы.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
      HttpContext.Session.Remove("IsAuthorized");
      return Ok();
    }
  }

  public class LoginModel
  {
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
  }
}
