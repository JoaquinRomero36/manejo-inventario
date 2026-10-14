// DataTables del historial completo con exportación a Excel/PDF/CSV (App Admin).
(function () {
    'use strict';
    $(function () {
        $('#tablaHistorial').DataTable({
            dom: "<'row'<'col-sm-6'l><'col-sm-6'f>>" +
                 "<'row'<'col-sm-12'B>>" +
                 "<'row'<'col-sm-12'tr>>" +
                 "<'row'<'col-sm-5'i><'col-sm-7'p>>",
            buttons: [
                { extend: 'excel', text: '<i class="fa fa-file-excel-o"></i> Excel' },
                { extend: 'pdf', text: '<i class="fa fa-file-pdf-o"></i> PDF' },
                { extend: 'csv', text: '<i class="fa fa-file-text-o"></i> CSV' },
                { extend: 'print', text: '<i class="fa fa-print"></i> Imprimir' }
            ],
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