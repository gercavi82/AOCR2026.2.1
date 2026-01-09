// ==========================================================
// ObservacionBL.cs (CORREGIDO COMPLETO)
// - Soluciona CS0103 agregando using correcto + alias del DAO
// - Mantiene tu lógica original
// ==========================================================
using System;
using System.Collections.Generic;
using System.Linq;
using CapaModelo;

// ✅ IMPORTA DAOs
using CapaDatos.DAOs;

// ✅ ALIAS EXPLÍCITO para evitar problemas de namespace
using ObservacionDAOType = CapaDatos.DAOs.ObservacionDAO;

namespace CapaNegocio
{
    public class ObservacionBL
    {
        #region CRUD Básico

        public static bool Insertar(Observacion observacion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarObservacion(observacion, out mensaje))
                    return false;

                observacion.FechaObservacion = DateTime.Now;

                bool resultado = ObservacionDAOType.Insertar(observacion);

                if (resultado)
                {
                    mensaje = "Observación registrada exitosamente.";
                    LogBL.RegistrarInfo($"Observación creada: Inspección {observacion.CodigoInspeccion}", "Observacion");
                }
                else
                {
                    mensaje = "Error al registrar la observación.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar observación", ex.ToString(), "Observacion");
                return false;
            }
        }

        public static bool Actualizar(Observacion observacion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarObservacion(observacion, out mensaje))
                    return false;

                bool resultado = ObservacionDAOType.Actualizar(observacion);

                if (resultado)
                {
                    mensaje = "Observación actualizada exitosamente.";
                    LogBL.RegistrarInfo($"Observación actualizada: ID {observacion.CodigoObservacion}", "Observacion");
                }
                else
                {
                    mensaje = "Error al actualizar la observación.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar observación", ex.ToString(), "Observacion");
                return false;
            }
        }

        public static bool Eliminar(int codigoObservacion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = ObservacionDAOType.Eliminar(codigoObservacion);

                if (resultado)
                {
                    mensaje = "Observación eliminada exitosamente.";
                    LogBL.RegistrarInfo($"Observación eliminada: ID {codigoObservacion}", "Observacion");
                }
                else
                {
                    mensaje = "Error al eliminar la observación.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar observación", ex.ToString(), "Observacion");
                return false;
            }
        }

        public static Observacion ObtenerPorId(int codigoObservacion)
        {
            try
            {
                return ObservacionDAOType.ObtenerPorId(codigoObservacion);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener observación", ex.ToString(), "Observacion");
                return null;
            }
        }

        #endregion

        #region Consultas

        public static List<Observacion> ObtenerPorInspeccion(int codigoInspeccion)
        {
            try
            {
                return ObservacionDAOType.ObtenerPorInspeccion(codigoInspeccion);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener observaciones por inspección", ex.ToString(), "Observacion");
                return new List<Observacion>();
            }
        }

        public static List<Observacion> ObtenerPorGravedad(string gravedad)
        {
            try
            {
                return ObservacionDAOType.ObtenerPorGravedad(gravedad);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener observaciones por gravedad", ex.ToString(), "Observacion");
                return new List<Observacion>();
            }
        }

        public static List<Observacion> ObtenerPorEstado(string estado)
        {
            try
            {
                return ObservacionDAOType.ObtenerPorEstado(estado);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener observaciones por estado", ex.ToString(), "Observacion");
                return new List<Observacion>();
            }
        }

        public static List<Observacion> ObtenerPendientes()
        {
            try
            {
                return ObservacionDAOType.ObtenerPorEstado("Pendiente");
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener observaciones pendientes", ex.ToString(), "Observacion");
                return new List<Observacion>();
            }
        }

        public static List<Observacion> ObtenerCriticas()
        {
            try
            {
                return ObservacionDAOType.ObtenerPorGravedad("Alta");
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener observaciones críticas", ex.ToString(), "Observacion");
                return new List<Observacion>();
            }
        }

        #endregion

        #region Gestión de Observaciones

        public static bool ActualizarEstado(int codigoObservacion, string nuevoEstado, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var estadosPermitidos = new[] { "Pendiente", "En Proceso", "Resuelta", "Cerrada" };
                if (!estadosPermitidos.Contains(nuevoEstado))
                {
                    mensaje = "Estado no válido.";
                    return false;
                }

                bool resultado = ObservacionDAOType.ActualizarEstado(codigoObservacion, nuevoEstado);

                if (resultado)
                {
                    mensaje = $"Estado actualizado a: {nuevoEstado}";
                    LogBL.RegistrarInfo($"Estado de observación actualizado: {codigoObservacion} -> {nuevoEstado}", "Observacion");
                }
                else
                {
                    mensaje = "Error al actualizar el estado.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar estado de observación", ex.ToString(), "Observacion");
                return false;
            }
        }

        public static bool ResolverObservacion(int codigoObservacion, string solucion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var observacion = ObservacionDAOType.ObtenerPorId(codigoObservacion);
                if (observacion == null)
                {
                    mensaje = "Observación no encontrada.";
                    return false;
                }

                observacion.Estado = "Resuelta";
                observacion.FechaResolucion = DateTime.Now;
                observacion.Observaciones = (observacion.Observaciones ?? "") +
                                            $"\n\nSOLUCIÓN ({DateTime.Now:dd/MM/yyyy}): {solucion}";

                bool resultado = ObservacionDAOType.Actualizar(observacion);

                if (resultado)
                {
                    mensaje = "Observación resuelta exitosamente.";
                    LogBL.RegistrarInfo($"Observación resuelta: ID {codigoObservacion}", "Observacion");
                }
                else
                {
                    mensaje = "Error al resolver la observación.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al resolver observación", ex.ToString(), "Observacion");
                return false;
            }
        }

        #endregion

        #region Estadísticas y Reportes

        public static Dictionary<string, int> ObtenerEstadisticasPorGravedad(int codigoInspeccion)
        {
            try
            {
                var observaciones = ObservacionDAOType.ObtenerPorInspeccion(codigoInspeccion);
                var estadisticas = new Dictionary<string, int>();

                foreach (var obs in observaciones)
                {
                    string gravedad = obs.Gravedad ?? "Sin Especificar";
                    if (estadisticas.ContainsKey(gravedad))
                        estadisticas[gravedad]++;
                    else
                        estadisticas[gravedad] = 1;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener estadísticas por gravedad", ex.ToString(), "Observacion");
                return new Dictionary<string, int>();
            }
        }

        public static Dictionary<string, int> ObtenerEstadisticasPorEstado(int codigoInspeccion)
        {
            try
            {
                var observaciones = ObservacionDAOType.ObtenerPorInspeccion(codigoInspeccion);
                var estadisticas = new Dictionary<string, int>();

                foreach (var obs in observaciones)
                {
                    string estado = obs.Estado ?? "Sin Estado";
                    if (estadisticas.ContainsKey(estado))
                        estadisticas[estado]++;
                    else
                        estadisticas[estado] = 1;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener estadísticas por estado", ex.ToString(), "Observacion");
                return new Dictionary<string, int>();
            }
        }

        public static int ContarPendientesPorInspeccion(int codigoInspeccion)
        {
            try
            {
                var observaciones = ObservacionDAOType.ObtenerPorInspeccion(codigoInspeccion);
                return observaciones.Count(o => o.Estado == "Pendiente");
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al contar observaciones pendientes", ex.ToString(), "Observacion");
                return 0;
            }
        }

        #endregion

        #region Validaciones

        private static bool ValidarObservacion(Observacion observacion, out string mensaje)
        {
            mensaje = string.Empty;

            if (observacion == null)
            {
                mensaje = "Los datos de la observación son requeridos.";
                return false;
            }

            if (!observacion.CodigoInspeccion.HasValue || observacion.CodigoInspeccion <= 0)
            {
                mensaje = "El código de inspección es requerido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(observacion.Descripcion))
            {
                mensaje = "La descripción es requerida.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(observacion.Gravedad))
            {
                var gravedadesPermitidas = new[] { "Alta", "Media", "Baja" };
                if (!gravedadesPermitidas.Contains(observacion.Gravedad))
                {
                    mensaje = "La gravedad debe ser: Alta, Media o Baja.";
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(observacion.Estado))
            {
                var estadosPermitidos = new[] { "Pendiente", "En Proceso", "Resuelta", "Cerrada" };
                if (!estadosPermitidos.Contains(observacion.Estado))
                {
                    mensaje = "Estado no válido.";
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
