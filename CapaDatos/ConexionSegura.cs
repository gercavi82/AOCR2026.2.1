using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace CapaDatos
{
    public class ConexionSegura
    {
        private static readonly byte[] _key = Encoding.UTF8.GetBytes("TuClaveSecreta32BytesParaAES!!");
        private static readonly byte[] _iv = Encoding.UTF8.GetBytes("TuIV16BytesAES!!");

        /// <summary>
        /// Encripta la cadena de conexión
        /// </summary>
        public static string EncriptarConnectionString(string connectionString)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(connectionString);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// Desencripta la cadena de conexión
        /// </summary>
        public static string DesencriptarConnectionString(string encryptedConnectionString)
        {
            byte[] buffer = Convert.FromBase64String(encryptedConnectionString);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new System.IO.MemoryStream(buffer))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene conexión con cadena encriptada
        /// </summary>
        public static NpgsqlConnection ObtenerConexionSegura()
        {
            string encryptedString = ConfigurationManager.ConnectionStrings["AocrDatabaseEncrypted"].ConnectionString;
            string decryptedString = DesencriptarConnectionString(encryptedString);

            NpgsqlConnection conexion = new NpgsqlConnection(decryptedString);
            conexion.Open();
            return conexion;
        }
    }
}