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

15. **Sin login (acceso físico).**
    La bodega tiene dos pantallas y cada una corre una app distinta, así que **no hay login
    obligatorio**: se quitó la exigencia de autenticación (`FallbackPolicy`) y el menú se muestra
    siempre. La autenticación por cookie se dejó **registrada** solo para que las páginas
    `Login`/`Logout` sigan compilando, pero quedan **sin uso**.

16. **Atribución de movimientos sin login.**
    `Movimiento.id_usuario` es obligatorio, así que los movimientos se atribuyen al **usuario
    operador de la semilla** (el primer usuario con `rol = normal`). Si no existiera ningún usuario
    `normal`, el servidor devuelve un error claro en vez de guardar un movimiento sin usuario.
    El usuario admin queda solo como registro histórico.

17. **Pantalla principal de Operaciones = Historial con banner de stock.**
    El Historial pasó a ser la página de inicio (la tabla más importante) con un **banner a la
    izquierda**:
    - si hay productos con la **alerta activa**, lista esos productos con su disponible y mínimo;
    - si **no** hay alertas, muestra los **3 productos activos con menor margen** sobre el mínimo
      (`disponible − mínimo`), para avisar cuáles están más cerca de disparar la alerta.
    La alerta se toma del flag `Stock.alerta_activa` (coherente con la columna *Estado*), no de un
    cálculo nuevo. Los movimientos de stock siguen en la pestaña *Productos*.

18. **Íconos de orden de DataTables.**
    La carpeta `images/` de DataTables no se vendorizó, así que los íconos de orden se reemplazaron
    por íconos de Font Awesome vía CSS (evita los 404) en ambas apps.

19. **Habilitar un producto deshabilitado (App Admin).**
    La baja de productos es lógica (`activo = false`), así que se agregó la acción **Habilitar** en
    la tabla de Productos para revertirla. Al habilitar se exige que la **categoría esté activa**
    (para no dejar un producto activo bajo una categoría inactiva, que es el invariante en el que se
    apoya la regla de deshabilitar categorías). Si la categoría está deshabilitada, se muestra un
    mensaje pidiendo habilitarla primero. Rehabilitar no altera el stock ni la alerta del producto.

20. **Avisos y confirmaciones sin diálogos nativos (App Admin).**
    Se reemplazaron los `alert()` por **notificaciones tipo toast** (arriba a la derecha, color según
    el caso, con autocierre a los 5 s y botón para cerrarlas) y los `confirm()` por un **modal de
    confirmación Bootstrap** reutilizable. Todo se centraliza en `wwwroot/js/site.js`
    (`InventarioUI.notificar` / `InventarioUI.confirmar`), el markup del modal y del contenedor está
    en `Pages/Shared/_Layout.cshtml` y los estilos en `wwwroot/css/site.css`. Los mensajes de éxito
    que ya usaban la banda `#alertaExito` se mantienen igual.
