// ==========================================================
// LogBL.cs (VERSIÓN 100% COMPATIBLE CON TU MODELO ACTUAL)
// ✅ SOLUCIONA:
//   - CS0117 Log no contiene Modulo
//   - CS0117 Log no contiene FechaHora
//   - CS0117 Log no contiene DetalleError
//   - Evita depender de métodos no existentes en LogDAO
//
// ✅ Esta versión SOLO depende de:
//    - LogDAO.Insertar(Log)
//    - LogDAO.ObtenerPorNivel(string, int)
//    - LogDAO.ObtenerPorFecha(DateTime, DateTime)
//
// 📌 Estrategia:
//    - Guarda "modulo" y "detalleError" dentro del Mensaje:
//        [MOD: X] [DET: Y] Mensaje...
//    - Consultas por módulo/usuario/últimos/errores se resuelven
//      en memoria usando ObtenerPorFecha/ObtenerPorNivel.
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using CapaModelo;
using CapaDatos.DAOs;

// Alias explícito
using LogDAOType = CapaDatos.DAOs.LogDAO;

namespace CapaNegocio
{
    public class LogBL
    {
        // ======================================================
        // Helpers internos
        // ======================================================

        private static string FormatearMensaje(string mensaje, string modulo, string detalleError)
        {
            string m = mensaje ?? string.Empty;

            // Prefijo módulo
            if (!string.IsNullOrWhiteSpace(modulo))
                m = $"[MOD:{modulo}] " + m;

            // Prefijo detalle
            if (!string.IsNullOrWhiteSpace(detalleError))
                m = $"[DET:{detalleError}] " + m;

            return m;
        }

        private static string ExtraerModuloDesdeMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                return null;

            // Busca patrón [MOD:xxx]
            const string tag = "[MOD:";
            int idx = mensaje.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            int start = idx + tag.Length;
            int end = mensaje.IndexOf("]", start);
            if (end <= start) return null;

            return mensaje.Substring(start, end - start).Trim();
        }

        private static List<Log> ObtenerLogsRangoAmplio(int dias = 365)
        {
            DateTime fin = DateTime.Now;
            DateTime ini = fin.AddDays(-Math.Abs(dias));

            try
            {
                return LogDAOType.ObtenerPorFecha(ini, fin) ?? new List<Log>();
            }
            catch
            {
                return new List<Log>();
            }
        }

        // ======================================================
        // Registro de Logs
        // ======================================================

        public static bool RegistrarLog(Log log)
        {
            try
            {
                if (log == null)
                    return false;

                // ✅ No usar propiedades inexistentes
                return LogDAOType.Insertar(log);
            }
            catch
            {
                return false;
            }
        }

        public static bool RegistrarInfo(string mensaje, string modulo = null, int? codigoUsuario = null)
        {
            try
            {
                var log = new Log
                {
                    Nivel = "INFO",
                    Mensaje = FormatearMensaje(mensaje, modulo, null),
                    CodigoUsuario = codigoUsuario
                };

                return LogDAOType.Insertar(log);
            }
            catch
            {
                return false;
            }
        }

        public static bool RegistrarAdvertencia(string mensaje, string modulo = null, int? codigoUsuario = null)
        {
            try
            {
                var log = new Log
                {
                    Nivel = "WARNING",
                    Mensaje = FormatearMensaje(mensaje, modulo, null),
                    CodigoUsuario = codigoUsuario
                };

                return LogDAOType.Insertar(log);
            }
            catch
            {
                return false;
            }
        }

        public static bool RegistrarError(string mensaje, string detalleError = null, string modulo = null, int? codigoUsuario = null)
        {
            try
            {
                var log = new Log
                {
                    Nivel = "ERROR",
                    // ✅ DetalleError se guarda dentro del Mensaje
                    Mensaje = FormatearMensaje(mensaje, modulo, detalleError),
                    CodigoUsuario = codigoUsuario
                };

                return LogDAOType.Insertar(log);
            }
            catch
            {
                return false;
            }
        }

        public static bool RegistrarCritico(string mensaje, string detalleError = null, string modulo = null, int? codigoUsuario = null)
        {
            try
            {
                var log = new Log
                {
                    Nivel = "CRITICAL",
                    // ✅ DetalleError se guarda dentro del Mensaje
                    Mensaje = FormatearMensaje(mensaje, modulo, detalleError),
                    CodigoUsuario = codigoUsuario
                };

                return LogDAOType.Insertar(log);
            }
            catch
            {
                return false;
            }
        }

        public static bool RegistrarDebug(string mensaje, string modulo = null, int? codigoUsuario = null)
        {
            try
            {
                var log = new Log
                {
                    Nivel = "DEBUG",
                    Mensaje = FormatearMensaje(mensaje, modulo, null),
                    CodigoUsuario = codigoUsuario
                };

                return LogDAOType.Insertar(log);
            }
            catch
            {
                return false;
            }
        }

        // ======================================================
        // Consultas (compatibles)
        // ======================================================

        public static List<Log> ObtenerPorNivel(string nivel, int limite = 100)
        {
            try
            {
                return LogDAOType.ObtenerPorNivel(nivel, limite) ?? new List<Log>();
            }
            catch
            {
                return new List<Log>();
            }
        }

        public static List<Log> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return LogDAOType.ObtenerPorFecha(fechaInicio, fechaFin) ?? new List<Log>();
            }
            catch
            {
                return new List<Log>();
            }
        }

        // ======================================================
        // Consultas "virtuales"
        // ======================================================

        public static List<Log> ObtenerPorModulo(string modulo, int limite = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modulo))
                    return new List<Log>();

                var logs = ObtenerLogsRangoAmplio(365);

                return logs
                    .Where(l =>
                    {
                        var m = ExtraerModuloDesdeMensaje(l.Mensaje);
                        return !string.IsNullOrWhiteSpace(m) &&
                               string.Equals(m, modulo, StringComparison.OrdinalIgnoreCase);
                    })
                    .OrderByDescending(l => l.CodigoLog)
                    .Take(limite)
                    .ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }

        public static List<Log> ObtenerPorUsuario(int codigoUsuario, int limite = 100)
        {
            try
            {
                var logs = ObtenerLogsRangoAmplio(365);

                return logs
                    .Where(l => l.CodigoUsuario.HasValue && l.CodigoUsuario.Value == codigoUsuario)
                    .OrderByDescending(l => l.CodigoLog)
                    .Take(limite)
                    .ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }

        public static List<Log> ObtenerErrores(int limite = 100)
        {
            try
            {
                var errores = ObtenerPorNivel("ERROR", limite);
                var criticos = ObtenerPorNivel("CRITICAL", limite);

                return errores
                    .Concat(criticos)
                    .OrderByDescending(l => l.CodigoLog)
                    .Take(limite)
                    .ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }

        public static List<Log> ObtenerUltimos(int cantidad = 50)
        {
            try
            {
                var logs = ObtenerLogsRangoAmplio(365);

                return logs
                    .OrderByDescending(l => l.CodigoLog)
                    .Take(cantidad)
                    .ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }

        // ======================================================
        // Estadísticas
        // ======================================================

        public static Dictionary<string, int> ObtenerEstadisticasPorNivel(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var logs = ObtenerPorFecha(fechaInicio, fechaFin);
                var estadisticas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var log in logs)
                {
                    string nivel = string.IsNullOrWhiteSpace(log.Nivel) ? "SIN NIVEL" : log.Nivel;
                    if (estadisticas.ContainsKey(nivel))
                        estadisticas[nivel]++;
                    else
                        estadisticas[nivel] = 1;
                }

                return estadisticas;
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        public static Dictionary<string, int> ObtenerEstadisticasPorModulo(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var logs = ObtenerPorFecha(fechaInicio, fechaFin);
                var estadisticas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var log in logs)
                {
                    string modulo = ExtraerModuloDesdeMensaje(log.Mensaje) ?? "SIN MÓDULO";

                    if (estadisticas.ContainsKey(modulo))
                        estadisticas[modulo]++;
                    else
                        estadisticas[modulo] = 1;
                }

                return estadisticas;
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        public static int ContarErroresRecientes(int horas = 24)
        {
            try
            {
                DateTime fin = DateTime.Now;
                DateTime ini = fin.AddHours(-Math.Abs(horas));

                var logs = ObtenerPorFecha(ini, fin);

                return logs.Count(l =>
                    string.Equals(l.Nivel, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(l.Nivel, "CRITICAL", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return 0;
            }
        }

        // ======================================================
        // Mantenimiento (sin soporte DAO)
        // ======================================================

        public static bool LimpiarLogsAntiguos(int diasAntiguedad, out string mensaje)
        {
            mensaje = "Operación no disponible en esta implementación de LogDAO.";
            return false;
        }

        public static bool LimpiarLogsPorNivel(string nivel, int diasAntiguedad, out string mensaje)
        {
            mensaje = "Operación no disponible en esta implementación de LogDAO.";
            return false;
        }
    }
}
