// Alta y baja lógica de productos (App Admin).
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
        tabla = $('#tablaProductos').DataTable({
            order: [[0, 'asc']],
            language: {
                search: 'Buscar:',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron productos',
                paginate: { first: 'Primero', last: 'Último', next: 'Siguiente', previous: 'Anterior' }
            },
            columnDefs: [{ orderable: false, targets: [5] }]
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

    function cargarCategorias() {
        $.ajax({ url: '?handler=Categorias', type: 'GET', cache: false }).done(function (items) {
            var $sel = $('#idCategoria');
            $sel.empty();
            $sel.append($('<option>', { value: '', text: 'Seleccione una categoría...' }));
            $.each(items, function (i, item) {
                $sel.append($('<option>', { value: item.id, text: item.texto }));
            });
            // Select2 con búsqueda sobre el combo de categorías.
            $sel.select2({
                placeholder: 'Seleccione una categoría...',
                allowClear: true,
                dropdownParent: $('#modalProducto')
            });
        });
    }

    function abrirModal() {
        $('#nombre').val('');
        $('#descripcion').val('');
        $('#cantidadMinima').val(0);
        $('#idCategoria').val('').trigger('change');
        $('#errorModal').addClass('hidden').text('');
        cargarCategorias();
        $('#modalProducto').modal('show');
    }

    function guardar() {
        var nombre = $.trim($('#nombre').val());
        var idCategoria = parseInt($('#idCategoria').val(), 10);
        var cantidadMinima = parseInt($('#cantidadMinima').val(), 10);

        if (nombre === '') {
            $('#errorModal').removeClass('hidden').text('El nombre del producto es obligatorio.');
            return;
        }
        if (isNaN(idCategoria) || idCategoria <= 0) {
            $('#errorModal').removeClass('hidden').text('Debe seleccionar una categoría.');
            return;
        }
        if (isNaN(cantidadMinima) || cantidadMinima < 0) {
            $('#errorModal').removeClass('hidden').text('La cantidad mínima no puede ser negativa.');
            return;
        }

        $('#btnGuardar').prop('disabled', true);
        $.ajax({
            url: '?handler=Crear',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({
                nombre: nombre,
                descripcion: $.trim($('#descripcion').val()),
                idCategoria: idCategoria,
                cantidadMinima: cantidadMinima
            })
        }).done(function (resp) {
            if (resp && resp.ok) {
                $('#modalProducto').modal('hide');
                mostrarExito(resp.mensaje);
                refrescarTabla();
            } else {
                $('#errorModal').removeClass('hidden').text((resp && resp.mensaje) || 'No se pudo crear el producto.');
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
            '¿Deshabilitar el producto "' + nombre + '"?',
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
                        InventarioUI.notificar((resp && resp.mensaje) || 'No se pudo deshabilitar el producto.', 'danger');
                    }
                }).fail(function () {
                    InventarioUI.notificar('Ocurrió un error al procesar la solicitud.', 'danger');
                });
            },
            { titulo: 'Deshabilitar producto', estilo: 'btn-danger', textoBoton: 'Deshabilitar' }
        );
    }

    function habilitar(boton) {
        var id = parseInt($(boton).data('id'), 10);
        var nombre = $(boton).data('nombre');

        InventarioUI.confirmar(
            '¿Habilitar el producto "' + nombre + '"?',
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
                        InventarioUI.notificar((resp && resp.mensaje) || 'No se pudo habilitar el producto.', 'danger');
                    }
                }).fail(function () {
                    InventarioUI.notificar('Ocurrió un error al procesar la solicitud.', 'danger');
                });
            },
            { titulo: 'Habilitar producto', estilo: 'btn-success', textoBoton: 'Habilitar' }
        );
    }

    $(function () {
        initTabla();
        $('#btnNuevo').on('click', abrirModal);
        $('#btnGuardar').on('click', guardar);
        $('#contenedorTabla').on('click', '.btn-deshabilitar', function () {
            deshabilitar(this);
        });
        $('#contenedorTabla').on('click', '.btn-habilitar', function () {
            habilitar(this);
        });
    });
})();