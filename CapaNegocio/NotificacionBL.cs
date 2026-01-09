// ==========================================================
// NotificacionBL.cs (CORREGIDO - SIN RESUMIR)
// ==========================================================
using System;
using System.Collections.Generic;
using System.Linq;
using CapaModelo;

// ✅ IMPORTA DAOs
using CapaDatos.DAOs;

// ✅ ALIAS EXPLÍCITO
using NotificacionDAOType = CapaDatos.DAOs.NotificacionDAO;

namespace CapaNegocio
{
    public class NotificacionBL
    {
        #region Creación de Notificaciones

        public static bool Insertar(Notificacion notificacion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarNotificacion(notificacion, out mensaje))
                    return false;

                notificacion.FechaCreacion = DateTime.Now;
                notificacion.Leida = false;

                bool resultado = NotificacionDAOType.Insertar(notificacion);

                if (resultado)
                {
                    mensaje = "Notificación creada exitosamente.";
                    LogBL.RegistrarInfo($"Notificación creada para usuario {notificacion.CodigoUsuario}", "Notificacion");
                }
                else
                {
                    mensaje = "Error al crear la notificación.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar notificación", ex.ToString(), "Notificacion");
                return false;
            }
        }

        public static bool EnviarNotificacion(int codigoUsuario, string titulo, string mensaje,
            string tipo = "INFO", string url = null)
        {
            try
            {
                var notificacion = new Notificacion
                {
                    CodigoUsuario = codigoUsuario,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = tipo,
                    Url = url,
                    FechaCreacion = DateTime.Now,
                    Leida = false
                };

                return NotificacionDAOType.Insertar(notificacion);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al enviar notificación", ex.ToString(), "Notificacion");
                return false;
            }
        }

        public static bool EnviarNotificacionMasiva(List<int> codigosUsuarios, string titulo,
            string mensaje, string tipo = "INFO")
        {
            try
            {
                int exitosas = 0;

                foreach (var codigoUsuario in codigosUsuarios)
                {
                    if (EnviarNotificacion(codigoUsuario, titulo, mensaje, tipo))
                        exitosas++;
                }

                LogBL.RegistrarInfo($"Notificaciones masivas enviadas: {exitosas}/{codigosUsuarios.Count}", "Notificacion");
                return exitosas > 0;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al enviar notificaciones masivas", ex.ToString(), "Notificacion");
                return false;
            }
        }

        #endregion

        #region Gestión de Notificaciones

        public static bool MarcarComoLeida(int codigoNotificacion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = NotificacionDAOType.MarcarComoLeida(codigoNotificacion);

                if (resultado)
                {
                    mensaje = "Notificación marcada como leída.";
                }
                else
                {
                    mensaje = "Error al marcar notificación como leída.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al marcar notificación como leída", ex.ToString(), "Notificacion");
                return false;
            }
        }

        public static bool MarcarTodasComoLeidas(int codigoUsuario, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = NotificacionDAOType.MarcarTodasComoLeidas(codigoUsuario);

                if (resultado)
                {
                    mensaje = "Todas las notificaciones marcadas como leídas.";
                    LogBL.RegistrarInfo($"Notificaciones marcadas como leídas para usuario {codigoUsuario}", "Notificacion");
                }
                else
                {
                    mensaje = "No hay notificaciones para marcar.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al marcar todas como leídas", ex.ToString(), "Notificacion");
                return false;
            }
        }

        public static bool Eliminar(int codigoNotificacion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = NotificacionDAOType.Eliminar(codigoNotificacion);

                if (resultado)
                {
                    mensaje = "Notificación eliminada exitosamente.";
                }
                else
                {
                    mensaje = "Error al eliminar la notificación.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar notificación", ex.ToString(), "Notificacion");
                return false;
            }
        }

        #endregion

        #region Consultas

        public static List<Notificacion> ObtenerPorUsuario(int codigoUsuario)
        {
            try
            {
                return NotificacionDAOType.ObtenerPorUsuario(codigoUsuario);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener notificaciones por usuario", ex.ToString(), "Notificacion");
                return new List<Notificacion>();
            }
        }

        public static List<Notificacion> ObtenerNoLeidas(int codigoUsuario)
        {
            try
            {
                return NotificacionDAOType.ObtenerNoLeidas(codigoUsuario);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener notificaciones no leídas", ex.ToString(), "Notificacion");
                return new List<Notificacion>();
            }
        }

        public static int ContarNoLeidas(int codigoUsuario)
        {
            try
            {
                return NotificacionDAOType.ContarNoLeidas(codigoUsuario);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al contar notificaciones no leídas", ex.ToString(), "Notificacion");
                return 0;
            }
        }

        public static List<Notificacion> ObtenerPorTipo(int codigoUsuario, string tipo)
        {
            try
            {
                return NotificacionDAOType.ObtenerPorTipo(codigoUsuario, tipo);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener notificaciones por tipo", ex.ToString(), "Notificacion");
                return new List<Notificacion>();
            }
        }

        public static List<Notificacion> ObtenerRecientes(int codigoUsuario, int cantidad = 10)
        {
            try
            {
                return NotificacionDAOType.ObtenerRecientes(codigoUsuario, cantidad);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener notificaciones recientes", ex.ToString(), "Notificacion");
                return new List<Notificacion>();
            }
        }

        #endregion

        #region Notificaciones Específicas del Sistema

        public static bool NotificarNuevaSolicitud(int codigoUsuario, int codigoSolicitud)
        {
            return EnviarNotificacion(
                codigoUsuario,
                "Nueva Solicitud",
                $"Se ha registrado una nueva solicitud #{codigoSolicitud}",
                "INFO",
                $"/solicitudes/detalle/{codigoSolicitud}"
            );
        }

        public static bool NotificarCambioEstado(int codigoUsuario, int codigoSolicitud, string nuevoEstado)
        {
            return EnviarNotificacion(
                codigoUsuario,
                "Cambio de Estado",
                $"La solicitud #{codigoSolicitud} cambió a estado: {nuevoEstado}",
                "SUCCESS",
                $"/solicitudes/detalle/{codigoSolicitud}"
            );
        }

        public static bool NotificarInspeccionProgramada(int codigoUsuario, int codigoInspeccion, DateTime fechaInspeccion)
        {
            return EnviarNotificacion(
                codigoUsuario,
                "Inspección Programada",
                $"Inspección #{codigoInspeccion} programada para {fechaInspeccion:dd/MM/yyyy HH:mm}",
                "WARNING",
                $"/inspecciones/detalle/{codigoInspeccion}"
            );
        }

        public static bool NotificarPagoRecibido(int codigoUsuario, int codigoPago, decimal monto)
        {
            return EnviarNotificacion(
                codigoUsuario,
                "Pago Recibido",
                $"Se ha registrado un pago de ${monto:N2}",
                "SUCCESS",
                $"/pagos/detalle/{codigoPago}"
            );
        }

        public static bool NotificarVencimientoCertificado(int codigoUsuario, int codigoCertificado, DateTime fechaVencimiento)
        {
            int diasRestantes = (fechaVencimiento - DateTime.Now).Days;
            string tipo = diasRestantes <= 7 ? "ERROR" : "WARNING";

            return EnviarNotificacion(
                codigoUsuario,
                "Vencimiento de Certificado",
                $"El certificado #{codigoCertificado} vence en {diasRestantes} días ({fechaVencimiento:dd/MM/yyyy})",
                tipo,
                $"/certificados/detalle/{codigoCertificado}"
            );
        }

        public static bool NotificarHallazgoCritico(int codigoUsuario, int codigoHallazgo, string descripcion)
        {
            return EnviarNotificacion(
                codigoUsuario,
                "Hallazgo Crítico",
                $"Se ha detectado un hallazgo crítico: {descripcion}",
                "ERROR",
                $"/hallazgos/detalle/{codigoHallazgo}"
            );
        }

        #endregion

        #region Validaciones

        private static bool ValidarNotificacion(Notificacion notificacion, out string mensaje)
        {
            mensaje = string.Empty;

            if (notificacion == null)
            {
                mensaje = "Los datos de la notificación son requeridos.";
                return false;
            }

            // ⚠️ Aquí estaba el error: CodigoUsuario es int, NO int?
            if (notificacion.CodigoUsuario <= 0)
            {
                mensaje = "El código de usuario es requerido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(notificacion.Titulo))
            {
                mensaje = "El título es requerido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(notificacion.Mensaje))
            {
                mensaje = "El mensaje es requerido.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(notificacion.Tipo))
            {
                var tiposPermitidos = new[] { "INFO", "SUCCESS", "WARNING", "ERROR" };
                if (!tiposPermitidos.Contains(notificacion.Tipo.ToUpper()))
                {
                    mensaje = "Tipo de notificación no válido. Debe ser: INFO, SUCCESS, WARNING o ERROR.";
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Mantenimiento

        public static bool LimpiarNotificacionesAntiguas(int diasAntiguedad, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = NotificacionDAOType.LimpiarNotificacionesAntiguas(diasAntiguedad);

                if (resultado)
                {
                    mensaje = $"Notificaciones anteriores a {diasAntiguedad} días eliminadas exitosamente.";
                    LogBL.RegistrarInfo($"Limpieza de notificaciones: {diasAntiguedad} días", "Notificacion");
                }
                else
                {
                    mensaje = "No se encontraron notificaciones para eliminar.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al limpiar notificaciones antiguas", ex.ToString(), "Notificacion");
                return false;
            }
        }

        #endregion
    }
}
