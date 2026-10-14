using InventarioAdmin.Data;
using InventarioAdmin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Pages;

public class HistorialModel : PageModel
{
    private readonly InventarioDbContext _db;

    public HistorialModel(InventarioDbContext db)
    {
        _db = db;
    }

    public List<MovimientoFila> Movimientos { get; set; } = new();

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