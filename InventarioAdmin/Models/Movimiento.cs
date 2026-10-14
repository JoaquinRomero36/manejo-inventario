namespace InventarioAdmin.Models;

public class Movimiento
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
    public TipoMovimiento Tipo { get; set; }

    /// <summary>Motivo del movimiento (venta, rotura, error de conteo, etc.).</summary>
    public string Descripcion { get; set; } = string.Empty;

    public int IdProducto { get; set; }
    public int IdUsuario { get; set; }

    public Producto? Producto { get; set; }
    public Usuario? Usuario { get; set; }
}