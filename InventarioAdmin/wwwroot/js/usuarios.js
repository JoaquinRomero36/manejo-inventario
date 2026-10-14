// Gestión de usuarios: alta, cambio de contraseña y baja lógica (App Admin).
(function () {
    'use strict';

    var tabla = null;
    var idPassword = 0;

    function antiforgeryToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    function initTabla() {
        if (tabla) {
            tabla.destroy();
        }
        tabla = $('#tablaUsuarios').DataTable({
            order: [[0, 'asc']],
            language: {
                search: 'Buscar:',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron usuarios',
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
        $('#nombre').val('');
        $('#email').val('');
        $('#rol').val('normal');
        $('#password').val('');
        $('#errorModal').addClass('hidden').text('');
        $('#modalUsuario').modal('show');
    }

    function guardar() {
        var nombre = $.trim($('#nombre').val());
        var email = $.trim($('#email').val());
        var rol = $('#rol').val();
        var password = $('#password').val();

        if (nombre === '') {
            $('#errorModal').removeClass('hidden').text('El nombre es obligatorio.');
            return;
        }
        if (email === '') {
            $('#errorModal').removeClass('hidden').text('El email es obligatorio.');
            return;
        }
        if (!password || password.length < 6) {
            $('#errorModal').removeClass('hidden').text('La contraseña debe tener al menos 6 caracteres.');
            return;
        }

        $('#btnGuardar').prop('disabled', true);
        $.ajax({
            url: '?handler=Crear',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({ nombre: nombre, email: email, rol: rol, password: password })
        }).done(function (resp) {
            if (resp && resp.ok) {
                $('#modalUsuario').modal('hide');
                mostrarExito(resp.mensaje);
                refrescarTabla();
            } else {
                $('#errorModal').removeClass('hidden').text((resp && resp.mensaje) || 'No se pudo crear el usuario.');
            }
        }).fail(function () {
            $('#errorModal').removeClass('hidden').text('Ocurrió un error al procesar la solicitud.');
        }).always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
    }

    function abrirPassword(boton) {
        idPassword = parseInt($(boton).data('id'), 10);
        $('#nombreUsuarioPassword').text($(boton).data('nombre'));
        $('#passwordNueva').val('');
        $('#errorModalPassword').addClass('hidden').text('');
        $('#modalPassword').modal('show');
    }

    function guardarPassword() {
        var password = $('#passwordNueva').val();
        if (!password || password.length < 6) {
            $('#errorModalPassword').removeClass('hidden').text('La contraseña debe tener al menos 6 caracteres.');
            return;
        }

        $('#btnGuardarPassword').prop('disabled', true);
        $.ajax({
            url: '?handler=CambiarPassword',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            data: JSON.stringify({ id: idPassword, password: password })
        }).done(function (resp) {
            if (resp && resp.ok) {
                $('#modalPassword').modal('hide');
                mostrarExito(resp.mensaje);
            } else {
                $('#errorModalPassword').removeClass('hidden').text((resp && resp.mensaje) || 'No se pudo cambiar la contraseña.');
            }
        }).fail(function () {
            $('#errorModalPassword').removeClass('hidden').text('Ocurrió un error al procesar la solicitud.');
        }).always(function () {
            $('#btnGuardarPassword').prop('disabled', false);
        });
    }

    function deshabilitar(boton) {
        var id = parseInt($(boton).data('id'), 10);
        var nombre = $(boton).data('nombre');

        InventarioUI.confirmar(
            '¿Deshabilitar el usuario "' + nombre + '"?',
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
                        InventarioUI.notificar((resp && resp.mensaje) || 'No se pudo deshabilitar el usuario.', 'danger');
                    }
                }).fail(function () {
                    InventarioUI.notificar('Ocurrió un error al procesar la solicitud.', 'danger');
                });
            },
            { titulo: 'Deshabilitar usuario', estilo: 'btn-danger', textoBoton: 'Deshabilitar' }
        );
    }

    $(function () {
        initTabla();
        $('#btnNuevo').on('click', abrirNuevo);
        $('#btnGuardar').on('click', guardar);
        $('#btnGuardarPassword').on('click', guardarPassword);
        $('#contenedorTabla').on('click', '.btn-password', function () { abrirPassword(this); });
        $('#contenedorTabla').on('click', '.btn-deshabilitar', function () { deshabilitar(this); });
    });
})();