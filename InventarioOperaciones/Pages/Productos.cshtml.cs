using InventarioOperaciones.Data;
using InventarioOperaciones.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioOperaciones.Pages;

public class ProductosModel : PageModel
{
    private readonly InventarioDbContext _db;
    private readonly ILogger<ProductosModel> _logger;

    public ProductosModel(InventarioDbContext db, ILogger<ProductosModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<ProductoFila> Filas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Filas = await ObtenerFilasAsync();
    }

    /// <summary>Devuelve solo el parcial de la tabla (usado para refrescar por AJAX).</summary>
    public async Task<IActionResult> OnGetTablaAsync()
    {
        var filas = await ObtenerFilasAsync();
        return Partial("_ProductsTable", filas);
    }

    public async Task<IActionResult> OnPostAgregarAsync([FromBody] MovimientoRequest request)
    {
        return await EjecutarMovimientoAsync(request, TipoMovimiento.agregar);
    }

    public async Task<IActionResult> OnPostSacarAsync([FromBody] MovimientoRequest request)
    {
        return await EjecutarMovimientoAsync(request, TipoMovimiento.sacar);
    }

    private async Task<IActionResult> EjecutarMovimientoAsync(MovimientoRequest request, TipoMovimiento tipo)
    {
        if (request is null || request.IdProducto <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "Producto inválido." });
        }

        // X debe ser un entero mayor a 0.
        if (request.Cantidad <= 0)
        {
            return new JsonResult(new { ok = false, mensaje = "La cantidad debe ser un número entero mayor a 0." });
        }

        if (string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return new JsonResult(new { ok = false, mensaje = "La descripción (motivo) es obligatoria." });
        }

        var idUsuario = await ObtenerUsuarioActualIdAsync();
        if (idUsuario is null)
        {
            return new JsonResult(new { ok = false, mensaje = "No hay un usuario operador configurado en la base." });
        }

        var producto = await _db.Productos
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Id == request.IdProducto);

        // 1. El producto debe existir y estar activo.
        if (producto is null || !producto.Activo)
        {
            return new JsonResult(new { ok = false, mensaje = "El producto no existe o está inactivo." });
        }

        if (producto.Stock is null)
        {
            return new JsonResult(new { ok = false, mensaje = "El producto no tiene stock registrado." });
        }

        var stock = producto.Stock;

        if (tipo == TipoMovimiento.sacar)
        {
            // 2. Debe haber stock suficiente.
            if (stock.CantidadDisponible < request.Cantidad)
            {
                return new JsonResult(new { ok = false, mensaje = $"Sin stock suficiente. Disponible: {stock.CantidadDisponible}." });
            }

            // 3. Registrar el movimiento.
            _db.Movimientos.Add(new Movimiento
            {
                Fecha = DateTime.Now,
                Cantidad = request.Cantidad,
                Tipo = TipoMovimiento.sacar,
                Descripcion = request.Descripcion.Trim(),
                IdProducto = producto.Id,
                IdUsuario = idUsuario.Value
            });

            // 4. Descontar.
            stock.CantidadDisponible -= request.Cantidad;

            // 5. Activar alerta si quedó bajo el mínimo.
            if (stock.CantidadDisponible < stock.CantidadMinima)
            {
                stock.AlertaActiva = true;
            }
        }
        else
        {
            // 2. Registrar el movimiento.
            _db.Movimientos.Add(new Movimiento
            {
                Fecha = DateTime.Now,
                Cantidad = request.Cantidad,
                Tipo = TipoMovimiento.agregar,
                Descripcion = request.Descripcion.Trim(),
                IdProducto = producto.Id,
                IdUsuario = idUsuario.Value
            });

            // 3. Sumar.
            stock.CantidadDisponible += request.Cantidad;

            // 4. Quitar la alerta si ya se alcanzó el mínimo.
            if (stock.AlertaActiva && stock.CantidadDisponible >= stock.CantidadMinima)
            {
                stock.AlertaActiva = false;
            }
        }

        // Movimiento + stock en una sola transacción.
        await _db.SaveChangesAsync();

        _logger.LogInformation("Movimiento {Tipo} de {Cantidad} sobre producto {ProductoId} por usuario {UsuarioId}",
            tipo, request.Cantidad, producto.Id, idUsuario.Value);

        var mensaje = tipo == TipoMovimiento.sacar
            ? "Stock descontado correctamente."
            : "Stock agregado correctamente.";

        return new JsonResult(new { ok = true, mensaje });
    }

    private async Task<List<ProductoFila>> ObtenerFilasAsync()
    {
        return await _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Stock)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoFila
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

    /// <summary>
    /// Sin login: los movimientos se atribuyen al usuario operador de la semilla
    /// (el primer usuario con rol normal, que es el creado por el DbInitializer).
    /// </summary>
    private async Task<int?> ObtenerUsuarioActualIdAsync()
    {
        return await _db.Usuarios
            .Where(u => u.Rol == RolUsuario.normal)
            .OrderBy(u => u.Id)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();
    }
}

/// <summary>Fila para el listado de productos.</summary>
public class ProductoFila
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int CantidadDisponible { get; set; }
    public int CantidadMinima { get; set; }
    public bool AlertaActiva { get; set; }
}

/// <summary>Cuerpo JSON para agregar/sacar stock.</summary>
public class MovimientoRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}