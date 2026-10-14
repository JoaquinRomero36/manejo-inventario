using InventarioAdmin.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using NReco.Logging.File;

var builder = WebApplication.CreateBuilder(args);

// Logging a archivo (no se registran contraseñas).
builder.Logging.AddFile("logs/inventario-admin-{Date}.log", options =>
{
    options.Append = true;
    options.FileSizeLimitBytes = 10 * 1024 * 1024;
    options.MaxRollingFiles = 7;
});

// Sin autenticación obligatoria: la app entra directo al Dashboard.
// Las páginas Login/Logout quedan sin uso (se conservan por si se reactivan).
builder.Services.AddRazorPages();

// Base de datos (SQL Server, Windows Integrated Security desde appsettings).
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
        options.Cookie.Name = "InventarioAdmin.Auth";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Clave de protección de datos propia de esta app (nombre de aplicación y carpeta distintos).
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("InventarioAdmin");

var app = builder.Build();

// Aplicar migraciones y seed: SOLO la App Admin lo hace.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        await db.Database.MigrateAsync();
        await DbInitializer.SeedAsync(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "No se pudo preparar la base de datos. Verifique la cadena de conexión y los permisos SQL.");
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