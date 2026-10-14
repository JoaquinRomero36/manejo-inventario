using InventarioAdmin.Data;
using InventarioAdmin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Pages;

public class DashboardModel : PageModel
{
    private readonly InventarioDbContext _db;

    public DashboardModel(InventarioDbContext db)
    {
        _db = db;
    }

    public List<ProductoAlerta> ProductosConAlerta { get; set; } = new();
    public List<MovimientoResumen> UltimosMovimientos { get; set; } = new();

    // KPIs
    public int TotalProductosActivos { get; set; }
    public int TotalCategoriasActivas { get; set; }
    public int ProductosConAlertaCount { get; set; }
    public int ProductosActivosSinStock { get; set; }
    public int TotalUnidadesStock { get; set; }
    public int MovimientosHoy { get; set; }

    public async Task OnGetAsync()
    {
        ProductosConAlerta = await _db.Stocks
            .AsNoTracking()
            .Where(s => s.AlertaActiva)
            .Include(s => s.Producto)
                .ThenInclude(p => p!.Categoria)
            .OrderBy(s => s.CantidadDisponible)
            .Select(s => new ProductoAlerta
            {
                Producto = s.Producto != null ? s.Producto.Nombre : "-",
                Categoria = s.Producto != null && s.Producto.Categoria != null ? s.Producto.Categoria.Descripcion : "-",
                CantidadDisponible = s.CantidadDisponible,
                CantidadMinima = s.CantidadMinima
            })
            .ToListAsync();

        ProductosConAlertaCount = ProductosConAlerta.Count;

        UltimosMovimientos = await _db.Movimientos
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.Fecha)
            .Take(15)
            .Select(m => new MovimientoResumen
            {
                Fecha = m.Fecha,
                Producto = m.Producto != null ? m.Producto.Nombre : "-",
                Tipo = m.Tipo == TipoMovimiento.agregar ? "Agregar" : "Sacar",
                Cantidad = m.Cantidad,
                Descripcion = m.Descripcion,
                Usuario = m.Usuario != null ? m.Usuario.Nombre : "-"
            })
            .ToListAsync();

        var hoy = DateTime.Today;

        TotalProductosActivos = await _db.Productos.CountAsync(p => p.Activo);
        TotalCategoriasActivas = await _db.Categorias.CountAsync(c => c.Activo);
        ProductosActivosSinStock = await _db.Stocks
            .AsNoTracking()
            .Include(s => s.Producto)
            .CountAsync(s => s.Producto != null && s.Producto.Activo && s.CantidadDisponible == 0);
        TotalUnidadesStock = await _db.Stocks
            .AsNoTracking()
            .Include(s => s.Producto)
            .Where(s => s.Producto != null && s.Producto.Activo)
            .SumAsync(s => s.CantidadDisponible);
        MovimientosHoy = await _db.Movimientos.CountAsync(m => m.Fecha >= hoy);
    }
}

public class ProductoAlerta
{
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public int CantidadMinima { get; set; }
}

public class MovimientoResumen
{
    public DateTime Fecha { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
}