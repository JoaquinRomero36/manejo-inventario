namespace InventarioAdmin.Models;

public class Stock
{
    public int Id { get; set; }
    public int CantidadDisponible { get; set; }
    public int CantidadMinima { get; set; }
    public bool AlertaActiva { get; set; }
    public int IdProducto { get; set; }

    public Producto? Producto { get; set; }
}