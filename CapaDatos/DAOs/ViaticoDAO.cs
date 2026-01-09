using System;
using System.Data;
using System.Collections.Generic;
using Npgsql;
using CapaModelo;
using CapaDatos.DAOs;

namespace CapaDatos
{
    public class ViaticoDAO
    {
        #region CRUD Básico

        public static bool Insertar(Viatico viatico)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"INSERT INTO aocr_tbviatico 
                    (codigo_inspeccion, codigo_tecnico, concepto, monto, 
                     fecha, tipo, comprobante, observaciones)
                    VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)
                    RETURNING codigo_viatico";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@p1", (object)viatico.CodigoInspeccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p2", (object)viatico.CodigoTecnico ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p3", viatico.Concepto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p4", (object)viatico.Monto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p5", viatico.Fecha ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@p6", viatico.Tipo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p7", viatico.Comprobante ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p8", viatico.Observaciones ?? (object)DBNull.Value);

                    var result = cmd.ExecuteScalar();
                    viatico.CodigoViatico = Convert.ToInt32(result);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar viático: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static bool Actualizar(Viatico viatico)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"UPDATE aocr_tbviatico SET 
                    concepto = @p1, monto = @p2, fecha = @p3, tipo = @p4,
                    comprobante = @p5, observaciones = @p6
                    WHERE codigo_viatico = @p7";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@p1", viatico.Concepto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p2", (object)viatico.Monto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p3", viatico.Fecha ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@p4", viatico.Tipo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p5", viatico.Comprobante ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p6", viatico.Observaciones ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p7", viatico.CodigoViatico);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar viático: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static bool Eliminar(int codigoViatico)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = "DELETE FROM aocr_tbviatico WHERE codigo_viatico = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", codigoViatico);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar viático: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static Viatico ObtenerPorId(int codigoViatico)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT v.*, t.nombretecnico, t.apellidotecnico
                    FROM aocr_tbviatico v
                    LEFT JOIN tecnico t ON v.codigo_tecnico = t.codigotecnico
                    WHERE v.codigo_viatico = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", codigoViatico);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapearViatico(reader);
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener viático: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        // ============================================================
        // MÉTODO FALTANTE: OBTENER TODOS ✅
        // ============================================================
        public static List<Viatico> ObtenerTodos()
        {
            NpgsqlConnection conexion = null;
            List<Viatico> lista = new List<Viatico>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT v.*, t.nombretecnico, t.apellidotecnico
                    FROM aocr_tbviatico v
                    LEFT JOIN tecnico t ON v.codigo_tecnico = t.codigotecnico
                    ORDER BY v.fecha DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearViatico(reader));
                        }
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todos los viáticos: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        // ============================================================
        // MÉTODO FALTANTE: OBTENER POR ESTADO ✅
        // ============================================================
        public static List<Viatico> ObtenerPorEstado(string estado)
        {
            NpgsqlConnection conexion = null;
            List<Viatico> lista = new List<Viatico>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();

                // Si tu tabla tiene un campo 'estado', usa esta query:
                string query = @"SELECT v.*, t.nombretecnico, t.apellidotecnico
                    FROM aocr_tbviatico v
                    LEFT JOIN tecnico t ON v.codigo_tecnico = t.codigotecnico
                    WHERE v.estado = @estado
                    ORDER BY v.fecha DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@estado", estado);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearViatico(reader));
                        }
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener viáticos por estado: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        #endregion

        #region Consultas

        public static List<Viatico> ObtenerPorInspeccion(int codigoInspeccion)
        {
            NpgsqlConnection conexion = null;
            List<Viatico> lista = new List<Viatico>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT v.*, t.nombretecnico, t.apellidotecnico
                    FROM aocr_tbviatico v
                    LEFT JOIN tecnico t ON v.codigo_tecnico = t.codigotecnico
                    WHERE v.codigo_inspeccion = @id 
                    ORDER BY v.fecha DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", codigoInspeccion);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearViatico(reader));
                        }
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener viáticos por inspección: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static List<Viatico> ObtenerPorTecnico(int codigoTecnico)
        {
            NpgsqlConnection conexion = null;
            List<Viatico> lista = new List<Viatico>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT v.*, t.nombretecnico, t.apellidotecnico
                    FROM aocr_tbviatico v
                    LEFT JOIN tecnico t ON v.codigo_tecnico = t.codigotecnico
                    WHERE v.codigo_tecnico = @id 
                    ORDER BY v.fecha DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", codigoTecnico);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearViatico(reader));
                        }
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener viáticos por técnico: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static List<Viatico> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            NpgsqlConnection conexion = null;
            List<Viatico> lista = new List<Viatico>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT v.*, t.nombretecnico, t.apellidotecnico
                    FROM aocr_tbviatico v
                    LEFT JOIN tecnico t ON v.codigo_tecnico = t.codigotecnico
                    WHERE v.fecha BETWEEN @inicio AND @fin 
                    ORDER BY v.fecha DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@inicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fin", fechaFin);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearViatico(reader));
                        }
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener viáticos por fecha: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static decimal ObtenerTotalPorInspeccion(int codigoInspeccion)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT COALESCE(SUM(monto), 0) 
                    FROM aocr_tbviatico 
                    WHERE codigo_inspeccion = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", codigoInspeccion);
                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener total de viáticos: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static Dictionary<string, decimal> ObtenerTotalesPorTipo(int codigoInspeccion)
        {
            NpgsqlConnection conexion = null;
            Dictionary<string, decimal> totales = new Dictionary<string, decimal>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT tipo, SUM(monto) as total
                    FROM aocr_tbviatico 
                    WHERE codigo_inspeccion = @id
                    GROUP BY tipo
                    ORDER BY tipo";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", codigoInspeccion);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tipo = reader["tipo"]?.ToString() ?? "Sin Tipo";
                            decimal total = Convert.ToDecimal(reader["total"]);
                            totales[tipo] = total;
                        }
                    }
                }
                return totales;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener totales por tipo: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        #endregion

        #region Mapeo

        private static Viatico MapearViatico(NpgsqlDataReader reader)
        {
            return new Viatico
            {
                CodigoViatico = Convert.ToInt32(reader["codigo_viatico"]),
                CodigoInspeccion = reader["codigo_inspeccion"] != DBNull.Value ? Convert.ToInt32(reader["codigo_inspeccion"]) : (int?)null,
                CodigoTecnico = reader["codigo_tecnico"] != DBNull.Value ? Convert.ToInt32(reader["codigo_tecnico"]) : (int?)null,
                Concepto = reader["concepto"]?.ToString(),
                Monto = reader["monto"] != DBNull.Value ? Convert.ToDecimal(reader["monto"]) : (decimal?)null,
                Fecha = reader["fecha"] != DBNull.Value ? Convert.ToDateTime(reader["fecha"]) : (DateTime?)null,
                Tipo = reader["tipo"]?.ToString(),
                Comprobante = reader["comprobante"]?.ToString(),
                Observaciones = reader["observaciones"]?.ToString(),
                NombreTecnico = reader.GetOrdinal("nombretecnico") >= 0 && reader["nombretecnico"] != DBNull.Value
                    ? reader["nombretecnico"].ToString() + " " + reader["apellidotecnico"]?.ToString()
                    : null
            };
        }

        #endregion
    }
}