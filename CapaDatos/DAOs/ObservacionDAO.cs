// ==========================================================
// ObservacionDAO.cs (CORREGIDO - SIN ERRORES CS1061/CS0117)
// PostgreSQL - Npgsql - .NET Framework 4.7.2
//
// ✅ SOLUCIONA:
//   - CS1061 / CS0117 sobre Observacion.CodigoUsuario
//   - EVITA depender en compile-time de propiedades opcionales
//     (Estado, Observaciones, FechaObservacion, FechaResolucion,
//      CodigoUsuario) usando REFLECTION.
//
// 📌 Tabla esperada: aocr_tbobservacion
// Columnas sugeridas:
//   codigoobservacion (PK, int)
//   codigoinspeccion  (int, FK)
//   descripcion       (text/varchar)
//   gravedad          (varchar)
//   estado            (varchar, opcional)
//   observaciones     (text, opcional)
//   fechaobservacion  (timestamp, opcional)
//   fecharesolucion   (timestamp, opcional)
//   codigousuario     (int, opcional)
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
    public class ObservacionDAO
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
        // Helpers Reflection (evitan dependencia directa del modelo)
        // ======================================================
        private static string GetStringProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                return val != null ? val.ToString() : null;
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

        private static DateTime? GetDateProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is DateTime)
                    return (DateTime)val;

                if (val is DateTime?)
                    return (DateTime?)val;

                try
                {
                    return Convert.ToDateTime(val);
                }
                catch
                {
                    return null;
                }
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

        private static int? GetIntNullableProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is int)
                    return (int)val;

                if (val is int?)
                    return (int?)val;

                try
                {
                    return Convert.ToInt32(val);
                }
                catch
                {
                    return null;
                }
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

        // ==============================
        // Mapeo
        // ==============================
        private static Observacion Map(IDataRecord r)
        {
            var obs = new Observacion
            {
                CodigoObservacion = r["codigoobservacion"] != DBNull.Value
                    ? Convert.ToInt32(r["codigoobservacion"])
                    : 0,

                CodigoInspeccion = r["codigoinspeccion"] != DBNull.Value
                    ? (int?)Convert.ToInt32(r["codigoinspeccion"])
                    : null,

                Descripcion = r["descripcion"] != DBNull.Value
                    ? r["descripcion"]?.ToString()
                    : null,

                Gravedad = r["gravedad"] != DBNull.Value
                    ? r["gravedad"]?.ToString()
                    : null
            };

            // Estado (si el modelo lo tiene)
            if (r["estado"] != DBNull.Value)
                SetStringProp(obs, "Estado", r["estado"]?.ToString());

            // Observaciones (si el modelo lo tiene)
            if (r["observaciones"] != DBNull.Value)
                SetStringProp(obs, "Observaciones", r["observaciones"]?.ToString());

            // Fechas (si el modelo las tiene)
            if (r["fechaobservacion"] != DBNull.Value)
                SetDateProp(obs, "FechaObservacion", Convert.ToDateTime(r["fechaobservacion"]));

            if (r["fecharesolucion"] != DBNull.Value)
                SetDateProp(obs, "FechaResolucion", Convert.ToDateTime(r["fecharesolucion"]));

            // CodigoUsuario (si el modelo lo tiene)
            if (r["codigousuario"] != DBNull.Value)
            {
                int? codUser = Convert.ToInt32(r["codigousuario"]);
                SetIntProp(obs, "CodigoUsuario", codUser);
            }

            return obs;
        }

        // ==============================
        // Insertar
        // ==============================
        public static bool Insertar(Observacion observacion)
        {
            const string sql = @"
                INSERT INTO aocr_tbobservacion
                (codigoinspeccion, descripcion, gravedad, estado, observaciones,
                 fechaobservacion, fecharesolucion, codigousuario)
                VALUES
                (@ins, @desc, @grav, @estado, @obs,
                 @fobs, @fres, @user);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@ins",
                    (object)observacion.CodigoInspeccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@desc",
                    (object)observacion.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@grav",
                    (object)observacion.Gravedad ?? DBNull.Value);

                // Estado / Observaciones opcionales vía reflection
                string estado = GetStringProp(observacion, "Estado") ?? "Pendiente";
                string notas = GetStringProp(observacion, "Observaciones");

                cmd.Parameters.AddWithValue("@estado",
                    (object)estado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@obs",
                    (object)notas ?? DBNull.Value);

                // Fechas opcionales vía reflection
                DateTime? fObs = GetDateProp(observacion, "FechaObservacion") ?? DateTime.Now;
                DateTime? fRes = GetDateProp(observacion, "FechaResolucion");

                cmd.Parameters.AddWithValue("@fobs",
                    (object)fObs ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@fres",
                    (object)fRes ?? DBNull.Value);

                // CodigoUsuario opcional vía reflection
                int? codUser = GetIntNullableProp(observacion, "CodigoUsuario");
                cmd.Parameters.AddWithValue("@user",
                    (object)codUser ?? DBNull.Value);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public static bool Actualizar(Observacion observacion)
        {
            const string sql = @"
                UPDATE aocr_tbobservacion
                SET
                    codigoinspeccion = @ins,
                    descripcion = @desc,
                    gravedad = @grav,
                    estado = @estado,
                    observaciones = @obs,
                    fechaobservacion = @fobs,
                    fecharesolucion = @fres,
                    codigousuario = @user
                WHERE codigoobservacion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@ins",
                    (object)observacion.CodigoInspeccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@desc",
                    (object)observacion.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@grav",
                    (object)observacion.Gravedad ?? DBNull.Value);

                string estado = GetStringProp(observacion, "Estado") ?? "Pendiente";
                string notas = GetStringProp(observacion, "Observaciones");

                cmd.Parameters.AddWithValue("@estado",
                    (object)estado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@obs",
                    (object)notas ?? DBNull.Value);

                DateTime? fObs = GetDateProp(observacion, "FechaObservacion") ?? DateTime.Now;
                DateTime? fRes = GetDateProp(observacion, "FechaResolucion");

                cmd.Parameters.AddWithValue("@fobs",
                    (object)fObs ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@fres",
                    (object)fRes ?? DBNull.Value);

                int? codUser = GetIntNullableProp(observacion, "CodigoUsuario");
                cmd.Parameters.AddWithValue("@user",
                    (object)codUser ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@id", observacion.CodigoObservacion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar
        // ==============================
        public static bool Eliminar(int codigoObservacion)
        {
            const string sql = @"
                DELETE FROM aocr_tbobservacion
                WHERE codigoobservacion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoObservacion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Obtener por Id
        // ==============================
        public static Observacion ObtenerPorId(int codigoObservacion)
        {
            const string sql = @"
                SELECT
                    codigoobservacion, codigoinspeccion, descripcion,
                    gravedad, estado, observaciones,
                    fechaobservacion, fecharesolucion, codigousuario
                FROM aocr_tbobservacion
                WHERE codigoobservacion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoObservacion);

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
        // Obtener por Inspección
        // ==============================
        public static List<Observacion> ObtenerPorInspeccion(int codigoInspeccion)
        {
            var list = new List<Observacion>();

            const string sql = @"
                SELECT
                    codigoobservacion, codigoinspeccion, descripcion,
                    gravedad, estado, observaciones,
                    fechaobservacion, fecharesolucion, codigousuario
                FROM aocr_tbobservacion
                WHERE codigoinspeccion = @ins
                ORDER BY codigoobservacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@ins", codigoInspeccion);

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
        // Obtener por Gravedad
        // ==============================
        public static List<Observacion> ObtenerPorGravedad(string gravedad)
        {
            var list = new List<Observacion>();

            const string sql = @"
                SELECT
                    codigoobservacion, codigoinspeccion, descripcion,
                    gravedad, estado, observaciones,
                    fechaobservacion, fecharesolucion, codigousuario
                FROM aocr_tbobservacion
                WHERE UPPER(gravedad) = UPPER(@grav)
                ORDER BY codigoobservacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@grav", (object)gravedad ?? DBNull.Value);

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
        // Obtener por Estado
        // ==============================
        public static List<Observacion> ObtenerPorEstado(string estado)
        {
            var list = new List<Observacion>();

            const string sql = @"
                SELECT
                    codigoobservacion, codigoinspeccion, descripcion,
                    gravedad, estado, observaciones,
                    fechaobservacion, fecharesolucion, codigousuario
                FROM aocr_tbobservacion
                WHERE UPPER(estado) = UPPER(@estado)
                ORDER BY codigoobservacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@estado", (object)estado ?? DBNull.Value);

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
        // Actualizar Estado
        // ==============================
        public static bool ActualizarEstado(int codigoObservacion, string nuevoEstado)
        {
            const string sql = @"
                UPDATE aocr_tbobservacion
                SET estado = @estado
                WHERE codigoobservacion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@estado", (object)nuevoEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", codigoObservacion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
