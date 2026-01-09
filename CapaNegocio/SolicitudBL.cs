using System;
using System.Collections.Generic;
using System.Linq;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    /// <summary>
    /// Lógica de Negocio para el Sistema AOCR
    /// Maneja las validaciones y el flujo entre la Presentación y los Datos.
    /// </summary>
    public class SolicitudBL
    {
        private readonly SolicitudAOCRDAO _solicitudDAO;
        private readonly HistorialEstadoDAO _historialDAO;

        public SolicitudBL()
        {
            _solicitudDAO = new SolicitudAOCRDAO();
            _historialDAO = new HistorialEstadoDAO();
        }

        #region Consultas y Listados

        public List<SolicitudAOCR> ObtenerTodasActivas()
        {
            return _solicitudDAO.ListarActivas();
        }

        // Resuelve error CS1061 en FinancieroController
        public SolicitudAOCR ObtenerDetalle(int codigoSolicitud)
        {
            if (codigoSolicitud <= 0) return null;
            return _solicitudDAO.ObtenerPorId(codigoSolicitud);
        }

        public List<SolicitudAOCR> ObtenerPorUsuario(int codigoUsuario)
        {
            return _solicitudDAO.ObtenerPorUsuario(codigoUsuario);
        }

        #endregion

        #region Operaciones de Creación y Edición

        // Resuelve error CS1061 en SolicitudController
        public bool Crear(SolicitudAOCR modelo, int codigoUsuario, out string mensaje)
        {
            mensaje = "";
            try
            {
                if (modelo == null) { mensaje = "Datos de solicitud inválidos."; return false; }

                modelo.CodigoUsuario = codigoUsuario;
                modelo.Estado = "BORRADOR";
                modelo.FechaSolicitud = DateTime.Now;
                modelo.CreatedAt = DateTime.Now;
                modelo.CreatedBy = codigoUsuario.ToString();

                if (string.IsNullOrWhiteSpace(modelo.NumeroSolicitud))
                    modelo.NumeroSolicitud = GenerarNumeroSolicitud(DateTime.Now.Year);

                int id = _solicitudDAO.InsertarConReturn(modelo);

                if (id > 0)
                {
                    _historialDAO.RegistrarCambio(id, null, "BORRADOR", codigoUsuario, "Creación inicial del trámite.");
                    mensaje = "Solicitud creada correctamente.";
                    return true;
                }

                mensaje = "No se pudo insertar el registro en la base de datos.";
                return false;
            }
            catch (Exception ex)
            {
                mensaje = "Error en Capa de Negocio: " + ex.Message;
                return false;
            }
        }

        // Resuelve error CS1061 en FinancieroController
        public bool Actualizar(SolicitudAOCR modelo, int codigoUsuario, out string mensaje)
        {
            mensaje = "";
            var actual = _solicitudDAO.ObtenerPorId(modelo.CodigoSolicitud);

            if (actual == null) { mensaje = "Solicitud no encontrada."; return false; }

            modelo.UpdatedAt = DateTime.Now;
            modelo.UpdatedBy = codigoUsuario.ToString();

            // Aquí llamamos al método del DAO que actualiza los campos generales
            bool ok = _solicitudDAO.ActualizarGeneral(modelo);

            if (ok) mensaje = "Cambios guardados con éxito.";
            else mensaje = "Error al intentar actualizar los datos.";

            return ok;
        }

        #endregion

        #region Gestión de Flujos y Estados

        // Resuelve error CS1061 en SolicitudController
        public bool Enviar(int codigoSolicitud, int codigoUsuario, bool esAdmin, out string mensaje)
        {
            mensaje = "";
            var solicitud = _solicitudDAO.ObtenerPorId(codigoSolicitud);

            if (solicitud == null) { mensaje = "Trámite no encontrado."; return false; }
            if (!esAdmin && solicitud.CodigoUsuario != codigoUsuario) { mensaje = "No tiene permisos sobre esta solicitud."; return false; }

            if (solicitud.Estado != "BORRADOR") { mensaje = "Solo puede enviar solicitudes en estado BORRADOR."; return false; }

            bool ok = _solicitudDAO.CambiarEstado(codigoSolicitud, "ENVIADO", codigoUsuario, "El usuario ha enviado la solicitud a revisión.");
            mensaje = ok ? "Solicitud enviada a la DGAC." : "Error al procesar el envío.";
            return ok;
        }

        public bool AsignarTecnico(int codigoSolicitud, int codigoTecnico, int codigoUsuario, out string mensaje)
        {
            mensaje = "";
            bool ok = _solicitudDAO.ActualizarTecnico(codigoSolicitud, codigoTecnico, codigoUsuario);

            if (ok)
            {
                _historialDAO.RegistrarCambio(codigoSolicitud, "ASIGNADO", "ASIGNADO", codigoUsuario, $"Técnico asignado (ID: {codigoTecnico})");
                mensaje = "Inspector asignado correctamente.";
            }
            else { mensaje = "No se pudo realizar la asignación."; }

            return ok;
        }

        // Resuelve error CS1061 en SolicitudController (Baja lógica)
        public bool EliminarSoft(int codigoSolicitud, int codigoUsuario, out string mensaje)
        {
            mensaje = "";
            var solicitud = _solicitudDAO.ObtenerPorId(codigoSolicitud);
            if (solicitud == null) { mensaje = "Registro no encontrado."; return false; }

            bool ok = _solicitudDAO.CambiarEstado(codigoSolicitud, "ELIMINADO", codigoUsuario, "Eliminación lógica solicitada por el usuario.");
            mensaje = ok ? "Registro eliminado con éxito." : "Error al intentar eliminar.";
            return ok;
        }

        // ==========================================
        // Actualizar con 4 argumentos (Resuelve Error CS1501)
        // ==========================================
        public bool Actualizar(SolicitudAOCR modelo, int codigoUsuario, out string mensaje, bool esAdmin = false)
        {
            mensaje = "";
            try
            {
                var actual = _solicitudDAO.ObtenerPorId(modelo.CodigoSolicitud);
                if (actual == null)
                {
                    mensaje = "Solicitud no encontrada.";
                    return false;
                }

                // Validación de seguridad: Solo el dueño o un Admin pueden editar
                if (!esAdmin && actual.CodigoUsuario != codigoUsuario)
                {
                    mensaje = "No tiene permisos para modificar esta solicitud.";
                    return false;
                }

                // Regla de negocio: Solo editar si está en BORRADOR (opcional, según tu flujo)
                if (!esAdmin && actual.Estado != "BORRADOR")
                {
                    mensaje = "La solicitud ya no se puede editar porque está en estado: " + actual.Estado;
                    return false;
                }

                modelo.UpdatedAt = DateTime.Now;
                modelo.UpdatedBy = codigoUsuario.ToString();

                bool ok = _solicitudDAO.ActualizarGeneral(modelo);
                if (ok) mensaje = "Solicitud actualizada correctamente.";

                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error al actualizar: " + ex.Message;
                return false;
            }
        }
        #endregion

        #region Métodos de Apoyo

        public string GenerarNumeroSolicitud(int year)
        {
            var total = _solicitudDAO.ListarActivas().Count(s => s.FechaSolicitud.Year == year);
            return $"AOCR-{year}-{(total + 1):D5}";
        }

        #endregion
    }
}