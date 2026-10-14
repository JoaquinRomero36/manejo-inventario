// Alta y baja lógica de categorías (App Admin).
(function () {
    'use strict';

    var tabla = null;

    function antiforgeryToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    function initTabla() {
        if (tabla) {
            tabla.destroy();
        }
        tabla = $('#tablaCategorias').DataTable({
            order: [[0, 'asc']],
            language: {
                search: 'Buscar:',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron categorías',
                paginate: { first: 'Primero', last: 'Último', next: 'Siguiente', previous: 'Anterior' }
            },
            columnDefs: [{ orderable: false, targets: [3] }]
        });
    }

    function refrescarTabla() {
        $.ajax({ url: '?handler=Tabla', type: 'GET', cache: false }).done(function (html) {
            $('#contenedorTabla').html(html);
            initTabla();
        });
    }

    function mostrarExito(mensaje) {
        $('#alertaExito').removeClass('hidden').text(mensaje);
        setTimeout(function () { $('#alertaExito').addClass('hidden'); }, 4000);
    }

    function abrirModal() {
        $('#descripcion').val('');
        $('#errorModal').addClass('hidden').text('');
        $('#modalCategoria').modal('show');
    }

    function guardar() {
        var descripcion = $.trim($('#descripcion').val());
        if (descripcion === '') {
            $('#errorModal').removeClass('hidden').text('La descripción es obligatoria.');
            return;
        }

        $('#btnGuardar').prop('disabled', true);
        $.ajax({
            url: '?handler=Crear',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({ descripcion: descripcion })
        }).done(function (resp) {
            if (resp && resp.ok) {
                $('#modalCategoria').modal('hide');
                mostrarExito(resp.mensaje);
                refrescarTabla();
            } else {
                $('#errorModal').removeClass('hidden').text((resp && resp.mensaje) || 'No se pudo crear la categoría.');
            }
        }).fail(function () {
            $('#errorModal').removeClass('hidden').text('Ocurrió un error al procesar la solicitud.');
        }).always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
    }

    function deshabilitar(boton) {
        var id = parseInt($(boton).data('id'), 10);
        var nombre = $(boton).data('nombre');

        InventarioUI.confirmar(
            '¿Deshabilitar la categoría "' + nombre + '"?',
            function () {
                $.ajax({
                    url: '?handler=Deshabilitar',
                    type: 'POST',
                    contentType: 'application/json',
                    headers: { 'RequestVerificationToken': antiforgeryToken() },
                    data: JSON.stringify({ id: id })
                }).done(function (resp) {
                    if (resp && resp.ok) {
                        mostrarExito(resp.mensaje);
                        refrescarTabla();
                    } else {
                        InventarioUI.notificar((resp && resp.mensaje) || 'No se pudo deshabilitar la categoría.', 'danger');
                    }
                }).fail(function () {
                    InventarioUI.notificar('Ocurrió un error al procesar la solicitud.', 'danger');
                });
            },
            { titulo: 'Deshabilitar categoría', estilo: 'btn-danger', textoBoton: 'Deshabilitar' }
        );
    }

    function habilitar(boton) {
        var id = parseInt($(boton).data('id'), 10);
        var nombre = $(boton).data('nombre');

        InventarioUI.confirmar(
            '¿Habilitar la categoría "' + nombre + '"?',
            function () {
                $.ajax({
                    url: '?handler=Habilitar',
                    type: 'POST',
                    contentType: 'application/json',
                    headers: { 'RequestVerificationToken': antiforgeryToken() },
                    data: JSON.stringify({ id: id })
                }).done(function (resp) {
                    if (resp && resp.ok) {
                        mostrarExito(resp.mensaje);
                        refrescarTabla();
                    } else {
                        InventarioUI.notificar((resp && resp.mensaje) || 'No se pudo habilitar la categoría.', 'danger');
                    }
                }).fail(function () {
                    InventarioUI.notificar('Ocurrió un error al procesar la solicitud.', 'danger');
                });
            },
            { titulo: 'Habilitar categoría', estilo: 'btn-success', textoBoton: 'Habilitar' }
        );
    }

    $(function () {
        initTabla();
        $('#btnNueva').on('click', abrirModal);
        $('#btnGuardar').on('click', guardar);
        $('#contenedorTabla').on('click', '.btn-deshabilitar', function () {
            deshabilitar(this);
        });
        $('#contenedorTabla').on('click', '.btn-habilitar', function () {
            habilitar(this);
        });
    });
})();