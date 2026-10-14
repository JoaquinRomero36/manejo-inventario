// JavaScript común de la aplicación (Inventario Admin).
(function () {
    'use strict';

    // Utilidades de UI compartidas por todas las páginas.
    var InventarioUI = window.InventarioUI || {};

    // Muestra una notificación tipo "toast" arriba a la derecha.
    // tipo: 'success' | 'danger' | 'warning' | 'info'
    InventarioUI.notificar = function (mensaje, tipo, opciones) {
        var opts = opciones || {};

        var $cont = $('#notificaciones');
        if ($cont.length === 0) {
            $cont = $('<div id="notificaciones" class="notif-container" aria-live="polite"></div>').appendTo('body');
        }

        var $toast = $(
            '<div class="alert notif-toast" role="alert">' +
                '<button type="button" class="close" data-dismiss="alert" aria-label="Cerrar">' +
                    '<span aria-hidden="true">&times;</span>' +
                '</button>' +
                '<span class="notif-texto"></span>' +
            '</div>'
        );
        $toast.addClass('alert-' + (tipo || 'info'));
        // .text() en vez de .html() para evitar inyección de markup.
        $toast.find('.notif-texto').text(mensaje);

        $cont.append($toast);

        var duracion = opts.duracion !== undefined ? opts.duracion : 5000;
        var timer = null;
        if (duracion > 0) {
            timer = setTimeout(function () { cerrarToast($toast); }, duracion);
        }

        $toast.on('close.bs.alert', function () {
            if (timer) { clearTimeout(timer); }
        });
        $toast.on('closed.bs.alert', function () {
            $(this).remove();
        });

        return $toast;
    };

    function cerrarToast($toast) {
        if ($.fn.alert) {
            $toast.alert('close');
        } else {
            $toast.fadeOut(150, function () { $(this).remove(); });
        }
    }

    // Modal de confirmación reutilizable (reemplaza al confirm() nativo).
    InventarioUI.confirmar = function (mensaje, onConfirmar, opciones) {
        var opts = opciones || {};
        var $modal = $('#modalConfirmar');

        // Respaldo: si el modal no está en la página, se usa el confirm() nativo.
        if ($modal.length === 0) {
            if (window.confirm(mensaje)) { onConfirmar(); }
            return;
        }

        $modal.find('.confirmar-titulo').text(opts.titulo || 'Confirmar');
        $modal.find('.confirmar-texto').text(mensaje);

        var $ok = $modal.find('.confirmar-ok');
        $ok.removeClass('btn-primary btn-danger btn-success btn-warning')
           .addClass(opts.estilo || 'btn-primary')
           .text(opts.textoBoton || 'Confirmar');

        $ok.off('click.confirmar').on('click.confirmar', function () {
            $modal.modal('hide');
            onConfirmar();
        });

        $modal.modal('show');
    };

    window.InventarioUI = InventarioUI;

    $(function () {
        // Autocierre de alertas de éxito (respaldo si alguna página no lo maneja).
        window.mostrarAlerta = function (selector, mensaje) {
            $(selector).removeClass('hidden').text(mensaje);
            setTimeout(function () { $(selector).addClass('hidden'); }, 4000);
        };
    });
})();
