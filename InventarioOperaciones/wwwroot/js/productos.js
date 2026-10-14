// Lógica del listado de productos y movimientos de stock (App Operaciones).
(function () {
    'use strict';

    var tabla = null;
    var idProducto = 0;
    var tipoMovimiento = 'agregar';

    function antiforgeryToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    // Inicializa/renueva DataTables tras cada refresco del parcial.
    function initTabla() {
        if (tabla) {
            tabla.destroy();
        }
        tabla = $('#tablaProductos').DataTable({
            order: [[0, 'asc']],
            // Pantalla táctil: no se usa el selector de cantidad y se fija en 9 (las que entran).
            pageLength: 9,
            lengthChange: false,
            language: {
                search: 'Buscar:',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron productos',
                paginate: { first: 'Primero', last: 'Último', next: 'Siguiente', previous: 'Anterior' }
            },
            columnDefs: [
                { orderable: false, targets: [4, 5] }
            ]
        });
    }

    // Refresca solo la tabla desde el parcial del servidor.
    function refrescarTabla() {
        $.ajax({
            url: '?handler=Tabla',
            type: 'GET',
            cache: false
        }).done(function (html) {
            $('#contenedorTabla').html(html);
            initTabla();
        });
    }

    function abrirModal(boton) {
        idProducto = parseInt($(boton).data('id'), 10);
        tipoMovimiento = $(boton).data('tipo');
        $('#nombreProducto').text($(boton).data('nombre'));
        $('#tituloMovimiento').text(tipoMovimiento === 'sacar' ? 'Sacar stock' : 'Agregar stock');
        $('#btnConfirmar')
            .removeClass('btn-primary btn-danger')
            .addClass(tipoMovimiento === 'sacar' ? 'btn-danger' : 'btn-primary');
        $('#cantidad').val(1);
        $('#descripcion').val('');
        $('#errorModal').addClass('hidden').text('');
        $('#modalMovimiento').modal('show');
    }

    function confirmar() {
        var cantidad = parseInt($('#cantidad').val(), 10);
        var descripcion = $.trim($('#descripcion').val());

        // Validación de cliente (el servidor vuelve a validar).
        if (isNaN(cantidad) || cantidad <= 0) {
            $('#errorModal').removeClass('hidden').text('La cantidad debe ser un número entero mayor a 0.');
            return;
        }
        if (descripcion === '') {
            $('#errorModal').removeClass('hidden').text('La descripción (motivo) es obligatoria.');
            return;
        }

        var handler = tipoMovimiento === 'sacar' ? 'Sacar' : 'Agregar';
        $('#btnConfirmar').prop('disabled', true);

        $.ajax({
            url: '?handler=' + handler,
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({ idProducto: idProducto, cantidad: cantidad, descripcion: descripcion })
        }).done(function (resp) {
            if (resp && resp.ok) {
                $('#modalMovimiento').modal('hide');
                $('#alertaExito').removeClass('hidden').text(resp.mensaje);
                refrescarTabla();
                setTimeout(function () { $('#alertaExito').addClass('hidden'); }, 4000);
            } else {
                $('#errorModal').removeClass('hidden').text((resp && resp.mensaje) || 'No se pudo completar la operación.');
            }
        }).fail(function () {
            $('#errorModal').removeClass('hidden').text('Ocurrió un error al procesar la solicitud.');
        }).always(function () {
            $('#btnConfirmar').prop('disabled', false);
        });
    }

    $(function () {
        initTabla();

        // Delegación: la tabla se reemplaza por AJAX.
        $('#contenedorTabla').on('click', '.btn-movimiento', function () {
            abrirModal(this);
        });

        $('#btnConfirmar').on('click', confirmar);
    });
})();