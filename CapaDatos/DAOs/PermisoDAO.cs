// ==========================================================
// PermisoDAO.cs (COMPATIBLE CON TU MODELO ACTUAL)
// PostgreSQL - Npgsql - .NET Framework 4.7.2
//
// ✅ Soluciona:
//   - CS0117 si tu modelo Permiso no tiene Leer/Crear/Editar/Eliminar/Modulo
//   - CS0103 por variables mal declaradas en patrones bool?
//   - CS0266 int? -> int en Map()
//
// ✅ Estrategia:
//   Usa Reflection para leer/escribir flags y Modulo solo si existen.
//
// 📌 Tabla esperada: aocr_tbpermiso
// Columnas esperadas:
//  idpermiso, codigorol, idmenu, idsubmenu,
//  leer, crear, editar, eliminar, modulo
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
    public class PermisoDAO
    {
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

        // ======================================================
        // Helpers Reflection (evitan CS0117)
        // ======================================================
        private static bool GetBoolProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return false;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return false;

                var val = prop.GetValue(obj, null);
                if (val == null) return false;

                if (val is bool b) return b;

                // ✅ Forma segura para bool?
                if (val is bool?)
                    return ((bool?)val).GetValueOrDefault();

                // Soporte banderas numéricas
                if (val is int i) return i != 0;
                if (val is short s) return s != 0;
                if (val is long l) return l != 0;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void SetBoolProp(object obj, string propName, bool value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                if (prop.PropertyType == typeof(bool))
                    prop.SetValue(obj, value, null);
                else if (prop.PropertyType == typeof(bool?))
                    prop.SetValue(obj, (bool?)value, null);
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(obj, value ? 1 : 0, null);
                else if (prop.PropertyType == typeof(short))
                    prop.SetValue(obj, (short)(value ? 1 : 0), null);
            }
            catch
            {
                // silencioso
            }
        }

        private static string GetStringProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
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

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
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
        // Mapeo
        // ==============================
        private static Permiso Map(IDataRecord r)
        {
            // ✅ IMPORTANTE:
            // No usamos int? en asignaciones base para evitar CS0266
            var permiso = new Permiso
            {
                IdPermiso = r["idpermiso"] != DBNull.Value ? Convert.ToInt32(r["idpermiso"]) : 0,

                // ✅ si el modelo es int -> ok
                // ✅ si el modelo es int? -> también ok
                CodigoRol = r["codigorol"] != DBNull.Value ? Convert.ToInt32(r["codigorol"]) : 0,
                IdMenu = r["idmenu"] != DBNull.Value ? Convert.ToInt32(r["idmenu"]) : 0,
                IdSubmenu = r["idsubmenu"] != DBNull.Value ? Convert.ToInt32(r["idsubmenu"]) : 0
            };

            // Flags opcionales (solo si existen en el modelo)
            if (r["leer"] != DBNull.Value) SetBoolProp(permiso, "Leer", Convert.ToBoolean(r["leer"]));
            if (r["crear"] != DBNull.Value) SetBoolProp(permiso, "Crear", Convert.ToBoolean(r["crear"]));
            if (r["editar"] != DBNull.Value) SetBoolProp(permiso, "Editar", Convert.ToBoolean(r["editar"]));
            if (r["eliminar"] != DBNull.Value) SetBoolProp(permiso, "Eliminar", Convert.ToBoolean(r["eliminar"]));

            // Modulo opcional
            if (r["modulo"] != DBNull.Value) SetStringProp(permiso, "Modulo", r["modulo"]?.ToString());

            return permiso;
        }

        // ==============================
        // Insertar
        // ==============================
        public static bool Insertar(Permiso permiso)
        {
            const string sql = @"
                INSERT INTO aocr_tbpermiso
                (codigorol, idmenu, idsubmenu, leer, crear, editar, eliminar, modulo)
                VALUES
                (@rol, @menu, @sub, @leer, @crear, @editar, @eliminar, @modulo);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@rol", permiso.CodigoRol);
                cmd.Parameters.AddWithValue("@menu", permiso.IdMenu);
                cmd.Parameters.AddWithValue("@sub", permiso.IdSubmenu);

                cmd.Parameters.AddWithValue("@leer", GetBoolProp(permiso, "Leer"));
                cmd.Parameters.AddWithValue("@crear", GetBoolProp(permiso, "Crear"));
                cmd.Parameters.AddWithValue("@editar", GetBoolProp(permiso, "Editar"));
                cmd.Parameters.AddWithValue("@eliminar", GetBoolProp(permiso, "Eliminar"));

                string modulo = GetStringProp(permiso, "Modulo");
                cmd.Parameters.AddWithValue("@modulo", (object)modulo ?? DBNull.Value);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public static bool Actualizar(Permiso permiso)
        {
            const string sql = @"
                UPDATE aocr_tbpermiso
                SET
                    codigorol = @rol,
                    idmenu = @menu,
                    idsubmenu = @sub,
                    leer = @leer,
                    crear = @crear,
                    editar = @editar,
                    eliminar = @eliminar,
                    modulo = @modulo
                WHERE idpermiso = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@rol", permiso.CodigoRol);
                cmd.Parameters.AddWithValue("@menu", permiso.IdMenu);
                cmd.Parameters.AddWithValue("@sub", permiso.IdSubmenu);

                cmd.Parameters.AddWithValue("@leer", GetBoolProp(permiso, "Leer"));
                cmd.Parameters.AddWithValue("@crear", GetBoolProp(permiso, "Crear"));
                cmd.Parameters.AddWithValue("@editar", GetBoolProp(permiso, "Editar"));
                cmd.Parameters.AddWithValue("@eliminar", GetBoolProp(permiso, "Eliminar"));

                string modulo = GetStringProp(permiso, "Modulo");
                cmd.Parameters.AddWithValue("@modulo", (object)modulo ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@id", permiso.IdPermiso);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar
        // ==============================
        public static bool Eliminar(int idPermiso)
        {
            const string sql = @"DELETE FROM aocr_tbpermiso WHERE idpermiso = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", idPermiso);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Obtener por Id
        // ==============================
        public static Permiso ObtenerPorId(int idPermiso)
        {
            const string sql = @"
                SELECT idpermiso, codigorol, idmenu, idsubmenu, leer, crear, editar, eliminar, modulo
                FROM aocr_tbpermiso
                WHERE idpermiso = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", idPermiso);

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
        // Obtener todos
        // ==============================
        public static List<Permiso> ObtenerTodos()
        {
            var list = new List<Permiso>();

            const string sql = @"
                SELECT idpermiso, codigorol, idmenu, idsubmenu, leer, crear, editar, eliminar, modulo
                FROM aocr_tbpermiso
                ORDER BY idpermiso DESC;";

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
        // Obtener por Rol
        // ==============================
        public static List<Permiso> ObtenerPorRol(int codigoRol)
        {
            var list = new List<Permiso>();

            const string sql = @"
                SELECT idpermiso, codigorol, idmenu, idsubmenu, leer, crear, editar, eliminar, modulo
                FROM aocr_tbpermiso
                WHERE codigorol = @rol
                ORDER BY idpermiso DESC;";

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
        // Obtener por Menú
        // ==============================
        public static List<Permiso> ObtenerPorMenu(int idMenu)
        {
            var list = new List<Permiso>();

            const string sql = @"
                SELECT idpermiso, codigorol, idmenu, idsubmenu, leer, crear, editar, eliminar, modulo
                FROM aocr_tbpermiso
                WHERE idmenu = @menu
                ORDER BY idpermiso DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@menu", idMenu);

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
        // Verificar Permiso por acción
        // ==============================
        public static bool VerificarPermiso(int codigoRol, int? idMenu, int? idSubmenu, string accion)
        {
            string col;
            switch ((accion ?? "").ToUpper())
            {
                case "LEER":
                case "VER":
                    col = "leer";
                    break;

                case "CREAR":
                case "INSERTAR":
                    col = "crear";
                    break;

                case "EDITAR":
                case "ACTUALIZAR":
                    col = "editar";
                    break;

                case "ELIMINAR":
                case "BORRAR":
                    col = "eliminar";
                    break;

                default:
                    return false;
            }

            string filtro = "";
            if (idSubmenu.HasValue)
                filtro = " AND idsubmenu = @sub ";
            else if (idMenu.HasValue)
                filtro = " AND idmenu = @menu ";

            string sql = $@"
                SELECT COUNT(*)
                FROM aocr_tbpermiso
                WHERE codigorol = @rol
                {filtro}
                AND COALESCE({col}, false) = true;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@rol", codigoRol);

                if (idMenu.HasValue) cmd.Parameters.AddWithValue("@menu", idMenu.Value);
                if (idSubmenu.HasValue) cmd.Parameters.AddWithValue("@sub", idSubmenu.Value);

                cn.Open();
                var n = Convert.ToInt32(cmd.ExecuteScalar());
                return n > 0;
            }
        }
    }
}
