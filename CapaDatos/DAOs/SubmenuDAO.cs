// ==========================================================
// SubmenuDAO.cs (COMPLETO y TOLERANTE AL MODELO)
// PostgreSQL - Npgsql - .NET Framework 4.7.2
//
// Métodos usados por SubmenuBL y MenuBL:
//   - Insertar(Submenu)
//   - Actualizar(Submenu)
//   - Eliminar(int)
//   - ObtenerPorId(int)
//   - ObtenerTodos()
//   - ObtenerPorMenu(int)
//   - ObtenerPorRol(int)
//   - CambiarOrden(int,int)
//
// Tabla sugerida: aocr_tbsubmenu
// Columnas mínimas esperadas:
//  idsubmenu (PK serial/int)
//  idmenu (FK int)
//  nombresubmenu (varchar)
//  descripcion (varchar/text null)   <-- opcional en el modelo
//  url (varchar null)
//  icono (varchar null)
//  orden (int null)
//  activo (bool null)
//
// Si tu tabla usa otros nombres, ajusta los SQL.
// ==========================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Reflection;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class SubmenuDAO
    {
        // ==============================
        // Helpers Reflection (opcionales)
        // ==============================
        private static string GetStringProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(
                    propName,
                    BindingFlags.Public | BindingFlags.Instance
                );

                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                return val?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void SetStringProp(object obj, string propName, string value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(
                    propName,
                    BindingFlags.Public | BindingFlags.Instance
                );

                if (prop == null || !prop.CanWrite) return;
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(obj, value, null);
            }
            catch
            {
                // silencioso
            }
        }

        // ==============================
        // Conexión
        // ==============================
        private static NpgsqlConnection CrearConexion()
        {
            var cs = ConfigurationManager.ConnectionStrings["AOCRConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new Exception("No existe la cadena de conexión 'AOCRConnection' en el config.");

            return new NpgsqlConnection(cs);
        }

        // ==============================
        // Mapeo
        // ==============================
        private static Submenu Map(IDataRecord r)
        {
            var s = new Submenu
            {
                IdSubmenu = r["idsubmenu"] != DBNull.Value ? Convert.ToInt32(r["idsubmenu"]) : 0,
                IdMenu = r["idmenu"] != DBNull.Value ? (int?)Convert.ToInt32(r["idmenu"]) : null,
                NombreSubmenu = r["nombresubmenu"] != DBNull.Value ? r["nombresubmenu"].ToString() : null,
                Url = r["url"] != DBNull.Value ? r["url"].ToString() : null,
                Icono = r["icono"] != DBNull.Value ? r["icono"].ToString() : null,
                Orden = r["orden"] != DBNull.Value ? (int?)Convert.ToInt32(r["orden"]) : null,
                Activo = r["activo"] != DBNull.Value ? (bool?)Convert.ToBoolean(r["activo"]) : null
            };

            // ✅ Solo intenta setear Descripcion si la propiedad existe en el modelo
            if (r["descripcion"] != DBNull.Value)
            {
                SetStringProp(s, "Descripcion", r["descripcion"].ToString());
            }

            return s;
        }

        // ==============================
        // Insertar
        // ==============================
        public static bool Insertar(Submenu submenu)
        {
            const string sql = @"
                INSERT INTO aocr_tbsubmenu
                (idmenu, nombresubmenu, descripcion, url, icono, orden, activo)
                VALUES
                (@idmenu, @nombre, @descripcion, @url, @icono, @orden, @activo);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                // campos obligatorios
                cmd.Parameters.AddWithValue("@idmenu", (object)submenu.IdMenu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", submenu.NombreSubmenu ?? string.Empty);

                // ✅ descripcion opcional vía Reflection
                var descripcion = GetStringProp(submenu, "Descripcion");
                cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@url", (object)submenu.Url ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@icono", (object)submenu.Icono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@orden", (object)submenu.Orden ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@activo", (object)submenu.Activo ?? true);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public static bool Actualizar(Submenu submenu)
        {
            const string sql = @"
                UPDATE aocr_tbsubmenu
                SET idmenu = @idmenu,
                    nombresubmenu = @nombre,
                    descripcion = @descripcion,
                    url = @url,
                    icono = @icono,
                    orden = @orden,
                    activo = @activo
                WHERE idsubmenu = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@idmenu", (object)submenu.IdMenu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", submenu.NombreSubmenu ?? string.Empty);

                // ✅ descripcion opcional vía Reflection
                var descripcion = GetStringProp(submenu, "Descripcion");
                cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@url", (object)submenu.Url ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@icono", (object)submenu.Icono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@orden", (object)submenu.Orden ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@activo", (object)submenu.Activo ?? true);
                cmd.Parameters.AddWithValue("@id", submenu.IdSubmenu);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar
        // ==============================
        public static bool Eliminar(int idSubmenu)
        {
            const string sql = @"DELETE FROM aocr_tbsubmenu WHERE idsubmenu = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", idSubmenu);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // ObtenerPorId
        // ==============================
        public static Submenu ObtenerPorId(int idSubmenu)
        {
            const string sql = @"
                SELECT idsubmenu, idmenu, nombresubmenu, descripcion, url, icono, orden, activo
                FROM aocr_tbsubmenu
                WHERE idsubmenu = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", idSubmenu);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                        return Map(rd);
                }
            }

            return null;
        }

        // ==============================
        // ObtenerTodos
        // ==============================
        public static List<Submenu> ObtenerTodos()
        {
            var list = new List<Submenu>();

            const string sql = @"
                SELECT idsubmenu, idmenu, nombresubmenu, descripcion, url, icono, orden, activo
                FROM aocr_tbsubmenu
                ORDER BY idsubmenu DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(Map(rd));
                }
            }

            return list;
        }

        // ==============================
        // ObtenerPorMenu
        // ==============================
        public static List<Submenu> ObtenerPorMenu(int idMenu)
        {
            var list = new List<Submenu>();

            const string sql = @"
                SELECT idsubmenu, idmenu, nombresubmenu, descripcion, url, icono, orden, activo
                FROM aocr_tbsubmenu
                WHERE idmenu = @idmenu
                ORDER BY orden NULLS LAST, idsubmenu ASC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@idmenu", idMenu);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(Map(rd));
                }
            }

            return list;
        }

        // ==============================
        // ObtenerPorRol
        // ==============================
        public static List<Submenu> ObtenerPorRol(int codigoRol)
        {
            // Se asume relación mediante permisos:
            // aocr_tbpermiso(codigorol, idsubmenu)
            var list = new List<Submenu>();

            const string sql = @"
                SELECT DISTINCT s.idsubmenu, s.idmenu, s.nombresubmenu, s.descripcion, s.url, s.icono, s.orden, s.activo
                FROM aocr_tbsubmenu s
                INNER JOIN aocr_tbpermiso p ON p.idsubmenu = s.idsubmenu
                WHERE p.codigorol = @rol
                ORDER BY s.orden NULLS LAST, s.nombresubmenu;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@rol", codigoRol);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(Map(rd));
                }
            }

            return list;
        }

        // ==============================
        // CambiarOrden
        // ==============================
        public static bool CambiarOrden(int idSubmenu, int nuevoOrden)
        {
            const string sql = @"
                UPDATE aocr_tbsubmenu
                SET orden = @orden
                WHERE idsubmenu = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@orden", nuevoOrden);
                cmd.Parameters.AddWithValue("@id", idSubmenu);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
