using System;
using System.Collections.Generic;
using System.Configuration;
using CapaModelo;
using Npgsql;

namespace CapaDatos.DAOs
{
    public static class AeronaveDAO
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["AOCRConnection"].ConnectionString;
        }

        public static void Insertar(Aeronave a)
        {
            using (NpgsqlConnection cn = new NpgsqlConnection(GetConnectionString()))
            {
                cn.Open();

                // Hemos sincronizado los nombres de las columnas de la DB con las propiedades del objeto
                string sql = @"
                    INSERT INTO aocr_tbaeronave
                    (
                        codigo_solicitud, fabricante, modelo, serie, matricula,
                        configuracion, etapa_ruido, peso_max, anio_fabricacion, 
                        capacidad_pasajeros, capacidad_carga, tipo_motor, 
                        numero_motores, activo, fecha_registro
                    )
                    VALUES
                    (
                        @sol, @fab, @mod, @ser, @mat,
                        @config, @etapa, @peso, @anio,
                        @pasajeros, @carga, @motor,
                        @motores, @activo, @fecha
                    );";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    // ✅ CORRECCIÓN: Ahora usamos .Fabricante y .Serie (Nombres nuevos en el Modelo)
                    cmd.Parameters.AddWithValue("@sol", a.CodigoSolicitud);
                    cmd.Parameters.AddWithValue("@fab", (object)a.Fabricante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mod", (object)a.Modelo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ser", (object)a.Serie ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mat", (object)a.Matricula ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@config", (object)a.Configuracion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@etapa", (object)a.EtapaRuido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@peso", (object)a.PesoMax ?? DBNull.Value); // Propiedad nueva
                    cmd.Parameters.AddWithValue("@anio", (object)a.AnioFabricacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pasajeros", (object)a.CapacidadPasajeros ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@carga", (object)a.CapacidadCarga ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@motor", (object)a.TipoMotor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@motores", (object)a.NumeroMotores ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@activo", a.Activo);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool ExisteMatricula(string matricula)
        {
            using (NpgsqlConnection cn = new NpgsqlConnection(GetConnectionString()))
            {
                cn.Open();
                string sql = "SELECT COUNT(1) FROM aocr_tbaeronave WHERE matricula = @m;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@m", matricula);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public static List<Aeronave> ObtenerPorSolicitud(int codigoSolicitud)
        {
            List<Aeronave> lista = new List<Aeronave>();

            using (NpgsqlConnection cn = new NpgsqlConnection(GetConnectionString()))
            {
                cn.Open();
                string sql = "SELECT * FROM aocr_tbaeronave WHERE codigo_solicitud = @sol ORDER BY codigo_aeronave ASC;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@sol", codigoSolicitud);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Aeronave a = new Aeronave
                            {
                                CodigoAeronave = Convert.ToInt32(reader["codigo_aeronave"]),
                                CodigoSolicitud = Convert.ToInt32(reader["codigo_solicitud"]),
                                // ✅ CORRECCIÓN: Mapeo sincronizado con las nuevas propiedades
                                Fabricante = reader["fabricante"].ToString(),
                                Modelo = reader["modelo"].ToString(),
                                Serie = reader["serie"].ToString(),
                                Matricula = reader["matricula"].ToString(),
                                Configuracion = reader["configuracion"].ToString(),
                                EtapaRuido = reader["etapa_ruido"].ToString(),
                                PesoMax = reader["peso_max"] != DBNull.Value ? Convert.ToDecimal(reader["peso_max"]) : (decimal?)null
                            };
                            lista.Add(a);
                        }
                    }
                }
            }
            return lista;
        }
    }
}