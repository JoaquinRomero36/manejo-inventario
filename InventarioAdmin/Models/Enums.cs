namespace InventarioAdmin.Models;

/// <summary>Tipo de movimiento de stock. Los nombres coinciden con los valores persistidos.</summary>
public enum TipoMovimiento
{
    agregar = 0,
    sacar = 1
}

/// <summary>Rol del usuario. Los nombres coinciden con los valores persistidos.</summary>
public enum RolUsuario
{
    normal = 0,
    admin = 1
}