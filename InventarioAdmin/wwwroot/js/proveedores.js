// ABM simple de proveedores (App Admin).
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
        tabla = $('#tablaProveedores').DataTable({
            order: [[0, 'asc']],
            language: {
                search: 'Buscar:',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron proveedores',
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

    function abrirNuevo() {
        $('#idProveedor').val(0);
        $('#numero').val('');
        $('#gmail').val('');
        $('#descripcion').val('');
        $('#tituloProveedor').text('Nuevo proveedor');
        $('#errorModal').addClass('hidden').text('');
        $('#modalProveedor').modal('show');
    }

    function abrirEditar(boton) {
        $('#idProveedor').val($(boton).data('id'));
        $('#numero').val($(boton).data('numero'));
        $('#gmail').val($(boton).data('gmail'));
        $('#descripcion').val($(boton).data('descripcion'));
        $('#tituloProveedor').text('Editar proveedor');
        $('#errorModal').addClass('hidden').text('');
        $('#modalProveedor').modal('show');
    }

    function guardar() {
        var id = parseInt($('#idProveedor').val(), 10);
        var numero = $.trim($('#numero').val());
        if (numero === '') {
            $('#errorModal').removeClass('hidden').text('El número es obligatorio.');
            return;
        }

        var handler = id > 0 ? 'Editar' : 'Crear';
        $('#btnGuardar').prop('disabled', true);
        $.ajax({
            url: '?handler=' + handler,
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({
                id: id,
                numero: numero,
                gmail: $.trim($('#gmail').val()),
                descripcion: $.trim($('#descripcion').val())
            })
        }).done(function (resp) {
            if (resp && resp.ok) {
                $('#modalProveedor').modal('hide');
                mostrarExito(resp.mensaje);
                refrescarTabla();
            } else {
                $('#errorModal').removeClass('hidden').text((resp && resp.mensaje) || 'No se pudo guardar el proveedor.');
            }
        }).fail(function () {
            $('#errorModal').removeClass('hidden').text('Ocurrió un error al procesar la solicitud.');
        }).always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
    }

    function eliminar(boton) {
        var id = parseInt($(boton).data('id'), 10);
        if (!confirm('¿Eliminar el proveedor "' + $(boton).data('numero') + '"?')) {
            return;
        }
        $.ajax({
            url: '?handler=Eliminar',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({ id: id })
        }).done(function (resp) {
            if (resp && resp.ok) {
                mostrarExito(resp.mensaje);
                refrescarTabla();
            } else {
                alert((resp && resp.mensaje) || 'No se pudo eliminar el proveedor.');
            }
        }).fail(function () {
            alert('Ocurrió un error al procesar la solicitud.');
        });
    }

    $(function () {
        initTabla();
        $('#btnNuevo').on('click', abrirNuevo);
        $('#btnGuardar').on('click', guardar);
        $('#contenedorTabla').on('click', '.btn-editar', function () { abrirEditar(this); });
        $('#contenedorTabla').on('click', '.btn-eliminar', function () { eliminar(this); });
    });
})();