using InventarioOperaciones.Data;
using InventarioOperaciones.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioOperaciones.Pages;

public class HistorialModel : PageModel
{
    private const int CantidadCercanosAlMinimo = 3;

    private readonly InventarioDbContext _db;

    public HistorialModel(InventarioDbContext db)
    {
        _db = db;
    }

    public List<MovimientoFila> Movimientos { get; set; } = new();

    /// <summary>Productos activos con la alerta de stock activa (disponible por debajo del mínimo).</summary>
    public List<ProductoAlertaFila> Alertas { get; set; } = new();

    /// <summary>
    /// Productos activos sin alerta, ordenados por menor margen sobre el mínimo.
    /// Se usan cuando no hay ninguna alerta para avisar cuáles están más cerca de dispararla.
    /// </summary>
    public List<ProductoAlertaFila> CercanosAlMinimo { get; set; } = new();

    public bool HayAlertas => Alertas.Count > 0;

    public async Task OnGetAsync()
    {
        Movimientos = await _db.Movimientos
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.Fecha)
            .Select(m => new MovimientoFila
            {
                Fecha = m.Fecha,
                Producto = m.Producto != null ? m.Producto.Nombre : "-",
                Tipo = m.Tipo == TipoMovimiento.agregar ? "Agregar" : "Sacar",
                Cantidad = m.Cantidad,
                Descripcion = m.Descripcion,
                Usuario = m.Usuario != null ? m.Usuario.Nombre : "-"
            })
            .ToListAsync();

        var productosConStock = await _db.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .Select(p => new ProductoAlertaFila
            {
                Id = p.Id,
                Nombre = p.Nombre,
                CantidadDisponible = p.Stock != null ? p.Stock.CantidadDisponible : 0,
                CantidadMinima = p.Stock != null ? p.Stock.CantidadMinima : 0,
                AlertaActiva = p.Stock != null && p.Stock.AlertaActiva
            })
            .ToListAsync();

        Alertas = productosConStock
            .Where(p => p.AlertaActiva)
            .OrderBy(p => p.Margen)
            .ThenBy(p => p.Nombre)
            .ToList();

        CercanosAlMinimo = productosConStock
            .Where(p => !p.AlertaActiva)
            .OrderBy(p => p.Margen)
            .ThenBy(p => p.Nombre)
            .Take(CantidadCercanosAlMinimo)
            .ToList();
    }
}

public class MovimientoFila
{
    public DateTime Fecha { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
}

/// <summary>Producto con su stock, usado por el banner de alertas del historial.</summary>
public class ProductoAlertaFila
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public int CantidadMinima { get; set; }
    public bool AlertaActiva { get; set; }

    /// <summary>Cuánto está por encima (o por debajo) del mínimo.</summary>
    public int Margen => CantidadDisponible - CantidadMinima;
}
