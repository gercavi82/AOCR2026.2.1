using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CapaModelo;
using CapaDatos.DAOs;

namespace CapaNegocio
{
    public class DocumentoBL
    {
        private readonly DocumentoDAO _documentoDAO;
        private readonly SolicitudAOCRDAO _solicitudAOCRDAO;

        // Configuraciones de validación
        private readonly string[] _extensionesPermitidas =
            { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".zip", ".rar" };

        private const long TAMANIO_MAXIMO = 10 * 1024 * 1024; // 10MB

        public DocumentoBL()
        {
            _documentoDAO = new DocumentoDAO();
            _solicitudAOCRDAO = new SolicitudAOCRDAO();
        }

        #region CRUD Principal

        public List<Documento> ObtenerTodos()
        {
            return _documentoDAO.ObtenerTodos();
        }

        public Documento ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID de documento inválido");
            return _documentoDAO.ObtenerPorId(id);
        }

        public List<Documento> ObtenerPorSolicitud(int solicitudId)
        {
            if (solicitudId <= 0) throw new ArgumentException("ID de solicitud inválido");
            return _documentoDAO.ObtenerPorSolicitud(solicitudId);
        }

        public bool Crear(Documento documento)
        {
            ValidarDocumento(documento);

            var solicitud = _solicitudAOCRDAO.ObtenerPorId(documento.CodigoSolicitud);
            if (solicitud == null)
                throw new Exception("La solicitud asociada no existe.");

            // Regla de Negocio: Solo permitir adjuntos en ciertos estados
            string[] estadosPermitidos = { "PENDIENTE", "EN_REVISION", "DOCUMENTOS_COMPLETOS", "BORRADOR" };
            if (!estadosPermitidos.Contains(solicitud.Estado))
            {
                throw new Exception($"No se pueden agregar documentos. La solicitud está en estado: {solicitud.Estado}");
            }

            documento.FechaSubida = DateTime.Now;
            documento.Estado = "PENDIENTE";

            return _documentoDAO.Crear(documento) > 0;
        }

        public bool Eliminar(int id)
        {
            var documento = _documentoDAO.ObtenerPorId(id);
            if (documento == null) throw new Exception("Documento no encontrado");

            // Borrado físico del archivo si existe
            if (!string.IsNullOrEmpty(documento.RutaArchivo) && File.Exists(documento.RutaArchivo))
            {
                try { File.Delete(documento.RutaArchivo); }
                catch { /* Log error de IO pero continuar con borrado lógico */ }
            }

            return _documentoDAO.Eliminar(id);
        }

        #endregion

        #region Gestión de Flujo de Aprobación

        public bool Aprobar(int documentoId, string usuario, string observaciones = null)
        {
            var documento = _documentoDAO.ObtenerPorId(documentoId);
            if (documento == null) throw new Exception("Documento no encontrado");

            documento.Estado = "APROBADO";
            documento.Observaciones = observaciones;

            bool ok = _documentoDAO.Actualizar(documento);

            if (ok)
            {
                // Disparador automático: Verifica si la solicitud cambia de estado
                VerificarDocumentosCompletos(documento.CodigoSolicitud, usuario);
            }

            return ok;
        }

        public bool Rechazar(int documentoId, string usuario, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("Debe especificar el motivo del rechazo");

            var documento = _documentoDAO.ObtenerPorId(documentoId);
            if (documento == null) throw new Exception("Documento no encontrado");

            documento.Estado = "RECHAZADO";
            documento.Observaciones = motivo;

            return _documentoDAO.Actualizar(documento);
        }

        #endregion

        #region Validaciones y Auxiliares

        private void ValidarDocumento(Documento d)
        {
            if (d == null) throw new Exception("Datos de documento nulos");
            if (d.CodigoSolicitud <= 0) throw new Exception("Código de solicitud no válido");
            if (string.IsNullOrWhiteSpace(d.NombreArchivo)) throw new Exception("El nombre del archivo es obligatorio");

            string ext = Path.GetExtension(d.NombreArchivo).ToLower();
            if (!_extensionesPermitidas.Contains(ext))
                throw new Exception($"Extensión {ext} no permitida. Use: " + string.Join(", ", _extensionesPermitidas));

            if (d.TamanioArchivo.HasValue && d.TamanioArchivo.Value > TAMANIO_MAXIMO)
                throw new Exception("El archivo excede el límite de 10MB");
        }

        /// <summary>
        /// Cambia la solicitud a DOCUMENTOS_COMPLETOS si todos están aprobados.
        /// </summary>
        private void VerificarDocumentosCompletos(int solicitudId, string usuario)
        {
            var documentos = _documentoDAO.ObtenerPorSolicitud(solicitudId);

            // Regla: Si hay documentos y TODOS están aprobados
            if (documentos.Any() && documentos.All(d => d.Estado == "APROBADO"))
            {
                var solicitud = _solicitudAOCRDAO.ObtenerPorId(solicitudId);

                if (solicitud != null && solicitud.Estado == "EN_REVISION")
                {
                    // ✅ CORRECCIÓN ERROR CS1503: Convertimos string 'usuario' a int
                    int idUsuarioReal;
                    if (!int.TryParse(usuario, out idUsuarioReal))
                    {
                        idUsuarioReal = 1; // ID por defecto (Sistema/Admin) en caso de error
                    }

                    _solicitudAOCRDAO.CambiarEstado(solicitudId, "DOCUMENTOS_COMPLETOS", idUsuarioReal, "Sistema: Todos los documentos aprobados.");
                }
            }
        }

        #endregion
    }
}