using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Hallazgos usando Dapper + PostgreSQL
    /// Compatible con .NET Framework 4.7.2
    /// Usa ConexionDAO estático.
    /// </summary>
    public class HallazgoDAO
    {
        // ✅ NO instanciar ConexionDAO
        private NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        // ============================================================
        // LISTAR POR INSPECCIÓN
        // ============================================================
        public List<Hallazgo> ObtenerPorInspeccion(int codigoInspeccion)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_hallazgo     AS CodigoHallazgo,
                        codigo_inspeccion   AS CodigoInspeccion,
                        descripcion         AS Descripcion,
                        criticidad          AS Criticidad,
                        estado              AS Estado,
                        fecha_deteccion     AS FechaDeteccion,
                        fecha_cierre        AS FechaCierre,
                        created_at          AS CreatedAt,
                        created_by          AS CreatedBy,
                        updated_at          AS UpdatedAt,
                        updated_by          AS UpdatedBy,
                        deleted_at          AS DeletedAt,
                        deleted_by          AS DeletedBy
                    FROM aocr_tbhallazgo
                    WHERE codigo_inspeccion = @codigoInspeccion
                      AND deleted_at IS NULL
                    ORDER BY created_at DESC;";

                return con.Query<Hallazgo>(sql, new { codigoInspeccion }).ToList();
            }
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================
        public Hallazgo ObtenerPorId(int id)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_hallazgo     AS CodigoHallazgo,
                        codigo_inspeccion   AS CodigoInspeccion,
                        descripcion         AS Descripcion,
                        criticidad          AS Criticidad,
                        estado              AS Estado,
                        fecha_deteccion     AS FechaDeteccion,
                        fecha_cierre        AS FechaCierre,
                        created_at          AS CreatedAt,
                        created_by          AS CreatedBy,
                        updated_at          AS UpdatedAt,
                        updated_by          AS UpdatedBy,
                        deleted_at          AS DeletedAt,
                        deleted_by          AS DeletedBy
                    FROM aocr_tbhallazgo
                    WHERE codigo_hallazgo = @id
                      AND deleted_at IS NULL;";

                return con.QueryFirstOrDefault<Hallazgo>(sql, new { id });
            }
        }

        // ============================================================
        // CREAR
        // ============================================================
        public int Crear(Hallazgo h)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                // ✅ Valores por defecto defensivos para DateTime (NO nullable)
                if (h.FechaDeteccion == default(DateTime))
                    h.FechaDeteccion = DateTime.Now;

                if (h.CreatedAt == default(DateTime))
                    h.CreatedAt = DateTime.Now;

                const string sql = @"
                    INSERT INTO aocr_tbhallazgo
                    (
                        codigo_inspeccion,
                        descripcion,
                        criticidad,
                        estado,
                        fecha_deteccion,
                        created_at,
                        created_by
                    )
                    VALUES
                    (
                        @CodigoInspeccion,
                        @Descripcion,
                        @Criticidad,
                        @Estado,
                        @FechaDeteccion,
                        @CreatedAt,
                        @CreatedBy
                    )
                    RETURNING codigo_hallazgo;";

                return con.ExecuteScalar<int>(sql, h);
            }
        }

        // ============================================================
        // ACTUALIZAR
        // ============================================================
        public int Actualizar(Hallazgo h)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                // ✅ Asignar siempre updated_at
                h.UpdatedAt = DateTime.Now;

                const string sql = @"
                    UPDATE aocr_tbhallazgo SET
                        descripcion = @Descripcion,
                        criticidad  = @Criticidad,
                        estado      = @Estado,
                        updated_at  = @UpdatedAt,
                        updated_by  = @UpdatedBy
                    WHERE codigo_hallazgo = @CodigoHallazgo
                      AND deleted_at IS NULL;";

                return con.Execute(sql, h);
            }
        }

        // ============================================================
        // CERRAR HALLAZGO
        // ============================================================
        public int Cerrar(Hallazgo h)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                // ✅ Asignar fechas de cierre/actualización
                h.FechaCierre = DateTime.Now;
                h.UpdatedAt = DateTime.Now;

                const string sql = @"
                    UPDATE aocr_tbhallazgo SET
                        estado       = @Estado,
                        fecha_cierre = @FechaCierre,
                        updated_at   = @UpdatedAt,
                        updated_by   = @UpdatedBy
                    WHERE codigo_hallazgo = @CodigoHallazgo
                      AND deleted_at IS NULL;";

                return con.Execute(sql, h);
            }
        }

        // ============================================================
        // ELIMINAR (SOFT DELETE)
        // ============================================================
        public int Eliminar(int idHallazgo, string usuario)
        {
            const string sql = @"
        UPDATE aocr_tbhallazgo
        SET
            deletedat = @deletedAt,
            deletedby = @deletedBy
        WHERE codigohallazgo = @id;";

            using (var cn = CrearConexion())   // Usa tu mismo helper de conexión en HallazgoDAO
            using (var cmd = new Npgsql.NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@deletedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@deletedBy", (object)usuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", idHallazgo);

                cn.Open();
                // Devuelve el número de filas afectadas (HallazgoBL espera > 0)
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
