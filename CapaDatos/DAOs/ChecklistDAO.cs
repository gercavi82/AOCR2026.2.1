using CapaModelo;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace CapaDatos.DAOs
{
    public static class ChecklistDAO
    {
        // Usamos una propiedad para no repetir la cadena de conexión
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["AOCRConnection"].ConnectionString;

        public static void Insertar(ChecklistItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                string sql = @"INSERT INTO checklist (codigosolicitud, descripcion, cumple) 
                               VALUES (@CodigoSolicitud, @Descripcion, @Cumple);";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CodigoSolicitud", item.CodigoSolicitud);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)item.Descripcion ?? DBNull.Value);

                    // CORRECCIÓN: Manejo seguro de Nullable bool
                    cmd.Parameters.AddWithValue("@Cumple", item.Cumple.HasValue ? (object)item.Cumple.Value : DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<ChecklistItem> ObtenerPorSolicitud(int codigoSolicitud)
        {
            var lista = new List<ChecklistItem>();

            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                string sql = @"SELECT codigosolicitud, descripcion, cumple FROM checklist WHERE codigosolicitud = @CodigoSolicitud;";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CodigoSolicitud", codigoSolicitud);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ChecklistItem
                            {
                                CodigoSolicitud = dr.GetInt32(0),
                                Descripcion = dr.IsDBNull(1) ? null : dr.GetString(1),
                                // CORRECCIÓN: Cast explícito a bool?
                                Cumple = dr.IsDBNull(2) ? (bool?)null : dr.GetBoolean(2)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public static Dictionary<string, int> ObtenerEstadisticasPorSolicitud(int codigoSolicitud)
        {
            var resultado = new Dictionary<string, int>
            {
                { "Cumplen", 0 }, { "NoCumplen", 0 }, { "SinEvaluar", 0 }, { "Total", 0 }
            };

            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                string sql = @"SELECT cumple FROM checklist WHERE codigosolicitud = @CodigoSolicitud;";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CodigoSolicitud", codigoSolicitud);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0))
                                resultado["SinEvaluar"]++;
                            else if (dr.GetBoolean(0))
                                resultado["Cumplen"]++;
                            else
                                resultado["NoCumplen"]++;

                            resultado["Total"]++;
                        }
                    }
                }
            }
            return resultado;
        }
    }
}