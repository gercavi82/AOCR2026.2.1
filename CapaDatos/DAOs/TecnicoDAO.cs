// ==========================================================
// TecnicoDAO.cs (COMPLETO Y COMPATIBLE CON TU MODELO ACTUAL)
// PostgreSQL - Npgsql - .NET Framework 4.7.2
//
// ✅ Métodos usados por TecnicoBL:
//   - Insertar
//   - Actualizar
//   - Eliminar
//   - ObtenerPorId
//   - ObtenerTodos
//   - ExistePorUsuario
//   - ObtenerPorUsuario
//   - ObtenerActivos
//   - ObtenerPorEspecialidad
//   - ObtenerDisponibles
//
// ✅ SOLUCIONA:
//   - CS0117: Tecnico no contiene CodigoUsuario/Certificaciones/AniosExperiencia/Activo/Disponible
//   - CS1061: accesos directos a props inexistentes
//   - CS0266: bool? -> bool
//
// ✅ Estrategia:
//   - Solo usa propiedades "seguras" directamente:
//       CodigoTecnico, Especialidad
//   - Todo lo demás por Reflection (si existe lo usa, si no existe lo ignora)
//
// 📌 Tabla sugerida: aocr_tbtecnico
// Columnas mínimas esperadas:
//  codigotecnico   SERIAL/INT PK
//  codigousuario   INT FK
//  especialidad    VARCHAR
//  certificaciones TEXT/VARCHAR NULL
//  aniosexperiencia INT NULL
//  activo          BOOLEAN NULL
//  disponible      BOOLEAN NULL
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
    public class TecnicoDAO
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
        // Helpers Reflection (tolerantes al modelo)
        // ======================================================
        private static int? GetIntProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is int i) return i;
                if (val is int?) return (int?)val;

                return Convert.ToInt32(val);
            }
            catch
            {
                return null;
            }
        }

        private static void SetIntProp(object obj, string propName, int? value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                if (prop.PropertyType == typeof(int))
                    prop.SetValue(obj, value ?? 0, null);
                else if (prop.PropertyType == typeof(int?))
                    prop.SetValue(obj, value, null);
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

        private static bool? GetBoolPropNullable(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is bool b) return b;
                if (val is bool?) return (bool?)val;

                if (val is int i) return i != 0;
                if (val is short s) return s != 0;
                if (val is long l) return l != 0;

                return null;
            }
            catch
            {
                return null;
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

        // ==============================
        // Mapeo (SIN usar props opcionales directas)
        // ==============================
        private static Tecnico Map(IDataRecord r)
        {
            var tecnico = new Tecnico
            {
                // ✅ Asume que estas SÍ existen en tu modelo actual
                CodigoTecnico = r["codigotecnico"] != DBNull.Value ? Convert.ToInt32(r["codigotecnico"]) : 0,
                Especialidad = r["especialidad"] != DBNull.Value ? r["especialidad"].ToString() : null
            };

            // ✅ Opcionales por Reflection
            if (r["codigousuario"] != DBNull.Value)
                SetIntProp(tecnico, "CodigoUsuario", Convert.ToInt32(r["codigousuario"]));

            if (r["certificaciones"] != DBNull.Value)
                SetStringProp(tecnico, "Certificaciones", r["certificaciones"]?.ToString());

            if (r["aniosexperiencia"] != DBNull.Value)
                SetIntProp(tecnico, "AniosExperiencia", Convert.ToInt32(r["aniosexperiencia"]));

            if (r["activo"] != DBNull.Value)
                SetBoolProp(tecnico, "Activo", Convert.ToBoolean(r["activo"]));

            if (r["disponible"] != DBNull.Value)
                SetBoolProp(tecnico, "Disponible", Convert.ToBoolean(r["disponible"]));

            return tecnico;
        }

        // ==============================
        // ExistePorUsuario
        // ==============================
        public static bool ExistePorUsuario(int codigoUsuario)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM aocr_tbtecnico
                WHERE codigousuario = @usuario;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@usuario", codigoUsuario);

                cn.Open();
                var n = Convert.ToInt32(cmd.ExecuteScalar());
                return n > 0;
            }
        }

        // ==============================
        // Insertar
        // ==============================
        public static bool Insertar(Tecnico tecnico)
        {
            const string sql = @"
                INSERT INTO aocr_tbtecnico
                (codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible)
                VALUES
                (@usuario, @especialidad, @certificaciones, @anios, @activo, @disponible);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                // ✅ CodigoUsuario opcional en el modelo
                var codUsuario = GetIntProp(tecnico, "CodigoUsuario");
                cmd.Parameters.AddWithValue("@usuario", (object)codUsuario ?? DBNull.Value);

                // ✅ Especialidad (se asume existente)
                cmd.Parameters.AddWithValue("@especialidad", tecnico.Especialidad ?? string.Empty);

                var certs = GetStringProp(tecnico, "Certificaciones");
                cmd.Parameters.AddWithValue("@certificaciones", (object)certs ?? DBNull.Value);

                var anios = GetIntProp(tecnico, "AniosExperiencia");
                cmd.Parameters.AddWithValue("@anios", (object)anios ?? DBNull.Value);

                var activoVal = GetBoolPropNullable(tecnico, "Activo");
                cmd.Parameters.AddWithValue("@activo", activoVal.HasValue ? (object)activoVal.Value : (object)true);

                var dispVal = GetBoolPropNullable(tecnico, "Disponible");
                cmd.Parameters.AddWithValue("@disponible", dispVal.HasValue ? (object)dispVal.Value : (object)true);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public static bool Actualizar(Tecnico tecnico)
        {
            const string sql = @"
                UPDATE aocr_tbtecnico
                SET codigousuario = @usuario,
                    especialidad = @especialidad,
                    certificaciones = @certificaciones,
                    aniosexperiencia = @anios,
                    activo = @activo,
                    disponible = @disponible
                WHERE codigotecnico = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                var codUsuario = GetIntProp(tecnico, "CodigoUsuario");
                cmd.Parameters.AddWithValue("@usuario", (object)codUsuario ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@especialidad", tecnico.Especialidad ?? string.Empty);

                var certs = GetStringProp(tecnico, "Certificaciones");
                cmd.Parameters.AddWithValue("@certificaciones", (object)certs ?? DBNull.Value);

                var anios = GetIntProp(tecnico, "AniosExperiencia");
                cmd.Parameters.AddWithValue("@anios", (object)anios ?? DBNull.Value);

                var activoVal = GetBoolPropNullable(tecnico, "Activo");
                cmd.Parameters.AddWithValue("@activo", activoVal.HasValue ? (object)activoVal.Value : (object)true);

                var dispVal = GetBoolPropNullable(tecnico, "Disponible");
                cmd.Parameters.AddWithValue("@disponible", dispVal.HasValue ? (object)dispVal.Value : (object)true);

                cmd.Parameters.AddWithValue("@id", tecnico.CodigoTecnico);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar
        // ==============================
        public static bool Eliminar(int codigoTecnico)
        {
            const string sql = @"DELETE FROM aocr_tbtecnico WHERE codigotecnico = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoTecnico);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // ObtenerPorId
        // ==============================
        public static Tecnico ObtenerPorId(int codigoTecnico)
        {
            const string sql = @"
                SELECT codigotecnico, codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible
                FROM aocr_tbtecnico
                WHERE codigotecnico = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoTecnico);

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
        public static List<Tecnico> ObtenerTodos()
        {
            var list = new List<Tecnico>();

            const string sql = @"
                SELECT codigotecnico, codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible
                FROM aocr_tbtecnico
                ORDER BY codigotecnico DESC;";

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
        // ObtenerPorUsuario
        // ==============================
        public static Tecnico ObtenerPorUsuario(int codigoUsuario)
        {
            const string sql = @"
                SELECT codigotecnico, codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible
                FROM aocr_tbtecnico
                WHERE codigousuario = @usuario
                ORDER BY codigotecnico DESC
                LIMIT 1;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@usuario", codigoUsuario);

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
        // ObtenerActivos
        // ==============================
        public static List<Tecnico> ObtenerActivos()
        {
            var list = new List<Tecnico>();

            const string sql = @"
                SELECT codigotecnico, codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible
                FROM aocr_tbtecnico
                WHERE COALESCE(activo, false) = true
                ORDER BY especialidad, codigotecnico;";

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
        // ObtenerPorEspecialidad
        // ==============================
        public static List<Tecnico> ObtenerPorEspecialidad(string especialidad)
        {
            var list = new List<Tecnico>();

            const string sql = @"
                SELECT codigotecnico, codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible
                FROM aocr_tbtecnico
                WHERE UPPER(especialidad) = UPPER(@esp)
                ORDER BY COALESCE(disponible, false) DESC, codigotecnico;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@esp", especialidad ?? string.Empty);

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
        // ObtenerDisponibles
        // ==============================
        public static List<Tecnico> ObtenerDisponibles()
        {
            var list = new List<Tecnico>();

            const string sql = @"
                SELECT codigotecnico, codigousuario, especialidad, certificaciones, aniosexperiencia, activo, disponible
                FROM aocr_tbtecnico
                WHERE COALESCE(activo, false) = true
                  AND COALESCE(disponible, false) = true
                ORDER BY especialidad, codigotecnico;";

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
    }
}
