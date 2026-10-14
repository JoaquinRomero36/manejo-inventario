using InventarioAdmin.Models;
using InventarioAdmin.Security;
using Microsoft.EntityFrameworkCore;

namespace InventarioAdmin.Data;

/// <summary>
/// Seed de la base. Solo lo ejecuta la App Admin y únicamente cuando la base está vacía.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(InventarioDbContext db, ILogger logger)
    {
        // Si ya hay usuarios, se asume base inicializada.
        if (await db.Usuarios.AnyAsync())
        {
            return;
        }

        logger.LogInformation("Base vacía: ejecutando seed inicial.");

        var saltAdmin = KeyDerivation.GenerateSalt();
        var admin = new Usuario
        {
            Nombre = "Administrador",
            Email = "admin@inventario.local",
            Rol = RolUsuario.admin,
            Salt = saltAdmin,
            PasswordHash = KeyDerivation.HashPassword("Admin123!", saltAdmin)
        };

        var saltOperador = KeyDerivation.GenerateSalt();
        var operador = new Usuario
        {
            Nombre = "Operador",
            Email = "operador@inventario.local",
            Rol = RolUsuario.normal,
            Salt = saltOperador,
            PasswordHash = KeyDerivation.HashPassword("Operador123!", saltOperador)
        };

        db.Usuarios.AddRange(admin, operador);

        var categorias = new List<Categoria>
        {
            new() { Descripcion = "Bebidas", Activo = true },
            new() { Descripcion = "Almacén", Activo = true },
            new() { Descripcion = "Limpieza", Activo = true }
        };
        db.Categorias.AddRange(categorias);

        // Productos con su Stock (cantidad_disponible = 0, mínimo indicado).
        db.Productos.AddRange(
            new Producto
            {
                Nombre = "Agua mineral 500ml",
                Descripcion = "Botella individual",
                Categoria = categorias[0],
                Activo = true,
                Stock = new Stock { CantidadDisponible = 0, CantidadMinima = 10, AlertaActiva = true }
            },
            new Producto
            {
                Nombre = "Gaseosa cola 1.5L",
                Descripcion = "Botella retornable",
                Categoria = categorias[0],
                Activo = true,
                Stock = new Stock { CantidadDisponible = 0, CantidadMinima = 5, AlertaActiva = true }
            },
            new Producto
            {
                Nombre = "Arroz 1kg",
                Descripcion = "Paquete largo fino",
                Categoria = categorias[1],
                Activo = true,
                Stock = new Stock { CantidadDisponible = 0, CantidadMinima = 20, AlertaActiva = true }
            });

        await db.SaveChangesAsync();
        logger.LogInformation("Seed inicial completado. Usuario admin: {Email}", admin.Email);
    }
}