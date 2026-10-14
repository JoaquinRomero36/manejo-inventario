using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventarioAdmin.Data;

/// <summary>
/// Factory usada solo por las herramientas de EF Core (dotnet ef migrations).
/// Evita ejecutar el pipeline real de la app durante el diseño.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
{
    public InventarioDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventarioDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost\\SQLEXPRESS;Database=ManejoInventario;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");
        return new InventarioDbContext(optionsBuilder.Options);
    }
}