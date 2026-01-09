using Npgsql;
using System;
using System.Configuration;
using CapaDatos.DAOs; // Esto permitirá que ArchivoDAO sea visible
namespace CapaDatos.DAOs
{
    public static class ArchivoDAO
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["AOCRConnection"].ConnectionString;

        public static void GuardarRegistro(int codigoSolicitud, string nombre, string ruta)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                // Sincronizado con tu tabla aocr_tbdocumento
                string sql = @"INSERT INTO aocr_tbdocumento (codigosolicitud, nombre_archivo, ruta_guardada, fecha_carga) 
                               VALUES (@sol, @nom, @ruta, @fecha);";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@sol", codigoSolicitud);
                    cmd.Parameters.AddWithValue("@nom", nombre);
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}