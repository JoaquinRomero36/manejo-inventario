using InventarioAdmin.Data;
using InventarioAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Pages;

public class CategoriasModel : PageModel
{
    private readonly InventarioDbContext _db;
    private readonly ILogger<CategoriasModel> _logger;

    public CategoriasModel(InventarioDbContext db, ILogger<CategoriasModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<CategoriaFila> Filas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Filas = await ObtenerFilasAsync();
    }

    public async Task<IActionResult> OnGetTablaAsync()
    {
        var filas = await ObtenerFilasAsync();
        return Partial("_CategoriasTable", filas);
    }

    public async Task<IActionResult> OnPostCrearAsync([FromBody] CrearCategoriaRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return new JsonResult(new { ok = false, mensaje = "La descripción es obligatoria." });
        }

        var descripcion = request.Descripcion.Trim();

        if (await _db.Categorias.AnyAsync(c => c.Descripcion == descripcion))
        {
            return new JsonResult(new { ok = false, mensaje = "Ya existe una categoría con esa descripción." });
        }

        _db.Categorias.Add(new Categoria { Descripcion = descripcion, Activo = true });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Categoría creada: {Descripcion}", descripcion);
        return new JsonResult(new { ok = true, mensaje = "Categoría creada correctamente." });
    }

    public async Task<IActionResult> OnPostDeshabilitarAsync([FromBody] IdRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Categoría inválida." });
        }

        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == request.Id);
        if (categoria is null)
        {
            return new JsonResult(new { ok = false, mensaje = "La categoría no existe." });
        }

        if (!categoria.Activo)
        {
            return new JsonResult(new { ok = false, mensaje = "La categoría ya está deshabilitada." });
        }

        // Solo se puede deshabilitar si no tiene productos activos.
        var tieneActivos = await _db.Productos.AnyAsync(p => p.IdCategoria == categoria.Id && p.Activo);
        if (tieneActivos)
        {
            return new JsonResult(new { ok = false, mensaje = "No se puede deshabilitar: la categoría tiene productos activos." });
        }

        categoria.Activo = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Categoría deshabilitada: {CategoriaId}", categoria.Id);
        return new JsonResult(new { ok = true, mensaje = "Categoría deshabilitada correctamente." });
    }

    public async Task<IActionResult> OnPostHabilitarAsync([FromBody] IdRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Categoría inválida." });
        }

        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == request.Id);
        if (categoria is null)
        {
            return new JsonResult(new { ok = false, mensaje = "La categoría no existe." });
        }

        if (categoria.Activo)
        {
            return new JsonResult(new { ok = false, mensaje = "La categoría ya está habilitada." });
        }

        categoria.Activo = true;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Categoría habilitada: {CategoriaId}", categoria.Id);
        return new JsonResult(new { ok = true, mensaje = "Categoría habilitada correctamente." });
    }

    private async Task<List<CategoriaFila>> ObtenerFilasAsync()
    {
        return await _db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Descripcion)
            .Select(c => new CategoriaFila
            {
                Id = c.Id,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                ProductosActivos = c.Productos.Count(p => p.Activo)
            })
            .ToListAsync();
    }
}

public class CategoriaFila
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int ProductosActivos { get; set; }
}

public class CrearCategoriaRequest
{
    public string Descripcion { get; set; } = string.Empty;
}