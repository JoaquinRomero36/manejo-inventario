namespace InventarioAdmin.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int IdCategoria { get; set; }

    /// <summary>Baja lógica: los productos nunca se borran, solo se deshabilitan.</summary>
    public bool Activo { get; set; } = true;

    public Categoria? Categoria { get; set; }
    public Stock? Stock { get; set; }
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}