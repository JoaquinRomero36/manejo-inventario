using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using InventarioOperaciones.Data;
using InventarioOperaciones.Models;
using InventarioOperaciones.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioOperaciones.Pages;

public class LoginModel : PageModel
{
    private readonly InventarioDbContext _db;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(InventarioDbContext db, ILogger<LoginModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMensaje { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Productos");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == Input.Email);

        // Mensaje genérico: no se revela si el email existe.
        if (usuario is null || !KeyDerivation.VerifyPassword(Input.Password, usuario.Salt, usuario.PasswordHash))
        {
            ErrorMensaje = "Email o contraseña incorrectos.";
            _logger.LogWarning("Intento de login fallido para {Email}", Input.Email);
            return Page();
        }

        // Esta aplicación solo admite usuarios con rol normal.
        if (usuario.Rol != RolUsuario.normal)
        {
            ErrorMensaje = "Su usuario no tiene permisos para esta aplicación.";
            _logger.LogWarning("Login rechazado por rol incorrecto para {Email}", Input.Email);
            return Page();
        }

        var claims = new List<Claim>
        {
            new("id", usuario.Id.ToString()),
            new("nombre", usuario.Nombre),
            new("rol", usuario.Rol.ToString()),
            new(ClaimTypes.Name, usuario.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        _logger.LogInformation("Login correcto de {Email}", usuario.Email);
        return RedirectToPage("/Productos");
    }
}