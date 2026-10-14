// JavaScript común de la aplicación (Inventario Operaciones).
(function () {
    'use strict';

    $(function () {
        window.mostrarAlerta = function (selector, mensaje) {
            $(selector).removeClass('hidden').text(mensaje);
            setTimeout(function () { $(selector).addClass('hidden'); }, 4000);
        };
    });
})();