// DataTables del historial de movimientos (App Operaciones).
(function () {
    'use strict';
    $(function () {
        $('#tablaHistorial').DataTable({
            order: [[0, 'desc']],
            language: {
                search: 'Buscar:',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron movimientos',
                paginate: { first: 'Primero', last: 'Último', next: 'Siguiente', previous: 'Anterior' }
            }
        });
    });
})();