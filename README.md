# Manejo de Inventario — dos aplicaciones web

Dos aplicaciones ASP.NET Core 8 (Razor Pages) **independientes** que comparten una misma base
de datos SQL Server. No comparten código: cada una tiene sus propias entidades, su propio
`DbContext` y su propia solución.

| App | Carpeta | Usuarios | Entrada |
|-----|---------|----------|---------|
| **InventarioAdmin** | `InventarioAdmin/` | Encargado / administrador | Directo al Dashboard |
| **InventarioOperaciones** | `InventarioOperaciones/` | Trabajadores de bodega | Directo al Historial |

> **Sin login.** La separación de accesos es **física**: en la bodega hay dos pantallas, cada una
> con una app distinta. No hay autenticación obligatoria; las páginas `Login`/`Logout` quedan
> sin uso (se conservan por si se quiere reactivar el login más adelante).


- **InventarioAdmin**: catálogo (categorías y productos), usuarios, proveedores e historial.
  Es la **única** que aplica migraciones y seed. Tanto las **categorías** como los **productos**
  tienen alta y **baja lógica** (*Deshabilitar*), y se pueden **volver a habilitar** (*Habilitar*)
  desde la misma tabla. Una categoría solo se puede deshabilitar si no tiene productos activos.
- **InventarioOperaciones**: solo mueve stock (agregar / sacar) y consulta el historial.
  **No** ejecuta `Migrate()`; al arrancar verifica que la base esté lista.
  Su pantalla de inicio es el **Historial**, con un **banner de stock a la izquierda**: si hay
  productos con la alerta activa muestra el mensaje de alerta; si no hay ninguna, muestra los
  **3 productos más cerca del mínimo**. Los movimientos se hacen desde la pestaña *Productos*.

## Stack

- C# 12 / .NET 8 (LTS), proyectos SDK-style y `global.json` (SDK 8.0.319).
- Razor Pages; los endpoints AJAX son **handlers del PageModel** (`?handler=...`).
- EF Core **8.0.22** (provider SqlServer), Fluent API en `OnModelCreating`.
- Autenticación por cookie **registrada pero no exigida** (sin login obligatorio), antiforgery en
  todos los POST, contraseñas con **PBKDF2** y sal por usuario.
- Logging con `ILogger` + **NReco.Logging.File** (archivos en `logs/`).
- Frontend vendorizado en `wwwroot/lib` (sin npm/Node): jQuery 2.2.0, Bootstrap 3.3.7,
  Font Awesome 4.7, DataTables 1.10.18 + Buttons, Select2 4.0.5, jQuery Validation + Unobtrusive.
- `bundleconfig.json` para minificar CSS/JS.

## Requisitos

- .NET SDK 8.0.x instalado (el `global.json` fija 8.0.319).
- SQL Server o SQL Server Express, con **Windows Integrated Security**.
- Para IIS: ASP.NET Core Hosting Bundle (módulo `AspNetCoreModuleV2`).

## Modelo de datos

Una sola base (`ManejoInventario`) con las tablas: `Categoria`, `Producto`, `Stock`,
`Movimiento`, `Usuario` y `Proveedor`. Los nombres de columnas son los del enunciado
(`id`, `descripcion`, `activo`, `id_categoria`, `cantidad_disponible`, `cantidad_minima`,
`alerta_activa`, `id_producto`, `fecha`, `cantidad`, `tipo`, `id_usuario`, `email`, `rol`,
`password_hash`, `salt`, `numero`, `gmail`).

Relaciones: Categoria 1–N Producto, Producto 1–1 Stock, Producto 1–N Movimiento,
Usuario 1–N Movimiento. `Proveedor` es una tabla independiente.

## 1) Migración y seed (App Admin)

La migración inicial ya está creada en `InventarioAdmin/Data/Migrations`.

Si necesitás recrearla:

```powershell
cd InventarioAdmin
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

`Program.cs` de la App Admin ejecuta `MigrateAsync()` y el `DbInitializer` (seed) al arrancar.
El seed crea, si la base está vacía:

- Usuario admin: `admin@inventario.local` / `Admin123!`
- Usuario operador: `operador@inventario.local` / `Operador123!`
- 3 categorías y 3 productos con su stock.

> **Sin login**, los movimientos de stock se atribuyen al **usuario operador de la semilla**
> (el primer usuario con rol `normal`). El usuario admin queda solo como registro histórico
> y para la pantalla de Usuarios. Cambiá las contraseñas iniciales en cuanto puedas.

## 2) Correr en desarrollo (puertos distintos)

Cada app se ejecuta por separado (dos soluciones). Ejemplo con Kestrel:

```powershell
# Terminal 1 - App Admin (http://localhost:5101)
cd InventarioAdmin
dotnet run

# Terminal 2 - App Operaciones (http://localhost:5102)
cd InventarioOperaciones
dotnet run
```

- **Primero** levantá la App Admin (crea/migra la base y hace el seed).
- Luego la App Operaciones (verifica que la base esté lista).

Ambas usan la cadena de conexión `DefaultConnection` de su `appsettings.json`
(podés sobreescribirla por entorno en `appsettings.Development.json` o con variables de entorno):

```
Server=localhost\SQLEXPRESS;Database=ManejoInventario;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

## 3) Permisos mínimos de base de datos

Ejecutar `sql/01-crear-logins-permisos.sql` (con una cuenta `sysadmin`), **después** de que la
base exista. El script:

- Crea un login/usuario de Windows por app (ajustá las identidades a las de tus app pools).
- **Admin**: `db_datareader`, `db_datawriter` y `db_ddladmin` (necesita DDL para migrar).
- **Operaciones**: `SELECT` en `Categoria` y `Producto`; `SELECT/UPDATE` en `Stock`;
  `SELECT/INSERT` en `Movimiento`; `SELECT` en `Usuario`. Sin permisos sobre `Proveedor` ni DDL.

## 4) Publicar en IIS como sitios separados

Publicar cada app (framework-dependent):

```powershell
dotnet publish InventarioAdmin/InventarioAdmin.csproj -c Release -p:PublishProfile=FolderProfile
dotnet publish InventarioOperaciones/InventarioOperaciones.csproj -c Release -p:PublishProfile=FolderProfile
```

En IIS, crear **dos sitios** con **dos app pools** y **dos puertos/host** distintos:

| Sitio | App pool | Puerto ejemplo | Ruta física |
|-------|----------|----------------|-------------|
| InventarioAdmin | InventarioAdminPool | 8081 | `...\InventarioAdmin\bin\Release\net8.0\publish` |
| InventarioOperaciones | InventarioOperacionesPool | 8082 | `...\InventarioOperaciones\bin\Release\net8.0\publish` |

- Cada `web.config` ya usa `hostingModel="inprocess"` (AspNetCoreModuleV2).
- Cada app tiene su **propia clave de protección de datos** (carpeta `keys/` y `SetApplicationName`).
- Dar permiso de lectura/escritura en la carpeta `keys/` (y `logs/`) a la identidad de cada app pool.

## Separación de accesos

La separación es **física**, no por login: hay dos pantallas en la bodega y cada una corre una app.

- **Admin** (encargado): entra directo al Dashboard y administra catálogo, usuarios y proveedores.
- **Operaciones** (trabajadores): entra directo al **Historial** (banner de stock + tabla de
  movimientos) y mueve stock desde *Productos*.

No hay autenticación obligatoria. Las páginas `Login`/`Logout` quedan sin uso (se conservan por si
más adelante se quiere reactivar el login).

## Estructura

```
manejo-inventario/
├─ global.json
├─ InventarioAdmin/            # solución + proyecto propios
│  ├─ Models/                  # entidades (incluye Proveedor)
│  ├─ Data/                    # DbContext, migración, DbInitializer
│  ├─ Security/                # PBKDF2 (KeyDerivation)
│  ├─ Pages/                   # Razor Pages + partials
│  ├─ wwwroot/lib/             # librerías vendorizadas
│  ├─ bundleconfig.json
│  └─ web.config
├─ InventarioOperaciones/      # solución + proyecto propios
│  ├─ Models/                  # entidades (sin Proveedor)
│  ├─ Data/                    # DbContext (sin migraciones)
│  ├─ Security/
│  ├─ Pages/
│  ├─ wwwroot/lib/
│  ├─ bundleconfig.json
│  └─ web.config
└─ sql/01-crear-logins-permisos.sql
```

## Supuestos

Ver la sección **Supuestos** al final de la conversación / en `Docs/SUPUESTOS.md`.
