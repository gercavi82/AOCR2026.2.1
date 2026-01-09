using System;
using System.Collections.Generic;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class LogDAO
    {
        #region Insertar

        public static bool Insertar(Log log)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();

                string query = @"INSERT INTO aocr_tblog 
                    (nivel, categoria, mensaje, detalle, codigo_usuario, 
                     ip_address, url, fecha_registro)
                    VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@p1", log.Nivel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p2", log.Categoria ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p3", log.Mensaje ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p4", log.Detalle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p5", (object)log.CodigoUsuario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p6", log.IpAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p7", log.Url ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p8", DateTime.Now);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                // No lanzar excepción para evitar bucles infinitos en logging
                System.Diagnostics.Debug.WriteLine($"Error al insertar log: {ex.Message}");
                return false;
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        #endregion

        #region Consultas

        public static List<Log> ObtenerPorNivel(string nivel, int limite = 100)
        {
            NpgsqlConnection conexion = null;
            List<Log> lista = new List<Log>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();

                string query = @"SELECT * FROM aocr_tblog 
                    WHERE nivel = @nivel 
                    ORDER BY fecha_registro DESC 
                    LIMIT @limite";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nivel", nivel);
                    cmd.Parameters.AddWithValue("@limite", limite);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearLog(reader));
                        }
                    }
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener logs: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static List<Log> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            NpgsqlConnection conexion = null;
            List<Log> lista = new List<Log>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();

                string query = @"SELECT * FROM aocr_tblog 
                    WHERE fecha_registro BETWEEN @inicio AND @fin 
                    ORDER BY fecha_registro DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@inicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fin", fechaFin);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearLog(reader));
                        }
                    }
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener logs por fecha: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static bool LimpiarLogsAntiguos(int diasAntiguedad)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();

                // ✅ Forma correcta en PostgreSQL usando intervalo multiplicado
                string query = @"DELETE FROM aocr_tblog 
                    WHERE fecha_registro < (CURRENT_DATE - (@dias * INTERVAL '1 day'))";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@dias", diasAntiguedad);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al limpiar logs: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        #endregion

        #region Métodos Auxiliares

        public static void RegistrarInfo(string mensaje, string categoria = "General", int? codigoUsuario = null)
        {
            Insertar(new Log
            {
                Nivel = "INFO",
                Categoria = categoria,
                Mensaje = mensaje,
                CodigoUsuario = codigoUsuario,
                FechaRegistro = DateTime.Now
            });
        }

        public static void RegistrarError(string mensaje, string detalle, string categoria = "General", int? codigoUsuario = null)
        {
            Insertar(new Log
            {
                Nivel = "ERROR",
                Categoria = categoria,
                Mensaje = mensaje,
                Detalle = detalle,
                CodigoUsuario = codigoUsuario,
                FechaRegistro = DateTime.Now
            });
        }

        public static void RegistrarWarning(string mensaje, string categoria = "General", int? codigoUsuario = null)
        {
            Insertar(new Log
            {
                Nivel = "WARNING",
                Categoria = categoria,
                Mensaje = mensaje,
                CodigoUsuario = codigoUsuario,
                FechaRegistro = DateTime.Now
            });
        }

        #endregion

        #region Mapeo

        private static Log MapearLog(NpgsqlDataReader reader)
        {
            return new Log
            {
                CodigoLog = Convert.ToInt32(reader["codigo_log"]),
                Nivel = reader["nivel"]?.ToString(),
                Categoria = reader["categoria"]?.ToString(),
                Mensaje = reader["mensaje"]?.ToString(),
                Detalle = reader["detalle"]?.ToString(),
                CodigoUsuario = reader["codigo_usuario"] != DBNull.Value
                    ? Convert.ToInt32(reader["codigo_usuario"])
                    : (int?)null,
                IpAddress = reader["ip_address"]?.ToString(),
                Url = reader["url"]?.ToString(),
                FechaRegistro = reader["fecha_registro"] != DBNull.Value
                    ? Convert.ToDateTime(reader["fecha_registro"])
                    : (DateTime?)null
            };
        }

        #endregion
    }
}
