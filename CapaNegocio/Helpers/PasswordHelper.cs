using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CapaNegocio.Helpers
{
    /// <summary>
    /// Helper para gestión segura de contraseñas
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Genera un hash de la contraseña usando SHA256
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash de la contraseña</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="hash">Hash almacenado</param>
        /// <returns>True si coinciden</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            string hashOfInput = HashPassword(password);

            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }

        /// <summary>
        /// Genera una contraseña aleatoria segura
        /// </summary>
        /// <param name="longitud">Longitud de la contraseña (mínimo 8)</param>
        /// <returns>Contraseña generada</returns>
        public static string GenerarPasswordAleatoria(int longitud = 12)
        {
            if (longitud < 8)
                longitud = 8;

            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string especiales = "!@#$%^&*";
            const string todos = mayusculas + minusculas + numeros + especiales;

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            // Asegurar al menos un carácter de cada tipo
            password.Append(mayusculas[random.Next(mayusculas.Length)]);
            password.Append(minusculas[random.Next(minusculas.Length)]);
            password.Append(numeros[random.Next(numeros.Length)]);
            password.Append(especiales[random.Next(especiales.Length)]);

            // Completar con caracteres aleatorios
            for (int i = 4; i < longitud; i++)
            {
                password.Append(todos[random.Next(todos.Length)]);
            }

            // Mezclar los caracteres
            return new string(password.ToString().OrderBy(x => random.Next()).ToArray());
        }

        /// <summary>
        /// Valida la fortaleza de una contraseña
        /// </summary>
        /// <param name="password">Contraseña a validar</param>
        /// <returns>Tupla con resultado y mensaje</returns>
        public static (bool esValida, string mensaje) ValidarFortaleza(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "La contraseña no puede estar vacía");

            if (password.Length < 6)
                return (false, "La contraseña debe tener al menos 6 caracteres");

            if (password.Length > 50)
                return (false, "La contraseña no puede exceder 50 caracteres");

            if (!password.Any(char.IsUpper))
                return (false, "La contraseña debe contener al menos una letra mayúscula");

            if (!password.Any(char.IsLower))
                return (false, "La contraseña debe contener al menos una letra minúscula");

            if (!password.Any(char.IsDigit))
                return (false, "La contraseña debe contener al menos un número");

            // Validaciones adicionales opcionales
            if (password.Length >= 8)
            {
                if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
                    return (true, "Contraseña fuerte (recomendado agregar caracteres especiales)");
            }

            return (true, "Contraseña válida");
        }

        /// <summary>
        /// Calcula el nivel de fortaleza de una contraseña (0-100)
        /// </summary>
        /// <param name="password">Contraseña a evaluar</param>
        /// <returns>Puntuación de 0 a 100</returns>
        public static int CalcularNivelFortaleza(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            int puntuacion = 0;

            // Longitud
            if (password.Length >= 8) puntuacion += 20;
            else if (password.Length >= 6) puntuacion += 10;

            if (password.Length >= 12) puntuacion += 10;
            if (password.Length >= 16) puntuacion += 10;

            // Complejidad
            if (password.Any(char.IsUpper)) puntuacion += 15;
            if (password.Any(char.IsLower)) puntuacion += 15;
            if (password.Any(char.IsDigit)) puntuacion += 15;
            if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) puntuacion += 15;

            return Math.Min(puntuacion, 100);
        }

        /// <summary>
        /// Genera un token aleatorio para recuperación de contraseña
        /// </summary>
        /// <param name="longitud">Longitud del token</param>
        /// <returns>Token generado</returns>
        public static string GenerarTokenRecuperacion(int longitud = 32)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder token = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < longitud; i++)
            {
                token.Append(caracteres[random.Next(caracteres.Length)]);
            }

            return token.ToString();
        }
    }
}