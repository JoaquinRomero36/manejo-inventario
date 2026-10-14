using InventarioAdmin.Data;
using InventarioAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Pages;

public class ProductosModel : PageModel
{
    private readonly InventarioDbContext _db;
    private readonly ILogger<ProductosModel> _logger;

    public ProductosModel(InventarioDbContext db, ILogger<ProductosModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<ProductoAdminFila> Filas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Filas = await ObtenerFilasAsync();
    }

    public async Task<IActionResult> OnGetTablaAsync()
    {
        var filas = await ObtenerFilasAsync();
        return Partial("_ProductosTable", filas);
    }

    /// <summary>Categorías activas para el Select2 del alta.</summary>
    public async Task<IActionResult> OnGetCategoriasAsync()
    {
        var categorias = await _db.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Descripcion)
            .Select(c => new { id = c.Id, texto = c.Descripcion })
            .ToListAsync();

        return new JsonResult(categorias);
    }

    public async Task<IActionResult> OnPostCrearAsync([FromBody] CrearProductoRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Nombre))
        {
            return new JsonResult(new { ok = false, mensaje = "El nombre del producto es obligatorio." });
        }

        if (request.CantidadMinima < 0)
        {
            return new JsonResult(new { ok = false, mensaje = "La cantidad mínima no puede ser negativa." });
        }

        // La categoría debe existir y estar activa.
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == request.IdCategoria);
        if (categoria is null || !categoria.Activo)
        {
            return new JsonResult(new { ok = false, mensaje = "La categoría no existe o está inactiva." });
        }

        // Crear el producto crea automáticamente su fila de Stock.
        var producto = new Producto
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim() ?? string.Empty,
            IdCategoria = categoria.Id,
            Activo = true,
            Stock = new Stock
            {
                CantidadDisponible = 0,
                CantidadMinima = request.CantidadMinima,
                // El alta arranca con stock 0: si el mínimo es mayor a 0, la alerta ya nace activa.
                AlertaActiva = 0 < request.CantidadMinima
            }
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Producto creado: {ProductoId} - {Nombre}", producto.Id, producto.Nombre);
        return new JsonResult(new { ok = true, mensaje = "Producto creado correctamente." });
    }

    public async Task<IActionResult> OnPostDeshabilitarAsync([FromBody] IdRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Producto inválido." });
        }

        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == request.Id);
        if (producto is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El producto no existe." });
        }

        if (!producto.Activo)
        {
            return new JsonResult(new { ok = false, mensaje = "El producto ya está deshabilitado." });
        }

        // Baja lógica: nunca se borra.
        producto.Activo = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Producto deshabilitado: {ProductoId}", producto.Id);
        return new JsonResult(new { ok = true, mensaje = "Producto deshabilitado correctamente." });
    }

    public async Task<IActionResult> OnPostHabilitarAsync([FromBody] IdRequest request)
    {
        if (request is null || request.Id <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Producto inválido." });
        }

        var producto = await _db.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == request.Id);
        if (producto is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El producto no existe." });
        }

        if (producto.Activo)
        {
            return new JsonResult(new { ok = false, mensaje = "El producto ya está habilitado." });
        }

        // Un producto activo siempre debe tener su categoría activa.
        if (producto.Categoria is null || !producto.Categoria.Activo)
        {
            return new JsonResult(new
            {
                ok = false,
                mensaje = "La categoría del producto está deshabilitada. Habilite la categoría primero."
            });
        }

        // Revertir la baja lógica.
        producto.Activo = true;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Producto habilitado: {ProductoId}", producto.Id);
        return new JsonResult(new { ok = true, mensaje = "Producto habilitado correctamente." });
    }

    private async Task<List<ProductoAdminFila>> ObtenerFilasAsync()
    {
        return await _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Stock)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoAdminFila
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Categoria = p.Categoria != null ? p.Categoria.Descripcion : "-",
                Activo = p.Activo,
                CantidadDisponible = p.Stock != null ? p.Stock.CantidadDisponible : 0,
                CantidadMinima = p.Stock != null ? p.Stock.CantidadMinima : 0,
                AlertaActiva = p.Stock != null && p.Stock.AlertaActiva
            })
            .ToListAsync();
    }
}

public class ProductoAdminFila
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int CantidadDisponible { get; set; }
    public int CantidadMinima { get; set; }
    public bool AlertaActiva { get; set; }
}

public class CrearProductoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public int CantidadMinima { get; set; }
}

public class IdRequest
{
    public int Id { get; set; }
}