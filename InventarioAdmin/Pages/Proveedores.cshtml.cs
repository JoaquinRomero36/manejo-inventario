using InventarioAdmin.Data;
using InventarioAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Pages;

public class ProveedoresModel : PageModel
{
    private readonly InventarioDbContext _db;
    private readonly ILogger<ProveedoresModel> _logger;

    public ProveedoresModel(InventarioDbContext db, ILogger<ProveedoresModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<ProveedorFila> Filas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Filas = await ObtenerFilasAsync();
    }

    public async Task<IActionResult> OnGetTablaAsync()
    {
        var filas = await ObtenerFilasAsync();
        return Partial("_ProveedoresTable", filas);
    }

    public async Task<IActionResult> OnPostCrearAsync([FromBody] ProveedorRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Numero))
        {
            return new JsonResult(new { ok = false, mensaje = "El número es obligatorio." });
        }

        _db.Proveedores.Add(new Proveedor
        {
            Numero = request.Numero.Trim(),
            Gmail = request.Gmail?.Trim() ?? string.Empty,
            Descripcion = request.Descripcion?.Trim() ?? string.Empty
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Proveedor creado: {Numero}", request.Numero);
        return new JsonResult(new { ok = true, mensaje = "Proveedor creado correctamente." });
    }

    public async Task<IActionResult> OnPostEditarAsync([FromBody] ProveedorRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Proveedor inválido." });
        }

        if (string.IsNullOrWhiteSpace(request.Numero))
        {
            return new JsonResult(new { ok = false, mensaje = "El número es obligatorio." });
        }

        var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.Id == request.Id);
        if (proveedor is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El proveedor no existe." });
        }

        proveedor.Numero = request.Numero.Trim();
        proveedor.Gmail = request.Gmail?.Trim() ?? string.Empty;
        proveedor.Descripcion = request.Descripcion?.Trim() ?? string.Empty;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Proveedor modificado: {ProveedorId}", proveedor.Id);
        return new JsonResult(new { ok = true, mensaje = "Proveedor actualizado correctamente." });
    }

    public async Task<IActionResult> OnPostEliminarAsync([FromBody] IdRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Proveedor inválido." });
        }

        var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.Id == request.Id);
        if (proveedor is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El proveedor no existe." });
        }

        // Tabla independiente y sin relaciones: el borrado es físico.
        _db.Proveedores.Remove(proveedor);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Proveedor eliminado: {ProveedorId}", request.Id);
        return new JsonResult(new { ok = true, mensaje = "Proveedor eliminado correctamente." });
    }

    private async Task<List<ProveedorFila>> ObtenerFilasAsync()
    {
        return await _db.Proveedores
            .AsNoTracking()
            .OrderBy(p => p.Numero)
            .Select(p => new ProveedorFila
            {
                Id = p.Id,
                Numero = p.Numero,
                Gmail = p.Gmail,
                Descripcion = p.Descripcion
            })
            .ToListAsync();
    }
}

public class ProveedorFila
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Gmail { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class ProveedorRequest
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Gmail { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}