// ==========================================================
// SesionDAO.cs (COMPATIBLE CON TU MODELO ACTUAL)
// PostgreSQL - Npgsql - .NET Framework 4.7.2
//
// ✅ SOLUCIONA:
//   - CS0117: 'Sesion' no contiene una definición para 'FechaFin'
//   - CS0266: int? -> int (especialmente CodigoUsuario en Map)
//
// ✅ Estrategia:
//   No se asignan int? a propiedades base.
//   Propiedades opcionales se manejan con Reflection:
//     - FechaFin (opcional)
//     - IpAddress (opcional)
//     - Activa (opcional)
//     - Token (opcional)
//
// 📌 Tabla sugerida: aocr_tbsesion
// Columnas sugeridas:
//  codigosesion   SERIAL/INT PK
//  codigousuario  INT FK
//  fechainicio    TIMESTAMP NULL
//  fechafin       TIMESTAMP NULL
//  ipaddress      VARCHAR NULL
//  activa         BOOLEAN NULL
//  token          VARCHAR NULL
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
    public class SesionDAO
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
        private static DateTime? GetDateProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is DateTime dt) return dt;
                if (val is DateTime?) return (DateTime?)val;

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static void SetDateProp(object obj, string propName, DateTime? value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(obj, value ?? DateTime.MinValue, null);
                else if (prop.PropertyType == typeof(DateTime?))
                    prop.SetValue(obj, value, null);
            }
            catch
            {
                // silencioso
            }
        }

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
                if (val is bool?)
                    return ((bool?)val).GetValueOrDefault();

                if (val is int i) return i != 0;
                if (val is short s) return s != 0;

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
        // Mapeo (corrige int? -> int)
        // ==============================
        private static Sesion Map(IDataRecord r)
        {
            var sesion = new Sesion
            {
                CodigoSesion = r["codigosesion"] != DBNull.Value ? Convert.ToInt32(r["codigosesion"]) : 0,

                // ✅ CLAVE: Nunca asignar int? aquí
                // Si el modelo es int -> ok
                // Si el modelo es int? -> también ok
                CodigoUsuario = r["codigousuario"] != DBNull.Value ? Convert.ToInt32(r["codigousuario"]) : 0,

                FechaInicio = r["fechainicio"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["fechainicio"]) : null
            };

            // Opcionales por Reflection
            if (r["fechafin"] != DBNull.Value)
                SetDateProp(sesion, "FechaFin", (DateTime?)Convert.ToDateTime(r["fechafin"]));

            if (r["ipaddress"] != DBNull.Value)
                SetStringProp(sesion, "IpAddress", r["ipaddress"]?.ToString());

            if (r["activa"] != DBNull.Value)
                SetBoolProp(sesion, "Activa", Convert.ToBoolean(r["activa"]));

            if (r["token"] != DBNull.Value)
                SetStringProp(sesion, "Token", r["token"]?.ToString());

            return sesion;
        }

        // ==============================
        // Insertar
        // ==============================
        public static bool Insertar(Sesion sesion)
        {
            const string sql = @"
                INSERT INTO aocr_tbsesion
                (codigousuario, fechainicio, fechafin, ipaddress, activa, token)
                VALUES
                (@codigousuario, @fechainicio, @fechafin, @ipaddress, @activa, @token);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                // tolerante si el modelo fuera int?
                cmd.Parameters.AddWithValue("@codigousuario", (object)sesion.CodigoUsuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fechainicio", (object)sesion.FechaInicio ?? DBNull.Value);

                var fechaFin = GetDateProp(sesion, "FechaFin");
                cmd.Parameters.AddWithValue("@fechafin", (object)fechaFin ?? DBNull.Value);

                var ip = GetStringProp(sesion, "IpAddress");
                cmd.Parameters.AddWithValue("@ipaddress", (object)ip ?? DBNull.Value);

                bool activa = GetBoolProp(sesion, "Activa");
                cmd.Parameters.AddWithValue("@activa", activa);

                var token = GetStringProp(sesion, "Token");
                cmd.Parameters.AddWithValue("@token", (object)token ?? DBNull.Value);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public static bool Actualizar(Sesion sesion)
        {
            const string sql = @"
                UPDATE aocr_tbsesion
                SET codigousuario = @codigousuario,
                    fechainicio = @fechainicio,
                    fechafin = @fechafin,
                    ipaddress = @ipaddress,
                    activa = @activa,
                    token = @token
                WHERE codigosesion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@codigousuario", (object)sesion.CodigoUsuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fechainicio", (object)sesion.FechaInicio ?? DBNull.Value);

                var fechaFin = GetDateProp(sesion, "FechaFin");
                cmd.Parameters.AddWithValue("@fechafin", (object)fechaFin ?? DBNull.Value);

                var ip = GetStringProp(sesion, "IpAddress");
                cmd.Parameters.AddWithValue("@ipaddress", (object)ip ?? DBNull.Value);

                bool activa = GetBoolProp(sesion, "Activa");
                cmd.Parameters.AddWithValue("@activa", activa);

                var token = GetStringProp(sesion, "Token");
                cmd.Parameters.AddWithValue("@token", (object)token ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@id", sesion.CodigoSesion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar
        // ==============================
        public static bool Eliminar(int codigoSesion)
        {
            const string sql = @"DELETE FROM aocr_tbsesion WHERE codigosesion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSesion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // ObtenerPorId
        // ==============================
        public static Sesion ObtenerPorId(int codigoSesion)
        {
            const string sql = @"
                SELECT codigosesion, codigousuario, fechainicio, fechafin, ipaddress, activa, token
                FROM aocr_tbsesion
                WHERE codigosesion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSesion);

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
        // ObtenerPorToken
        // ==============================
        public static Sesion ObtenerPorToken(string token)
        {
            const string sql = @"
                SELECT codigosesion, codigousuario, fechainicio, fechafin, ipaddress, activa, token
                FROM aocr_tbsesion
                WHERE token = @token
                ORDER BY codigosesion DESC
                LIMIT 1;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@token", token ?? string.Empty);

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
        // ObtenerPorUsuario
        // ==============================
        public static List<Sesion> ObtenerPorUsuario(int codigoUsuario)
        {
            var list = new List<Sesion>();

            const string sql = @"
                SELECT codigosesion, codigousuario, fechainicio, fechafin, ipaddress, activa, token
                FROM aocr_tbsesion
                WHERE codigousuario = @codigousuario
                ORDER BY fechainicio DESC NULLS LAST;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@codigousuario", codigoUsuario);

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
        // ObtenerSesionesActivas (por usuario)
        // ==============================
        public static List<Sesion> ObtenerSesionesActivas(int codigoUsuario)
        {
            var list = new List<Sesion>();

            const string sql = @"
                SELECT codigosesion, codigousuario, fechainicio, fechafin, ipaddress, activa, token
                FROM aocr_tbsesion
                WHERE codigousuario = @codigousuario
                  AND COALESCE(activa, false) = true
                ORDER BY fechainicio DESC NULLS LAST;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@codigousuario", codigoUsuario);

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
        // ObtenerTodasSesionesActivas
        // ==============================
        public static List<Sesion> ObtenerTodasSesionesActivas()
        {
            var list = new List<Sesion>();

            const string sql = @"
                SELECT codigosesion, codigousuario, fechainicio, fechafin, ipaddress, activa, token
                FROM aocr_tbsesion
                WHERE COALESCE(activa, false) = true
                ORDER BY fechainicio DESC NULLS LAST;";

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
        // ObtenerPorFecha
        // ==============================
        public static List<Sesion> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            var list = new List<Sesion>();

            const string sql = @"
                SELECT codigosesion, codigousuario, fechainicio, fechafin, ipaddress, activa, token
                FROM aocr_tbsesion
                WHERE fechainicio IS NOT NULL
                  AND fechainicio >= @inicio
                  AND fechainicio <= @fin
                ORDER BY fechainicio ASC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@inicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fin", fechaFin);

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
