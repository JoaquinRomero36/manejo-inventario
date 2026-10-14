/*
=======================================================================================
  ManejoInventario - Creación de logins/usuarios de base y permisos mínimos
=======================================================================================
  Crea, con Integrated Security (sin usuario/clave en archivos):
    - Login/Usuario de Windows para la identidad de la App Admin.
    - Login/Usuario de Windows para la identidad de la App Operaciones.
    - Permisos mínimos por app.

  App Admin:      lectura/escritura sobre TODAS las tablas + DDL (aplica migraciones y seed).
  App Operaciones: SELECT en Categoria y Producto; SELECT/UPDATE en Stock;
                   SELECT/INSERT en Movimiento; SELECT en Usuario.
                   Sin permisos sobre Proveedor ni DDL.

  IMPORTANTE:
    - Ejecutar con una cuenta con permisos de sysadmin (o db_owner) sobre el servidor.
    - Reemplazar las identidades por las de tus app pools / cuentas reales.
    - Ejecutar este script DESPUÉS de que la App Admin haya creado la base de datos
      (o crear la base 'ManejoInventario' previamente de forma manual).
=======================================================================================
*/

/* -----------------------------------------------------------------------------------
   Identidades de Windows usadas por cada sitio IIS (app pool).
   Ajustar según corresponda, por ejemplo:
     IIS APPPOOL\InventarioAdminPool
     IIS APPPOOL\InventarioOperacionesPool
     DOMINIO\svc_inv_admin
     DOMINIO\svc_inv_operaciones
   ----------------------------------------------------------------------------------- */

/* =============================== MASTER: LOGINS ==================================== */
USE master;
GO

DECLARE @LoginAdmin  sysname = N'IIS APPPOOL\InventarioAdminPool';
DECLARE @LoginOper   sysname = N'IIS APPPOOL\InventarioOperacionesPool';

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginAdmin)
BEGIN
    EXEC('CREATE LOGIN [' + @LoginAdmin + '] FROM WINDOWS;');
    PRINT 'Login creado: ' + @LoginAdmin;
END
ELSE
    PRINT 'Login ya existe: ' + @LoginAdmin;

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginOper)
BEGIN
    EXEC('CREATE LOGIN [' + @LoginOper + '] FROM WINDOWS;');
    PRINT 'Login creado: ' + @LoginOper;
END
ELSE
    PRINT 'Login ya existe: ' + @LoginOper;
GO

/* ==================== BASE DE DATOS: USUARIOS Y PERMISOS ============================ */
USE ManejoInventario;
GO

DECLARE @LoginAdmin  sysname = N'IIS APPPOOL\InventarioAdminPool';
DECLARE @LoginOper   sysname = N'IIS APPPOOL\InventarioOperacionesPool';

/* --- Usuario de la App Admin --- */
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @LoginAdmin)
    EXEC('CREATE USER [' + @LoginAdmin + '] FOR LOGIN [' + @LoginAdmin + '];');

/* --- Usuario de la App Operaciones --- */
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @LoginOper)
    EXEC('CREATE USER [' + @LoginOper + '] FOR LOGIN [' + @LoginOper + '];');
GO

/* -----------------------------------------------------------------------------------
   PERMISOS APP ADMIN
   Lectura/escritura sobre todas las tablas + DDL para aplicar migraciones y seed.
   ----------------------------------------------------------------------------------- */
DECLARE @LoginAdmin sysname = N'IIS APPPOOL\InventarioAdminPool';
DECLARE @sql nvarchar(max);

SET @sql = N'
    ALTER ROLE db_datareader ADD MEMBER [' + @LoginAdmin + '];
    ALTER ROLE db_datawriter ADD MEMBER [' + @LoginAdmin + '];
    ALTER ROLE db_ddladmin   ADD MEMBER [' + @LoginAdmin + '];';
EXEC(@sql);
GO

/* -----------------------------------------------------------------------------------
   PERMISOS APP OPERACIONES
   Permisos explícitos y mínimos. NO se otorga nada sobre Proveedor ni DDL.
   ----------------------------------------------------------------------------------- */
DECLARE @LoginOper sysname = N'IIS APPPOOL\InventarioOperacionesPool';
DECLARE @sql2 nvarchar(max);

SET @sql2 = N'
    -- Categoria: solo lectura
    GRANT SELECT ON dbo.Categoria  TO [' + @LoginOper + '];

    -- Producto: solo lectura
    GRANT SELECT ON dbo.Producto   TO [' + @LoginOper + '];

    -- Stock: lectura y actualización (mueve stock)
    GRANT SELECT, UPDATE ON dbo.Stock TO [' + @LoginOper + '];

    -- Movimiento: lectura y alta (registra movimientos)
    GRANT SELECT, INSERT ON dbo.Movimiento TO [' + @LoginOper + '];

    -- Usuario: solo lectura (valida login)
    GRANT SELECT ON dbo.Usuario    TO [' + @LoginOper + '];

    -- NO se otorga SELECT sobre dbo.Proveedor (la App Operaciones no mapea esa tabla).
    -- NO se otorga DDL: no puede crear, alterar ni eliminar objetos.';
EXEC(@sql2);
GO

PRINT 'Permisos aplicados correctamente.';
GO
