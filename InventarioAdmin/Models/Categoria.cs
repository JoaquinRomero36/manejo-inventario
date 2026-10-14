namespace InventarioAdmin.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Baja lógica: las categorías nunca se borran, solo se deshabilitan.</summary>
    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}