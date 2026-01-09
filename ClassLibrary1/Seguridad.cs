using System;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace CapaUtilidades
{
    public static class Seguridad
    {
        public static string EncriptarConBCrypt(string textoPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(textoPlano);
        }

        public static bool VerificarBCrypt(string textoPlano, string hashAlmacenado)
        {
            try
            {
                // Validación defensiva: evitar hashes que no son bcrypt
                if (string.IsNullOrWhiteSpace(hashAlmacenado))
                    return false;

                // bcrypt válido siempre empieza por "$2"
                if (!hashAlmacenado.StartsWith("$2"))
                    return false;

                return BCrypt.Net.BCrypt.Verify(textoPlano, hashAlmacenado);
            }
            catch
            {
                // Si aún así algo falla, se considera inválido
                return false;
            }
        }

        // Opcional: útil para migración de usuarios antiguos
        public static string EncriptarSHA256(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano)) return string.Empty;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
    }
}
