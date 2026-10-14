namespace InventarioAdmin.Models;

/// <summary>Tabla independiente, sin relaciones. Solo la usa la App Admin.</summary>
public class Proveedor
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Gmail { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}