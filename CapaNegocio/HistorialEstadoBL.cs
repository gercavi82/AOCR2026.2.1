using System;
using System.Collections.Generic;
using System.Linq;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    /// <summary>
    /// Lógica de Negocio para el Historial de Estados
    /// Arquitectura: Instancia (Corregido)
    /// </summary>
    public class HistorialEstadoBL
    {
        // ✅ 1. Variable para el DAO
        private readonly HistorialEstadoDAO _historialDAO;

        // ✅ 2. Constructor para inicializar
        public HistorialEstadoBL()
        {
            _historialDAO = new HistorialEstadoDAO();
        }

        #region Registro de Cambios de Estado

        // ✅ 3. Quitamos 'static' de TODOS los métodos y usamos '_historialDAO'

        public bool RegistrarCambioEstado(HistorialEstado historial, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarHistorial(historial, out mensaje))
                    return false;

                historial.FechaCambio = DateTime.Now;

                // Uso de la instancia _historialDAO
                bool resultado = _historialDAO.Insertar(historial);

                if (resultado)
                {
                    mensaje = "Cambio de estado registrado exitosamente.";
                    LogBL.RegistrarInfo(
                        $"Cambio de estado: Solicitud {historial.CodigoSolicitud} -> {historial.EstadoNuevo}",
                        "HistorialEstado");
                }
                else
                {
                    mensaje = "Error al registrar cambio de estado.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al registrar cambio de estado", ex.ToString(), "HistorialEstado");
                return false;
            }
        }

        public bool RegistrarCambio(int codigoSolicitud, string estadoAnterior, string estadoNuevo,
            int? codigoUsuario, string observaciones = null)
        {
            try
            {
                // Manejo de nulo para usuario (por si viene null, ponemos 0 o un ID genérico)
                int idUsuario = codigoUsuario ?? 0;

                var historial = new HistorialEstado
                {
                    CodigoSolicitud = codigoSolicitud,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = estadoNuevo,
                    CodigoUsuario = idUsuario,
                    Observaciones = observaciones,
                    FechaCambio = DateTime.Now
                };

                // Uso de la instancia _historialDAO
                return _historialDAO.Insertar(historial);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al registrar cambio rápido de estado", ex.ToString(), "HistorialEstado");
                return false;
            }
        }

        #endregion

        #region Consultas

        public List<HistorialEstado> ObtenerPorSolicitud(int codigoSolicitud)
        {
            try
            {
                return _historialDAO.ObtenerPorSolicitud(codigoSolicitud);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener historial por solicitud", ex.ToString(), "HistorialEstado");
                return new List<HistorialEstado>();
            }
        }

        public HistorialEstado ObtenerUltimoCambio(int codigoSolicitud)
        {
            try
            {
                return _historialDAO.ObtenerUltimoCambio(codigoSolicitud);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener último cambio", ex.ToString(), "HistorialEstado");
                return null;
            }
        }

        public List<HistorialEstado> ObtenerPorEstado(string estado)
        {
            try
            {
                return _historialDAO.ObtenerPorEstado(estado);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener historial por estado", ex.ToString(), "HistorialEstado");
                return new List<HistorialEstado>();
            }
        }

        public List<HistorialEstado> ObtenerPorUsuario(int codigoUsuario)
        {
            try
            {
                return _historialDAO.ObtenerPorUsuario(codigoUsuario);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener historial por usuario", ex.ToString(), "HistorialEstado");
                return new List<HistorialEstado>();
            }
        }

        public List<HistorialEstado> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return _historialDAO.ObtenerPorFecha(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener historial por fecha", ex.ToString(), "HistorialEstado");
                return new List<HistorialEstado>();
            }
        }

        #endregion

        #region Análisis y Reportes

        public Dictionary<string, int> ObtenerEstadisticasCambios(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var historiales = _historialDAO.ObtenerPorFecha(fechaInicio, fechaFin);
                var estadisticas = new Dictionary<string, int>();

                foreach (var historial in historiales)
                {
                    string transicion = $"{historial.EstadoAnterior} -> {historial.EstadoNuevo}";
                    if (estadisticas.ContainsKey(transicion))
                        estadisticas[transicion]++;
                    else
                        estadisticas[transicion] = 1;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener estadísticas de cambios", ex.ToString(), "HistorialEstado");
                return new Dictionary<string, int>();
            }
        }

        public int CalcularTiempoPromedioEnEstado(string estado)
        {
            try
            {
                var historiales = _historialDAO.ObtenerPorEstado(estado)
                    .OrderBy(h => h.CodigoSolicitud)
                    .ThenBy(h => h.FechaCambio)
                    .ToList();

                var tiempos = new List<int>();

                for (int i = 0; i < historiales.Count() - 1; i++)
                {
                    // ✅ CORRECCIÓN: Usamos .HasValue y .Value para manejar los nulos
                    if (historiales[i].CodigoSolicitud == historiales[i + 1].CodigoSolicitud &&
                        historiales[i].FechaCambio.HasValue &&
                        historiales[i + 1].FechaCambio.HasValue)
                    {
                        // Al usar .Value obtenemos el DateTime exacto y la resta devuelve un TimeSpan (no nulo)
                        var span = historiales[i + 1].FechaCambio.Value - historiales[i].FechaCambio.Value;
                        var diferencia = span.TotalHours;

                        tiempos.Add((int)diferencia);
                    }
                }

                return tiempos.Any() ? (int)tiempos.Average() : 0;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al calcular tiempo promedio", ex.ToString(), "HistorialEstado");
                return 0;
            }
        }

        public string ObtenerEstadoActual(int codigoSolicitud)
        {
            try
            {
                var ultimoCambio = _historialDAO.ObtenerUltimoCambio(codigoSolicitud);
                return ultimoCambio?.EstadoNuevo ?? "Desconocido";
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener estado actual", ex.ToString(), "HistorialEstado");
                return "Error";
            }
        }

        public List<string> ObtenerFlujoProceso(int codigoSolicitud)
        {
            try
            {
                var historiales = _historialDAO.ObtenerPorSolicitud(codigoSolicitud)
                    .OrderBy(h => h.FechaCambio)
                    .ToList();

                var flujo = new List<string>();

                if (historiales.Any() && !string.IsNullOrEmpty(historiales[0].EstadoAnterior))
                    flujo.Add(historiales[0].EstadoAnterior);

                foreach (var h in historiales)
                {
                    if (!string.IsNullOrEmpty(h.EstadoNuevo))
                        flujo.Add(h.EstadoNuevo);
                }

                return flujo;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener flujo de proceso", ex.ToString(), "HistorialEstado");
                return new List<string>();
            }
        }

        #endregion

        #region Validaciones

        // Nota: Quitamos static también aquí para mantener consistencia,
        // aunque al ser privado no afectaría externamente.
        private bool ValidarHistorial(HistorialEstado historial, out string mensaje)
        {
            mensaje = string.Empty;

            if (historial == null)
            {
                mensaje = "Los datos del historial son requeridos.";
                return false;
            }

            if (!historial.CodigoSolicitud.HasValue || historial.CodigoSolicitud <= 0)
            {
                mensaje = "El código de solicitud es requerido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(historial.EstadoNuevo))
            {
                mensaje = "El estado nuevo es requerido.";
                return false;
            }

            var estadosPermitidos = new[]
            {
                "Pendiente", "En Revisión", "Aprobado", "Rechazado",
                "En Proceso", "Completado", "Cancelado",
                "BORRADOR", "ENVIADO", "ELIMINADO", "EN_REVISION_TECNICA", "DOCUMENTOS_COMPLETOS" // Agregados los estados de tu lógica
            };

            // Validación flexible: convertimos a mayúsculas para comparar
            string estadoNuevoNorm = historial.EstadoNuevo.ToUpper();

            // Si quieres ser estricto con la lista:
            /*
            if (!estadosPermitidos.Any(e => e.ToUpper() == estadoNuevoNorm))
            {
                mensaje = $"Estado no válido: {historial.EstadoNuevo}";
                return false;
            }
            */

            return true;
        }

        #endregion
    }
}