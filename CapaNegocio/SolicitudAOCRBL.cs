using System;
using System.Collections.Generic;
using CapaModelo;
using CapaDatos.DAOs;

namespace CapaNegocio
{
    public static class SolicitudAOCRBL
    {
        // ================================
        // 1. Crear Solicitud AOCR
        // ================================
        public static int CrearSolicitud(SolicitudAOCR solicitud, out string mensaje)
        {
            try
            {
                solicitud.FechaSolicitud = DateTime.Now;
                solicitud.Estado = "PENDIENTE";

                int id = new SolicitudAOCRDAO().InsertarConReturn(solicitud);
                mensaje = "Solicitud creada exitosamente.";
                return id;
            }
            catch (Exception ex)
            {
                mensaje = "Error al crear solicitud: " + ex.Message;
                return 0;
            }
        }

        // =======================================
        // 2. Obtener solicitudes por usuario
        // =======================================
        public static List<SolicitudAOCR> ListarPorUsuario(int codigoUsuario)
        {
            return new SolicitudAOCRDAO().ObtenerPorUsuario(codigoUsuario);
        }

        // =======================================
        // 3. Obtener por ID (para edición o revisión)
        // =======================================
        public static SolicitudAOCR ObtenerPorId(int id)
        {
            return new SolicitudAOCRDAO().ObtenerPorId(id);
        }

        // =======================================
        // 4. Actualizar solicitud completa
        // =======================================
        public static bool ActualizarSolicitud(SolicitudAOCR solicitud, out string mensaje)
        {
            try
            {
                bool ok = new SolicitudAOCRDAO().ActualizarGeneral(solicitud);
                mensaje = ok ? "Solicitud actualizada correctamente." : "No se pudo actualizar la solicitud.";
                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error al actualizar: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 5. Cambiar estado en cualquier punto del flujo
        // =====================================================
        public static bool CambiarEstado(
            int idSolicitud,
            string nuevoEstado,
            int codigoUsuario,
            string observaciones,
            out string mensaje)
        {
            try
            {
                bool ok = new SolicitudAOCRDAO().CambiarEstado(
                    idSolicitud, nuevoEstado, codigoUsuario, observaciones
                );

                mensaje = ok ? "Estado actualizado correctamente." : "No fue posible cambiar el estado.";
                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error cambiando estado: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 6. Eliminar solicitud (si aplica)
        // =====================================================
        public static bool Eliminar(int id, out string mensaje)
        {
            try
            {
                bool ok = new SolicitudAOCRDAO().Eliminar(id, out mensaje);
                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error al eliminar: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 7. Listar todas las solicitudes (para panel administrador)
        // =====================================================
        public static List<SolicitudAOCR> ListarActivas()
        {
            return new SolicitudAOCRDAO().ListarActivas();
        }

        // =====================================================
        // 8. Listar solicitudes por estado específico
        // =====================================================
        public static List<SolicitudAOCR> ListarPorEstado(string estado)
        {
            return new SolicitudAOCRDAO().ObtenerPorEstado(estado);
        }

        // =====================================================
        // 9. Listar solicitudes pendientes de revisión
        // =====================================================
        public static List<SolicitudAOCR> ListarPendientesRevision()
        {
            return new SolicitudAOCRDAO().ObtenerPendientesRevision();
        }

        // =====================================================
        // 10. Listar para validación de jefatura
        // =====================================================
        public static List<SolicitudAOCR> ListarParaValidacionJefatura()
        {
            return new SolicitudAOCRDAO().ObtenerParaValidacionJefatura();
        }
        // =====================================================
        // 11.MarcarParaInspeccion
        // =====================================================
        public static bool MarcarParaInspeccion(int idSolicitud)
        {
            try
            {
                var dao = new SolicitudAOCRDAO();
                return dao.CambiarEstado(idSolicitud, "INSPECCION_SOLICITADA", 0); // 0 = sistema o usuario genérico
            }
            catch
            {
                return false;
            }
        }

    }
}
