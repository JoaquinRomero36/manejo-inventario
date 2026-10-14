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