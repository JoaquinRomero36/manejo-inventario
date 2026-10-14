// DataTables del historial + paginación del banner de alertas (App Operaciones).
(function () {
    'use strict';

    // La pantalla es táctil, así que la lista de alertas no debe scrollear:
    // se muestran solo los productos que entran y se navega con las flechas.
    function iniciarPaginacionAlertas() {
        var $lista = $('.alerta-stock-lista');
        if ($lista.length === 0) {
            return;
        }

        var $items = $lista.children('li');
        if ($items.length === 0) {
            return;
        }

        var $cuerpo = $lista.closest('.alerta-stock-cuerpo');
        var $paginacion = $('.alerta-stock-paginacion');
        var $contador = $paginacion.find('.alerta-stock-contador');

        var paginas = [];
        var paginaActual = 0;

        function calcularPaginas() {
            // Se muestran todos para poder medir su alto natural.
            $items.show();

            var altoDisponible = $cuerpo.height();
            var alturas = $items
                .map(function () { return $(this).outerHeight(true); })
                .get();

            var resultado = [];
            var pagina = [];
            var acumulado = 0;

            for (var i = 0; i < alturas.length; i++) {
                // Se corta cuando el siguiente ya no entra (nunca deja una página vacía).
                if (pagina.length > 0 && acumulado + alturas[i] > altoDisponible) {
                    resultado.push(pagina);
                    pagina = [];
                    acumulado = 0;
                }
                pagina.push(i);
                acumulado += alturas[i];
            }

            if (pagina.length > 0) {
                resultado.push(pagina);
            }

            return resultado;
        }

        function dibujar() {
            $items.hide();

            var indices = paginas[paginaActual] || [];
            for (var i = 0; i < indices.length; i++) {
                $items.eq(indices[i]).show();
            }

            var hayVariasPaginas = paginas.length > 1;
            $paginacion.toggleClass('paginacion-visible', hayVariasPaginas);

            if (hayVariasPaginas) {
                $contador.text((paginaActual + 1) + ' / ' + paginas.length);
                $paginacion.find('.alerta-stock-prev').prop('disabled', paginaActual === 0);
                $paginacion.find('.alerta-stock-next').prop('disabled', paginaActual >= paginas.length - 1);
            }
        }

        function recalcular() {
            paginas = calcularPaginas();
            if (paginaActual > paginas.length - 1) {
                paginaActual = Math.max(0, paginas.length - 1);
            }
            dibujar();
        }

        $paginacion.on('click', '.alerta-stock-prev', function () {
            if (paginaActual > 0) {
                paginaActual--;
                dibujar();
            }
        });

        $paginacion.on('click', '.alerta-stock-next', function () {
            if (paginaActual < paginas.length - 1) {
                paginaActual++;
                dibujar();
            }
        });

        recalcular();

        var temporizador = null;
        $(window).on('resize', function () {
            clearTimeout(temporizador);
            temporizador = setTimeout(recalcular, 150);
        });
    }

    $(function () {
        $('#tablaHistorial').DataTable({
            order: [[0, 'desc']],
            // Pantalla táctil: no se usa el selector de cantidad y se fija en 9 (las que entran).
            pageLength: 9,
            lengthChange: false,
            language: {
                search: 'Buscar:',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Sin registros',
                zeroRecords: 'No se encontraron movimientos',
                paginate: { first: 'Primero', last: 'Último', next: 'Siguiente', previous: 'Anterior' }
            }
        });

        iniciarPaginacionAlertas();
    });
})();
