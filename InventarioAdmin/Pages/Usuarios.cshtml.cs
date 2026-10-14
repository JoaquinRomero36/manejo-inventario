using System.ComponentModel.DataAnnotations;
using InventarioAdmin.Data;
using InventarioAdmin.Models;
using InventarioAdmin.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Pages;

public class UsuariosModel : PageModel
{
    private readonly InventarioDbContext _db;
    private readonly ILogger<UsuariosModel> _logger;

    public UsuariosModel(InventarioDbContext db, ILogger<UsuariosModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<UsuarioFila> Filas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Filas = await ObtenerFilasAsync();
    }

    public async Task<IActionResult> OnGetTablaAsync()
    {
        var filas = await ObtenerFilasAsync();
        return Partial("_UsuariosTable", filas);
    }

    public async Task<IActionResult> OnPostCrearAsync([FromBody] CrearUsuarioRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { ok = false, mensaje = "Datos inválidos." });
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return new JsonResult(new { ok = false, mensaje = "El nombre es obligatorio." });
        }

        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        // Validación de formato de email.
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return new JsonResult(new { ok = false, mensaje = "El email no es válido." });
        }

        // El email es único.
        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
        {
            return new JsonResult(new { ok = false, mensaje = "Ya existe un usuario con ese email." });
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
        {
            return new JsonResult(new { ok = false, mensaje = "La contraseña debe tener al menos 6 caracteres." });
        }

        // El rol debe ser normal o admin.
        if (!Enum.TryParse<RolUsuario>(request.Rol, ignoreCase: true, out var rol))
        {
            return new JsonResult(new { ok = false, mensaje = "El rol indicado no es válido." });
        }

        var salt = KeyDerivation.GenerateSalt();
        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Email = email,
            Rol = rol,
            Salt = salt,
            PasswordHash = KeyDerivation.HashPassword(request.Password, salt)
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Usuario creado: {UsuarioId} ({Email}) rol {Rol}", usuario.Id, usuario.Email, rol);
        return new JsonResult(new { ok = true, mensaje = "Usuario creado correctamente." });
    }

    public async Task<IActionResult> OnPostCambiarPasswordAsync([FromBody] CambiarPasswordRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Usuario inválido." });
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
        {
            return new JsonResult(new { ok = false, mensaje = "La contraseña debe tener al menos 6 caracteres." });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == request.Id);
        if (usuario is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El usuario no existe." });
        }

        // Nueva sal en cada cambio de contraseña.
        usuario.Salt = KeyDerivation.GenerateSalt();
        usuario.PasswordHash = KeyDerivation.HashPassword(request.Password, usuario.Salt);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Contraseña cambiada para el usuario {UsuarioId}", usuario.Id);
        return new JsonResult(new { ok = true, mensaje = "Contraseña actualizada correctamente." });
    }

    public async Task<IActionResult> OnPostDeshabilitarAsync([FromBody] IdRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Usuario inválido." });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == request.Id);
        if (usuario is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El usuario no existe." });
        }

        // No se puede eliminar al último admin.
        if (usuario.Rol == RolUsuario.admin)
        {
            var admins = await _db.Usuarios.CountAsync(u => u.Rol == RolUsuario.admin);
            if (admins <= 1)
            {
                return new JsonResult(new { ok = false, mensaje = "No se puede eliminar al último administrador." });
            }
        }

        // No se puede eliminar un usuario que tenga movimientos registrados (integridad referencial).
        var tieneMovimientos = await _db.Movimientos.AnyAsync(m => m.IdUsuario == usuario.Id);
        if (tieneMovimientos)
        {
            return new JsonResult(new { ok = false, mensaje = "No se puede deshabilitar: el usuario tiene movimientos registrados." });
        }

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Usuario deshabilitado/eliminado: {UsuarioId}", usuario.Id);
        return new JsonResult(new { ok = true, mensaje = "Usuario deshabilitado correctamente." });
    }

    private async Task<List<UsuarioFila>> ObtenerFilasAsync()
    {
        return await _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioFila
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol == RolUsuario.admin ? "admin" : "normal"
            })
            .ToListAsync();
    }
}

public class UsuarioFila
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class CrearUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CambiarPasswordRequest
{
    public int Id { get; set; }
    public string Password { get; set; } = string.Empty;
}