using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public static class RolDAO
    {
        // Usamos tu clase de conexión centralizada
        private static NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        /// <summary>
        /// Obtiene todos los roles. Mapea 'codigorol' de la BD a 'IdRol' del modelo.
        /// </summary>
        public static List<Rol> ObtenerTodos()
        {
            using (var cn = CrearConexion())
            {
                cn.Open();

                // ⚠ IMPORTANTE: Usamos 'AS' para que coincida con tu clase Rol.cs
                string sql = @"
                    SELECT 
                        codigorol       AS IdRol,
                        descripcion     AS Descripcion,
                        descripcion     AS Nombre,  -- Asumimos que Nombre y Descripción son lo mismo por ahora
                        activo          AS Activo,
                        'System'        AS CreadoPor,
                        CURRENT_DATE    AS FechaCreacion
                    FROM rol 
                    ORDER BY codigorol";

                return cn.Query<Rol>(sql).ToList();
            }
        }

        public static Rol ObtenerPorId(int id)
        {
            using (var cn = CrearConexion())
            {
                cn.Open();
                string sql = @"
                    SELECT 
                        codigorol   AS IdRol,
                        descripcion AS Descripcion,
                        activo      AS Activo
                    FROM rol 
                    WHERE codigorol = @id";

                return cn.QueryFirstOrDefault<Rol>(sql, new { id });
            }
        }

        public static bool Insertar(Rol rol, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (var cn = CrearConexion())
                {
                    cn.Open();
                    // Insertamos solo los campos que existen en la tabla física
                    string sql = @"INSERT INTO rol (descripcion, activo) 
                                   VALUES (@Descripcion, @Activo)";

                    int filas = cn.Execute(sql, rol);

                    if (filas > 0)
                    {
                        mensaje = "Rol registrado correctamente.";
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public static bool Actualizar(Rol rol, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (var cn = CrearConexion())
                {
                    cn.Open();
                    // Usamos @IdRol porque así se llama la propiedad en tu clase
                    string sql = @"UPDATE rol 
                                   SET descripcion = @Descripcion, 
                                       activo = @Activo 
                                   WHERE codigorol = @IdRol";

                    int filas = cn.Execute(sql, rol);

                    if (filas > 0)
                    {
                        mensaje = "Rol actualizado correctamente.";
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public static bool Eliminar(int id, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (var cn = CrearConexion())
                {
                    cn.Open();
                    // Validación de integridad referencial
                    int uso = cn.ExecuteScalar<int>("SELECT COUNT(*) FROM usuario_rol WHERE codigorol = @id", new { id });

                    if (uso > 0)
                    {
                        mensaje = "No se puede eliminar: El rol está asignado a usuarios.";
                        return false;
                    }

                    string sql = "DELETE FROM rol WHERE codigorol = @id";
                    int filas = cn.Execute(sql, new { id });

                    if (filas > 0)
                    {
                        mensaje = "Rol eliminado.";
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }
        // ============================================================
        // CAMBIAR ESTADO (CORREGIDO)
        // ============================================================
        public static bool CambiarEstado(int id, bool activo, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                // SOLUCIÓN: Usamos CrearConexion() en vez de new NpgsqlConnection(connectionString)
                using (var cn = CrearConexion())
                {
                    cn.Open();

                    // AJUSTE SQL: Usamos 'activo' y 'codigorol' para coincidir con tu base de datos
                    string sql = "UPDATE rol SET activo = @activo WHERE codigorol = @id";

                    // Usamos Dapper para ejecutar, igual que en tus otros métodos
                    int filas = cn.Execute(sql, new { activo = activo, id = id });

                    if (filas > 0)
                    {
                        return true;
                    }

                    mensaje = "No se encontró el rol con ese ID.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al actualizar estado: " + ex.Message;
                return false;
            }
        }
    }
    
}