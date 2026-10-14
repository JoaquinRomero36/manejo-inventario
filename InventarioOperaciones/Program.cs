using InventarioOperaciones.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using NReco.Logging.File;

var builder = WebApplication.CreateBuilder(args);

// Logging a archivo (no se registran contraseñas).
builder.Logging.AddFile("logs/inventario-operaciones-{Date}.log", options =>
{
    options.Append = true;
    options.FileSizeLimitBytes = 10 * 1024 * 1024;
    options.MaxRollingFiles = 7;
});

// Sin autenticación obligatoria: la app entra directo al listado de productos.
// Las páginas Login/Logout quedan sin uso (se conservan por si se reactivan).
builder.Services.AddRazorPages();

builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// La autenticación por cookie se conserva registrada para que Login/Logout sigan compilando,
// pero NO se exige login en ninguna página (no hay FallbackPolicy).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.Name = "InventarioOperaciones.Auth";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Clave de protección de datos propia de esta app (nombre de aplicación y carpeta distintos).
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("InventarioOperaciones");

var app = builder.Build();

// Esta app NO aplica migraciones. Solo verifica que la base exista y esté lista.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
    try
    {
        if (!await db.Database.CanConnectAsync())
        {
            throw new InvalidOperationException(
                "No se puede conectar a la base de datos 'ManejoInventario'. " +
                "Verifique que SQL Server esté disponible, que la base exista y que el usuario de Windows " +
                "de esta aplicación tenga permisos. La App Admin es la encargada de crearla y migrarla.");
        }

        // Valida que el esquema esperado exista (falla con mensaje claro si falta).
        _ = await db.Productos.AsNoTracking().AnyAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "La base de datos no está lista. Ejecute primero la App InventarioAdmin para aplicar migraciones y seed.");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();