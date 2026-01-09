using System;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio.Services
{
    public class DireccionService
    {
        private readonly SolicitudDAO _solicitudDAO;

        public DireccionService()
        {
            _solicitudDAO = new SolicitudDAO();
        }

        // Validación Final
        public ResultadoOperacion Validar(int solicitudId, bool aprobada, string observaciones)
        {
            var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
            if (solicitud == null)
                return ResultadoOperacion.Error("Solicitud no encontrada.");

            solicitud.Estado = aprobada ? "APROBADA_DIRECCION" : "RECHAZADA_DIRECCION";
            solicitud.FechaActualizacion = DateTime.Now;

            _solicitudDAO.Actualizar(solicitud);

            return ResultadoOperacion.Ok(null, "Validación concluida.");
        }

        // Legalización
        public ResultadoOperacion Legalizar(int solicitudId, string director, string cargo)
        {
            var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
            solicitud.Director = director;
            solicitud.CargoDirector = cargo;
            solicitud.Estado = "LEGALIZADA";

            _solicitudDAO.Actualizar(solicitud);

            return ResultadoOperacion.Ok(null, "Solicitud legalizada.");
        }
    }
}
