using InventarioOperaciones.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioOperaciones.Data;

/// <summary>
/// DbContext exclusivo de la App Operaciones. No mapea Proveedor.
/// El mapeo de las tablas compartidas debe coincidir con el de la App Admin.
/// Esta app no aplica migraciones.
/// </summary>
public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categoria");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
            e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Producto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
            e.Property(x => x.IdCategoria).HasColumnName("id_categoria");
            e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

            e.HasOne(x => x.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(x => x.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Stock>(e =>
        {
            e.ToTable("Stock");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CantidadDisponible).HasColumnName("cantidad_disponible");
            e.Property(x => x.CantidadMinima).HasColumnName("cantidad_minima");
            e.Property(x => x.AlertaActiva).HasColumnName("alerta_activa");
            e.Property(x => x.IdProducto).HasColumnName("id_producto");

            e.HasOne(x => x.Producto)
                .WithOne(p => p.Stock)
                .HasForeignKey<Stock>(x => x.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.IdProducto).IsUnique();
        });

        modelBuilder.Entity<Movimiento>(e =>
        {
            e.ToTable("Movimiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Fecha).HasColumnName("fecha");
            e.Property(x => x.Cantidad).HasColumnName("cantidad");
            e.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
            e.Property(x => x.IdProducto).HasColumnName("id_producto");
            e.Property(x => x.IdUsuario).HasColumnName("id_usuario");

            e.HasOne(x => x.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(x => x.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Usuario)
                .WithMany(u => u.Movimientos)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
            e.Property(x => x.Rol).HasColumnName("rol").HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            e.Property(x => x.Salt).HasColumnName("salt").HasMaxLength(500).IsRequired();

            e.HasIndex(x => x.Email).IsUnique();
        });
    }
}