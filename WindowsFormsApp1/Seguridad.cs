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
            return BCrypt.Net.BCrypt.Verify(textoPlano, hashAlmacenado);
        }
    }
}
