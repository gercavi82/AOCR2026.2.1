using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CapaNegocio.Helpers
{
    /// <summary>
    /// Helper para validaciones comunes
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Valida si un email es válido
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <returns>True si es válido</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Patrón RFC 5322 simplificado
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida si un NIT colombiano es válido
        /// </summary>
        /// <param name="nit">NIT a validar</param>
        /// <returns>True si es válido</returns>
        public static bool IsValidNIT(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit))
                return false;

            // Remover caracteres no numéricos excepto el guión
            nit = nit.Replace(".", "").Replace(",", "").Replace(" ", "");

            // Formato: XXXXXXXXX-X
            string pattern = @"^\d{9}-\d{1}$";

            if (!Regex.IsMatch(nit, pattern))
            {
                // Intentar validar sin guión
                if (nit.Length >= 9 && nit.Length <= 10 && nit.All(char.IsDigit))
                    return true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida si una cédula colombiana es válida
        /// </summary>
        /// <param name="cedula">Cédula a validar</param>
        /// <returns>True si es válida</returns>
        public static bool IsValidCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return false;

            // Remover caracteres no numéricos
            cedula = new string(cedula.Where(char.IsDigit).ToArray());

            // Debe tener entre 6 y 10 dígitos
            if (cedula.Length < 6 || cedula.Length > 10)
                return false;

            // Todos deben ser números
            return cedula.All(char.IsDigit);
        }

        /// <summary>
        /// Valida si un teléfono colombiano es válido
        /// </summary>
        /// <param name="telefono">Teléfono a validar</param>
        /// <returns>True si es válido</returns>
        public static bool IsValidTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return false;

            // Remover caracteres no numéricos
            string numeroLimpio = new string(telefono.Where(char.IsDigit).ToArray());

            // Teléfono fijo: 7 dígitos (con indicativo 10)
            // Celular: 10 dígitos
            if (numeroLimpio.Length == 7 || numeroLimpio.Length == 10)
                return true;

            return false;
        }

        /// <summary>
        /// Valida si una URL es válida
        /// </summary>
        /// <param name="url">URL a validar</param>
        /// <returns>True si es válida</returns>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Valida si una fecha está en un rango válido
        /// </summary>
        /// <param name="fecha">Fecha a validar</param>
        /// <param name="fechaMinima">Fecha mínima permitida</param>
        /// <param name="fechaMaxima">Fecha máxima permitida</param>
        /// <returns>True si está en el rango</returns>
        public static bool IsValidDateRange(DateTime fecha, DateTime? fechaMinima = null, DateTime? fechaMaxima = null)
        {
            if (fechaMinima.HasValue && fecha < fechaMinima.Value)
                return false;

            if (fechaMaxima.HasValue && fecha > fechaMaxima.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Valida si un número está en un rango
        /// </summary>
        /// <param name="numero">Número a validar</param>
        /// <param name="minimo">Valor mínimo</param>
        /// <param name="maximo">Valor máximo</param>
        /// <returns>True si está en el rango</returns>
        public static bool IsInRange(decimal numero, decimal minimo, decimal maximo)
        {
            return numero >= minimo && numero <= maximo;
        }

        /// <summary>
        /// Valida si un string contiene solo letras
        /// </summary>
        /// <param name="texto">Texto a validar</param>
        /// <returns>True si solo contiene letras</returns>
        public static bool IsOnlyLetters(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return texto.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Valida si un string contiene solo números
        /// </summary>
        /// <param name="texto">Texto a validar</param>
        /// <returns>True si solo contiene números</returns>
        public static bool IsOnlyNumbers(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return texto.All(char.IsDigit);
        }

        /// <summary>
        /// Valida si un string es alfanumérico
        /// </summary>
        /// <param name="texto">Texto a validar</param>
        /// <returns>True si es alfanumérico</returns>
        public static bool IsAlphanumeric(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return texto.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Limpia un string de caracteres especiales
        /// </summary>
        /// <param name="texto">Texto a limpiar</param>
        /// <returns>Texto limpio</returns>
        public static string LimpiarCaracteresEspeciales(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            return Regex.Replace(texto, @"[^a-zA-Z0-9\s]", "");
        }

        /// <summary>
        /// Valida si un archivo tiene una extensión permitida
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo</param>
        /// <param name="extensionesPermitidas">Array de extensiones permitidas</param>
        /// <returns>True si la extensión es válida</returns>
        public static bool IsValidFileExtension(string nombreArchivo, string[] extensionesPermitidas)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return false;

            string extension = System.IO.Path.GetExtension(nombreArchivo).ToLower();

            return extensionesPermitidas.Any(ext => ext.ToLower() == extension);
        }

        /// <summary>
        /// Valida si el tamaño de un archivo es válido
        /// </summary>
        /// <param name="tamanioBytes">Tamaño en bytes</param>
        /// <param name="tamanioMaximoMB">Tamaño máximo en MB</param>
        /// <returns>True si el tamaño es válido</returns>
        public static bool IsValidFileSize(long tamanioBytes, int tamanioMaximoMB)
        {
            long tamanioMaximoBytes = tamanioMaximoMB * 1024 * 1024;
            return tamanioBytes > 0 && tamanioBytes <= tamanioMaximoBytes;
        }

        /// <summary>
        /// Formatea un NIT con el formato estándar
        /// </summary>
        /// <param name="nit">NIT sin formato</param>
        /// <returns>NIT formateado</returns>
        public static string FormatearNIT(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit))
                return string.Empty;

            // Remover caracteres no numéricos
            nit = new string(nit.Where(char.IsDigit).ToArray());

            if (nit.Length < 9)
                return nit;

            // Formato: XXX.XXX.XXX-X
            if (nit.Length == 10)
            {
                return $"{nit.Substring(0, 3)}.{nit.Substring(3, 3)}.{nit.Substring(6, 3)}-{nit.Substring(9, 1)}";
            }
            else if (nit.Length == 9)
            {
                return $"{nit.Substring(0, 3)}.{nit.Substring(3, 3)}.{nit.Substring(6, 3)}";
            }

            return nit;
        }

        /// <summary>
        /// Formatea un teléfono con el formato estándar
        /// </summary>
        /// <param name="telefono">Teléfono sin formato</param>
        /// <returns>Teléfono formateado</returns>
        public static string FormatearTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            // Remover caracteres no numéricos
            string numeroLimpio = new string(telefono.Where(char.IsDigit).ToArray());

            if (numeroLimpio.Length == 10)
            {
                // Formato celular: (XXX) XXX-XXXX
                return $"({numeroLimpio.Substring(0, 3)}) {numeroLimpio.Substring(3, 3)}-{numeroLimpio.Substring(6, 4)}";
            }
            else if (numeroLimpio.Length == 7)
            {
                // Formato fijo: XXX-XXXX
                return $"{numeroLimpio.Substring(0, 3)}-{numeroLimpio.Substring(3, 4)}";
            }

            return telefono;
        }

        /// <summary>
        /// Sanitiza un string para prevenir inyección SQL
        /// </summary>
        /// <param name="input">String a sanitizar</param>
        /// <returns>String sanitizado</returns>
        public static string SanitizarSQL(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remover caracteres peligrosos
            input = input.Replace("'", "''");
            input = input.Replace("--", "");
            input = input.Replace(";", "");
            input = input.Replace("/*", "");
            input = input.Replace("*/", "");
            input = input.Replace("xp_", "");
            input = input.Replace("sp_", "");

            return input;
        }
    }
}