using System;
using System.Collections.Generic;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class MenuDAO
    {
        #region CRUD Básico

        public static bool Insertar(Menu menu)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"INSERT INTO menu 
                    (nombremenu, icono, url, orden, activo)
                    VALUES (@p1, @p2, @p3, @p4, @p5)
                    RETURNING idmenu";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@p1", menu.NombreMenu ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p2", menu.Icono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p3", menu.Url ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p4", (object)menu.Orden ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p5", menu.Activo ?? true);

                    var result = cmd.ExecuteScalar();
                    menu.IdMenu = Convert.ToInt32(result);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar menú: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static bool Actualizar(Menu menu)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"UPDATE menu SET 
                    nombremenu = @p1, icono = @p2, url = @p3, 
                    orden = @p4, activo = @p5
                    WHERE idmenu = @p6";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@p1", menu.NombreMenu ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p2", menu.Icono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p3", menu.Url ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p4", (object)menu.Orden ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p5", menu.Activo ?? true);
                    cmd.Parameters.AddWithValue("@p6", menu.IdMenu);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar menú: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static bool Eliminar(int idMenu)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = "UPDATE menu SET activo = false WHERE idmenu = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idMenu);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar menú: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static Menu ObtenerPorId(int idMenu)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = "SELECT * FROM menu WHERE idmenu = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idMenu);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapearMenu(reader);
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener menú: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static List<Menu> ObtenerTodos()
        {
            NpgsqlConnection conexion = null;
            List<Menu> lista = new List<Menu>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = "SELECT * FROM menu WHERE activo = true ORDER BY orden";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearMenu(reader));
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener menús: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        #endregion

        #region Métodos Específicos

        public static List<Menu> ObtenerMenusPorRol(int codigoRol)
        {
            NpgsqlConnection conexion = null;
            List<Menu> lista = new List<Menu>();

            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = @"SELECT DISTINCT m.* 
                    FROM menu m
                    INNER JOIN submenu sm ON m.idmenu = sm.idmenu
                    INNER JOIN permiso p ON sm.idsubmenu = p.idsubmenu
                    WHERE p.codigorol = @rol AND m.activo = true AND p.activo = true
                    ORDER BY m.orden";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@rol", codigoRol);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearMenu(reader));
                        }
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener menús por rol: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        public static bool CambiarOrden(int idMenu, int nuevoOrden)
        {
            NpgsqlConnection conexion = null;
            try
            {
                conexion = ConexionDAO.ObtenerConexion();
                string query = "UPDATE menu SET orden = @orden WHERE idmenu = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@orden", nuevoOrden);
                    cmd.Parameters.AddWithValue("@id", idMenu);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar orden del menú: " + ex.Message, ex);
            }
            finally
            {
                ConexionDAO.CerrarConexion(conexion);
            }
        }

        #endregion

        #region Mapeo

        private static Menu MapearMenu(NpgsqlDataReader reader)
        {
            return new Menu
            {
                IdMenu = Convert.ToInt32(reader["idmenu"]),
                NombreMenu = reader["nombremenu"]?.ToString(),
                Icono = reader["icono"]?.ToString(),
                Url = reader["url"]?.ToString(),
                Orden = reader["orden"] != DBNull.Value ? Convert.ToInt32(reader["orden"]) : (int?)null,
                Activo = reader["activo"] != DBNull.Value && Convert.ToBoolean(reader["activo"])
            };
        }

        #endregion
    }
}
