# Supuestos

Cuando algo era ambiguo se eligió la opción más simple. Estas son las decisiones tomadas:

1. **"Deshabilitar usuario" = baja física del registro.**
   El modelo de `Usuario` no incluye una columna `activo`, y no se agregaron campos fuera del
   modelo. Por eso "deshabilitar" un usuario se implementa **eliminando** el registro. Se impide:
   - eliminar al **último admin** (`rol = admin`);
   - eliminar un usuario que tenga **movimientos** registrados (integridad referencial de la FK
     `Movimiento.id_usuario`).
   En ambos casos se muestra un mensaje de error.

2. **Persistencia de enums como texto.**
   `Movimiento.tipo` guarda `"agregar"` / `"sacar"` y `Usuario.rol` guarda `"normal"` / `"admin"`,
   usando los nombres exactos del enunciado para que la columna sea legible y estable.

3. **Listado de productos en Operaciones.**
   Se muestran **todos** los productos (activos e inactivos) con su stock y su estado. Para los
   inactivos, las acciones agregar/sacar quedan deshabilitadas (además, el servidor las rechaza).

4. **Longitudes y validaciones.**
   - `email`: máximo 200, **único**.
   - `nombre`, `nombre`/`email` de usuario, `numero` de proveedor: máximo 200/100.
   - `password_hash` / `salt`: máximo 500.
   - `descripcion` de categoría/movimiento/proveedor: 500; de producto: 1000.
   - Contraseña mínima: **6 caracteres**.
   - `cantidad_minima` no puede ser negativa; la cantidad de un movimiento debe ser **entero > 0**
     y la **descripción/motivo es obligatoria**.

5. **Antiforgery en llamadas AJAX.**
   El token se obtiene del `@Html.AntiForgeryToken()` del layout y se envía por el header
   `RequestVerificationToken`, ya que los handlers AJAX reciben JSON (`[FromBody]`).

6. **Protección de datos.**
   Cada app persiste sus claves en su propia carpeta `keys/` (`PersistKeysToFileSystem`) y define
   `SetApplicationName("InventarioAdmin" | "InventarioOperaciones")`. Junto con el nombre de cookie
   distinto, esto garantiza que una sesión no sirva en la otra app.

7. **Migraciones solo en Admin.**
   Únicamente `InventarioAdmin` tiene la migración y la aplica (`MigrateAsync()`).
   `InventarioOperaciones` no tiene migraciones ni toca `__EFMigrationsHistory`: al arrancar solo
   verifica `CanConnectAsync()` y que el esquema responda.

8. **Historial en Operaciones.**
   Muestra los movimientos con **fecha, producto, tipo, cantidad, descripción y usuario** (sin
   exportación). La exportación a Excel/PDF/CSV con Buttons es solo para el historial de Admin,
   según lo pedido.

9. **`Proveedor.gmail`** se trata como texto libre (no se valida como email). El ABM usa borrado
   físico por ser una tabla independiente sin campo `activo`.

10. **"Degradar" al último admin.**
    No se pidió una pantalla para cambiar el rol de un usuario, así que no se implementó el cambio
    de rol. La protección pedida se cubre impidiendo la baja del último admin.

11. **Fecha de movimientos**: se usa la hora local del servidor (`DateTime.Now`).

12. **Seed**: se incluyen dos usuarios iniciales (admin y operador) para poder probar ambas apps,
    más 3 categorías y 3 productos con stock en 0 y alerta activa.

13. **Librerías de frontend**: se vendorizaron las versiones exactas pedidas (jQuery 2.2.0,
    Bootstrap 3.3.7, DataTables 1.10.18 + Buttons, Select2 4.0.5, Font Awesome 4.7) descargándolas
    a `wwwroot/lib`. No se usa npm/Node ni build de front.

14. **Una sola migración/base**: el esquema completo (incluida `Proveedor`) lo crea la App Admin.
    La App Operaciones mapea solo las tablas que usa.
