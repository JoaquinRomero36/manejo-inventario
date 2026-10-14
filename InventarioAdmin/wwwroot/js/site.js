// JavaScript común de la aplicación (Inventario Admin).
(function () {
    'use strict';

    // Configuración global de idioma para los diálogos de DataTables ya está en cada página.
    $(function () {
        // Autocierre de alertas de éxito (respaldo si alguna página no lo maneja).
        window.mostrarAlerta = function (selector, mensaje) {
            $(selector).removeClass('hidden').text(mensaje);
            setTimeout(function () { $(selector).addClass('hidden'); }, 4000);
        };
    });
})();