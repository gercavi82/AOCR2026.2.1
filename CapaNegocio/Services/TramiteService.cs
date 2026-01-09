using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio.Services
{
    public class TramiteService
    {
        private readonly SolicitudDAO _solicitudDAO;
        private readonly DocumentoDAO _documentoDAO;

        public TramiteService()
        {
            _solicitudDAO = new SolicitudDAO();
            _documentoDAO = new DocumentoDAO();
        }

        // 1. Crear Solicitud
        public ResultadoOperacion CrearSolicitud(SolicitudAOCR solicitud)
        {
            if (_solicitudDAO.Crear(solicitud))
                return ResultadoOperacion.Ok(new { SolicitudId = solicitud.Id }, "Solicitud creada exitosamente.");

            return ResultadoOperacion.Error("No se pudo crear la solicitud.");
        }

        // 2. Agregar documento
        public ResultadoOperacion AgregarDocumento(int solicitudId, Documento doc)
        {
            doc.SolicitudId = solicitudId;

            if (_documentoDAO.Agregar(doc))
                return ResultadoOperacion.Ok(null, "Documento cargado.");

            return ResultadoOperacion.Error("Error al cargar el documento.");
        }

        // 3. Solicitud está lista para revisión documental
        public ResultadoOperacion EnviarDocumentos(int solicitudId)
        {
            var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
            if (solicitud == null)
                return ResultadoOperacion.Error("Solicitud no encontrada.");

            solicitud.Estado = "EN_REVISION_DOCUMENTAL";

            if (_solicitudDAO.Actualizar(solicitud))
                return ResultadoOperacion.Ok(null, "Documentos enviados a revisión.");

            return ResultadoOperacion.Error("Error al actualizar estado.");
        }

        // 4. Revisión documental - se detectaron observaciones
        public ResultadoOperacion SolicitarSubsanacion(int solicitudId, string observaciones)
        {
            var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
            if (solicitud == null)
                return ResultadoOperacion.Error("Solicitud no encontrada.");

            solicitud.Estado = "SUBSANACION";

            if (_solicitudDAO.Actualizar(solicitud))
                return ResultadoOperacion.Ok(null, "Se solicitó subsanación.");

            return ResultadoOperacion.Error("No se pudo solicitar subsanación.");
        }

        // 5. Subsanar
        public ResultadoOperacion Subsanar(int solicitudId, List<Documento> documentos)
        {
            foreach (var d in documentos)
            {
                d.SolicitudId = solicitudId;
                _documentoDAO.Agregar(d);
            }

            var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
            solicitud.Estado = "EN_REVISION_DOCUMENTAL";

            _solicitudDAO.Actualizar(solicitud);

            return ResultadoOperacion.Ok(null, "Subsanación cargada con éxito.");
        }
    }
}
