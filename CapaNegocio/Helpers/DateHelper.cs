using System;
using System.Globalization;

namespace CapaNegocio.Helpers
{
    /// <summary>
    /// Helper para manejo de fechas
    /// </summary>
    public static class DateHelper
    {
        private static readonly CultureInfo CulturaEspanol = new CultureInfo("es-CO");

        /// <summary>
        /// Formatea una fecha en formato largo en español
        /// </summary>
        /// <param name="fecha">Fecha a formatear</param>
        /// <returns>Fecha formateada</returns>
        public static string FormatearFechaLarga(DateTime fecha)
        {
            return fecha.ToString("dddd, dd 'de' MMMM 'de' yyyy", CulturaEspanol);
        }

        /// <summary>
        /// Formatea una fecha en formato corto
        /// </summary>
        /// <param name="fecha">Fecha a formatear</param>
        /// <returns>Fecha formateada</returns>
        public static string FormatearFechaCorta(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy", CulturaEspanol);
        }

        /// <summary>
        /// Formatea una fecha con hora
        /// </summary>
        /// <param name="fecha">Fecha a formatear</param>
        /// <returns>Fecha con hora formateada</returns>
        public static string FormatearFechaHora(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy HH:mm", CulturaEspanol);
        }

        /// <summary>
        /// Calcula la diferencia en días entre dos fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        /// <returns>Diferencia en días</returns>
        public static int DiferenciaEnDias(DateTime fechaInicio, DateTime fechaFin)
        {
            return (fechaFin - fechaInicio).Days;
        }

        /// <summary>
        /// Obtiene el nombre del mes en español
        /// </summary>
        /// <param name="mes">Número del mes (1-12)</param>
        /// <returns>Nombre del mes</returns>
        public static string ObtenerNombreMes(int mes)
        {
            return CulturaEspanol.DateTimeFormat.GetMonthName(mes);
        }

        /// <summary>
        /// Obtiene el primer día del mes
        /// </summary>
        /// <param name="fecha">Fecha de referencia</param>
        /// <returns>Primer día del mes</returns>
        public static DateTime PrimerDiaDelMes(DateTime fecha)
        {
            return new DateTime(fecha.Year, fecha.Month, 1);
        }

        /// <summary>
        /// Obtiene el último día del mes
        /// </summary>
        /// <param name="fecha">Fecha de referencia</param>
        /// <returns>Último día del mes</returns>
        public static DateTime UltimoDiaDelMes(DateTime fecha)
        {
            return new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
        }

        /// <summary>
        /// Verifica si una fecha es día hábil (lunes a viernes)
        /// </summary>
        /// <param name="fecha">Fecha a verificar</param>
        /// <returns>True si es día hábil</returns>
        public static bool EsDiaHabil(DateTime fecha)
        {
            return fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Calcula la edad en años
        /// </summary>
        /// <param name="fechaNacimiento">Fecha de nacimiento</param>
        /// <returns>Edad en años</returns>
        public static int CalcularEdad(DateTime fechaNacimiento)
        {
            int edad = DateTime.Now.Year - fechaNacimiento.Year;

            if (DateTime.Now.Month < fechaNacimiento.Month ||
                (DateTime.Now.Month == fechaNacimiento.Month && DateTime.Now.Day < fechaNacimiento.Day))
            {
                edad--;
            }

            return edad;
        }

        /// <summary>
        /// Obtiene una descripción relativa del tiempo transcurrido
        /// </summary>
        /// <param name="fecha">Fecha de referencia</param>
        /// <returns>Descripción relativa (ej: "hace 2 horas")</returns>
        public static string TiempoTranscurrido(DateTime fecha)
        {
            TimeSpan diferencia = DateTime.Now - fecha;

            if (diferencia.TotalMinutes < 1)
                return "hace unos segundos";

            if (diferencia.TotalMinutes < 60)
                return $"hace {(int)diferencia.TotalMinutes} minuto(s)";

            if (diferencia.TotalHours < 24)
                return $"hace {(int)diferencia.TotalHours} hora(s)";

            if (diferencia.TotalDays < 30)
                return $"hace {(int)diferencia.TotalDays} día(s)";

            if (diferencia.TotalDays < 365)
                return $"hace {(int)(diferencia.TotalDays / 30)} mes(es)";

            return $"hace {(int)(diferencia.TotalDays / 365)} año(s)";
        }
    }
}