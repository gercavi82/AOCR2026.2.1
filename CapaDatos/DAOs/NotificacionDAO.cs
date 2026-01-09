using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Notificación AOCR – PostgreSQL (Npgsql)
    /// Tabla sugerida: aocr_tbnotificacion
    /// Columnas esperadas:
    ///   codigonotificacion (PK, serial/int)
    ///   codigousuario      (int)
    ///   titulo             (varchar)
    ///   mensaje            (text)
    ///   tipo               (varchar)
    ///   url                (varchar)
    ///   fechacreacion      (timestamp)
    ///   leida              (boolean)
    /// </summary>
    public class NotificacionDAO
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

        // ==============================
        // Mapeo
        // ==============================
        private static Notificacion Map(IDataRecord r)
        {
            var not = new Notificacion();

            // Asumiendo propiedades del modelo:
            // int CodigoNotificacion, int CodigoUsuario, string Titulo, string Mensaje,
            // string Tipo, string Url, DateTime? FechaCreacion, bool Leida

            if (r["codigonotificacion"] != DBNull.Value)
                not.CodigoNotificacion = Convert.ToInt32(r["codigonotificacion"]);

            if (r["codigousuario"] != DBNull.Value)
                not.CodigoUsuario = Convert.ToInt32(r["codigousuario"]);

            not.Titulo = r["titulo"] != DBNull.Value ? r["titulo"].ToString() : null;
            not.Mensaje = r["mensaje"] != DBNull.Value ? r["mensaje"].ToString() : null;
            not.Tipo = r["tipo"] != DBNull.Value ? r["tipo"].ToString() : null;
            not.Url = r["url"] != DBNull.Value ? r["url"].ToString() : null;

            if (r["fechacreacion"] != DBNull.Value)
                not.FechaCreacion = Convert.ToDateTime(r["fechacreacion"]);

            if (r["leida"] != DBNull.Value)
                not.Leida = Convert.ToBoolean(r["leida"]);

            return not;
        }

        // ==============================
        // Insertar
        // ==============================
        public static bool Insertar(Notificacion notificacion)
        {
            const string sql = @"
                INSERT INTO aocr_tbnotificacion
                (codigousuario, titulo, mensaje, tipo, url, fechacreacion, leida)
                VALUES
                (@user, @tit, @msg, @tipo, @url, @fec, @leida)
                RETURNING codigonotificacion;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", notificacion.CodigoUsuario);
                cmd.Parameters.AddWithValue("@tit", (object)notificacion.Titulo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@msg", (object)notificacion.Mensaje ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipo", (object)notificacion.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@url", (object)notificacion.Url ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fec", (object)notificacion.FechaCreacion ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@leida", notificacion.Leida);

                cn.Open();
                var obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    notificacion.CodigoNotificacion = Convert.ToInt32(obj);

                return notificacion.CodigoNotificacion > 0;
            }
        }

        // ==============================
        // Marcar como leída
        // ==============================
        public static bool MarcarComoLeida(int codigoNotificacion)
        {
            const string sql = @"
                UPDATE aocr_tbnotificacion
                SET leida = TRUE
                WHERE codigonotificacion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoNotificacion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool MarcarTodasComoLeidas(int codigoUsuario)
        {
            const string sql = @"
                UPDATE aocr_tbnotificacion
                SET leida = TRUE
                WHERE codigousuario = @user
                  AND leida = FALSE;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", codigoUsuario);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar
        // ==============================
        public static bool Eliminar(int codigoNotificacion)
        {
            const string sql = @"
                DELETE FROM aocr_tbnotificacion
                WHERE codigonotificacion = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoNotificacion);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Consultas
        // ==============================
        public static List<Notificacion> ObtenerPorUsuario(int codigoUsuario)
        {
            var list = new List<Notificacion>();

            const string sql = @"
                SELECT
                    codigonotificacion, codigousuario, titulo, mensaje,
                    tipo, url, fechacreacion, leida
                FROM aocr_tbnotificacion
                WHERE codigousuario = @user
                ORDER BY fechacreacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", codigoUsuario);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(Map(rd));
                }
            }

            return list;
        }

        public static List<Notificacion> ObtenerNoLeidas(int codigoUsuario)
        {
            var list = new List<Notificacion>();

            const string sql = @"
                SELECT
                    codigonotificacion, codigousuario, titulo, mensaje,
                    tipo, url, fechacreacion, leida
                FROM aocr_tbnotificacion
                WHERE codigousuario = @user
                  AND leida = FALSE
                ORDER BY fechacreacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", codigoUsuario);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(Map(rd));
                }
            }

            return list;
        }

        public static int ContarNoLeidas(int codigoUsuario)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM aocr_tbnotificacion
                WHERE codigousuario = @user
                  AND leida = FALSE;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", codigoUsuario);

                cn.Open();
                var obj = cmd.ExecuteScalar();
                return (obj != null && obj != DBNull.Value) ? Convert.ToInt32(obj) : 0;
            }
        }

        // ==============================
        // 🔹 FALTABA: ObtenerPorTipo
        // ==============================
        public static List<Notificacion> ObtenerPorTipo(int codigoUsuario, string tipo)
        {
            var list = new List<Notificacion>();

            const string sql = @"
                SELECT
                    codigonotificacion, codigousuario, titulo, mensaje,
                    tipo, url, fechacreacion, leida
                FROM aocr_tbnotificacion
                WHERE codigousuario = @user
                  AND UPPER(tipo) = UPPER(@tipo)
                ORDER BY fechacreacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", codigoUsuario);
                cmd.Parameters.AddWithValue("@tipo", (object)tipo ?? DBNull.Value);

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
        // 🔹 FALTABA: ObtenerRecientes
        // ==============================
        public static List<Notificacion> ObtenerRecientes(int codigoUsuario, int cantidad)
        {
            var list = new List<Notificacion>();

            const string sql = @"
                SELECT
                    codigonotificacion, codigousuario, titulo, mensaje,
                    tipo, url, fechacreacion, leida
                FROM aocr_tbnotificacion
                WHERE codigousuario = @user
                ORDER BY fechacreacion DESC
                LIMIT @cant;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", codigoUsuario);
                cmd.Parameters.AddWithValue("@cant", cantidad);

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
        // Mantenimiento: limpiar antiguas
        // ==============================
        public static bool LimpiarNotificacionesAntiguas(int diasAntiguedad)
        {
            DateTime fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

            const string sql = @"
                DELETE FROM aocr_tbnotificacion
                WHERE fechacreacion < @fechaLimite;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@fechaLimite", fechaLimite);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
