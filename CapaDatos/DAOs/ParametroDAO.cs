using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Parámetros para PostgreSQL
    /// Compatible .NET Framework 4.7.2
    /// </summary>
    public class ParametroDAO
    {
        // ==============================
        // Conexión directa desde config
        // ==============================
        private NpgsqlConnection CrearConexion()
        {
            var cs = ConfigurationManager.ConnectionStrings["AOCRConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new Exception("No existe la cadena de conexión 'AOCRConnection' en el config.");

            return new NpgsqlConnection(cs);
        }

        // ==============================
        // Mapeo
        // Ajusta nombres si tu tabla difiere
        // ==============================
        private Parametro MapParametro(IDataRecord r)
        {
            return new Parametro
            {
                CodigoParametro = r["codigoparametro"] != DBNull.Value ? Convert.ToInt32(r["codigoparametro"]) : 0,
                Clave = r["clave"]?.ToString(),
                Valor = r["valor"]?.ToString(),
                Descripcion = r["descripcion"]?.ToString(),
                Activo = r["activo"] != DBNull.Value && Convert.ToBoolean(r["activo"]),

                CreatedAt = r["createdat"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["createdat"]) : null,
                CreatedBy = r["createdby"] != DBNull.Value ? (int?)Convert.ToInt32(r["createdby"]) : null,
                UpdatedAt = r["updatedat"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["updatedat"]) : null,
                UpdatedBy = r["updatedby"] != DBNull.Value ? (int?)Convert.ToInt32(r["updatedby"]) : null,
                DeletedAt = r["deletedat"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["deletedat"]) : null,
                DeletedBy = r["deletedby"] != DBNull.Value ? (int?)Convert.ToInt32(r["deletedby"]) : null
            };
        }

        // ==============================
        // Listar
        // ==============================
        public List<Parametro> ListarTodos()
        {
            var lista = new List<Parametro>();

            const string sql = @"
                SELECT codigoparametro, clave, valor, descripcion, activo,
                       createdat, createdby, updatedat, updatedby, deletedat, deletedby
                FROM aocr_tbparametro
                ORDER BY clave;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapParametro(rd));
                }
            }

            return lista;
        }

        public List<Parametro> ListarActivos()
        {
            var lista = new List<Parametro>();

            const string sql = @"
                SELECT codigoparametro, clave, valor, descripcion, activo,
                       createdat, createdby, updatedat, updatedby, deletedat, deletedby
                FROM aocr_tbparametro
                WHERE activo = TRUE AND deletedat IS NULL
                ORDER BY clave;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapParametro(rd));
                }
            }

            return lista;
        }

        // ==============================
        // Obtener por ID
        // ==============================
        public Parametro ObtenerPorId(int codigoParametro)
        {
            const string sql = @"
                SELECT codigoparametro, clave, valor, descripcion, activo,
                       createdat, createdby, updatedat, updatedby, deletedat, deletedby
                FROM aocr_tbparametro
                WHERE codigoparametro = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoParametro);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                        return MapParametro(rd);
                }
            }

            return null;
        }

        // ==============================
        // Obtener por Clave
        // ==============================
        public Parametro ObtenerPorClave(string clave)
        {
            const string sql = @"
                SELECT codigoparametro, clave, valor, descripcion, activo,
                       createdat, createdby, updatedat, updatedby, deletedat, deletedby
                FROM aocr_tbparametro
                WHERE clave = @clave AND deletedat IS NULL
                ORDER BY codigoparametro DESC
                LIMIT 1;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@clave", clave ?? string.Empty);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                        return MapParametro(rd);
                }
            }

            return null;
        }

        // ==============================
        // Crear
        // ==============================
        public bool Crear(Parametro p, int codigoUsuario)
        {
            const string sql = @"
                INSERT INTO aocr_tbparametro
                (clave, valor, descripcion, activo, createdat, createdby)
                VALUES
                (@clave, @valor, @descripcion, @activo, @createdat, @createdby);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@clave", (object)p.Clave ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@valor", (object)p.Valor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)p.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@activo", p.Activo);
                cmd.Parameters.AddWithValue("@createdat", DateTime.Now);
                cmd.Parameters.AddWithValue("@createdby", codigoUsuario);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public bool Actualizar(Parametro p, int codigoUsuario)
        {
            const string sql = @"
                UPDATE aocr_tbparametro
                SET clave = @clave,
                    valor = @valor,
                    descripcion = @descripcion,
                    activo = @activo,
                    updatedat = @updatedat,
                    updatedby = @updatedby
                WHERE codigoparametro = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@clave", (object)p.Clave ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@valor", (object)p.Valor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)p.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@activo", p.Activo);
                cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);
                cmd.Parameters.AddWithValue("@updatedby", codigoUsuario);
                cmd.Parameters.AddWithValue("@id", p.CodigoParametro);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Eliminar Soft
        // ==============================
        public bool EliminarSoft(int codigoParametro, int codigoUsuario)
        {
            const string sql = @"
                UPDATE aocr_tbparametro
                SET deletedat = @dt,
                    deletedby = @db,
                    activo = FALSE
                WHERE codigoparametro = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@db", codigoUsuario);
                cmd.Parameters.AddWithValue("@id", codigoParametro);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
