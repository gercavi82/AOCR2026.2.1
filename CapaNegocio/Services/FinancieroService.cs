using System;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio.Services
{
    public class FinancieroService
    {
        private readonly PagoDAO _pagoDAO;
        private readonly SolicitudDAO _solicitudDAO;

        public FinancieroService()
        {
            _pagoDAO = new PagoDAO();
            _solicitudDAO = new SolicitudDAO();
        }

        // Registrar pago
        public ResultadoOperacion RegistrarPago(Pago pago)
        {
            if (_pagoDAO.Registrar(pago))
                return ResultadoOperacion.Ok(null, "Pago registrado con éxito.");

            return ResultadoOperacion.Error("Error registrando pago.");
        }

        // Validar pago inicial
        public ResultadoOperacion ValidarPagoInicial(int solicitudId)
        {
            var pago = _pagoDAO.ObtenerPorSolicitud(solicitudId);
            if (pago == null)
                return ResultadoOperacion.Error("No hay pago registrado.");

            pago.Estado = "VALIDADO";
            _pagoDAO.Actualizar(pago);

            var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
            solicitud.Estado = "PENDIENTE_ASIGNACION_TECNICA";
            _solicitudDAO.Actualizar(solicitud);

            return ResultadoOperacion.Ok(null, "Pago validado.");
        }
    }
}
