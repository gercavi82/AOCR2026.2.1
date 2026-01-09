using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Inspección usando Dapper + PostgreSQL
    /// Compatible con .NET Framework 4.7.2
    ///
    /// ✅ Versión estática para ser compatible con tus BL
    /// ✅ Elimina el uso de Map() (ya no existe)
    /// ✅ No mezcla ADO.NET manual con Dapper
    /// </summary>
    public class InspeccionDAO
    {
        // ✅ Conexión estática, compatible con llamadas estáticas del BL
        private static NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        // =========================================================
        // LISTAR POR SOLICITUD
        // =========================================================
        public static List<Inspeccion> ObtenerPorSolicitud(int idSolicitud)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_solicitud  AS CodigoSolicitud,
                        codigotecnico     AS CodigoTecnico,
                        fecha_inspeccion  AS FechaInspeccion,
                        inspector         AS Inspector,
                        informe           AS Informe,
                        resultado         AS Resultado,
                        fecha_cierre      AS FechaCierre,
                        created_at        AS CreatedAt,
                        created_by        AS CreatedBy,
                        updated_at        AS UpdatedAt,
                        updated_by        AS UpdatedBy,
                        deleted_at        AS DeletedAt,
                        deleted_by        AS DeletedBy
                    FROM aocr_tbinspeccion
                    WHERE codigo_solicitud = @idSolicitud
                      AND deleted_at IS NULL
                    ORDER BY codigo_inspeccion DESC;";

                return con.Query<Inspeccion>(sql, new { idSolicitud }).ToList();
            }
        }

        // =========================================================
        // OBTENER POR ID
        // =========================================================
        public static Inspeccion ObtenerPorId(int id)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_solicitud  AS CodigoSolicitud,
                        codigotecnico     AS CodigoTecnico,
                        fecha_inspeccion  AS FechaInspeccion,
                        inspector         AS Inspector,
                        informe           AS Informe,
                        resultado         AS Resultado,
                        fecha_cierre      AS FechaCierre,
                        created_at        AS CreatedAt,
                        created_by        AS CreatedBy,
                        updated_at        AS UpdatedAt,
                        updated_by        AS UpdatedBy,
                        deleted_at        AS DeletedAt,
                        deleted_by        AS DeletedBy
                    FROM aocr_tbinspeccion
                    WHERE codigo_inspeccion = @id
                      AND deleted_at IS NULL;";

                return con.QueryFirstOrDefault<Inspeccion>(sql, new { id });
            }
        }

        // =========================================================
        // CREAR
        // =========================================================
        public static int Crear(Inspeccion i)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                if (i == null) throw new ArgumentNullException(nameof(i));

                // Si en tu modelo estos campos pueden venir null,
                // asegúrate de asignarlos antes de llamar al DAO.
                if (!i.CreatedAt.HasValue) i.CreatedAt = DateTime.Now;

                const string sql = @"
                    INSERT INTO aocr_tbinspeccion
                    (
                        codigo_solicitud,
                        codigotecnico,
                        fecha_inspeccion,
                        inspector,
                        informe,
                        resultado,
                        created_at,
                        created_by
                    )
                    VALUES
                    (
                        @CodigoSolicitud,
                        @CodigoTecnico,
                        @FechaInspeccion,
                        @Inspector,
                        @Informe,
                        @Resultado,
                        @CreatedAt,
                        @CreatedBy
                    )
                    RETURNING codigo_inspeccion;";

                return con.ExecuteScalar<int>(sql, i);
            }
        }

        // =========================================================
        // ACTUALIZAR INFORME
        // =========================================================
        public static int GuardarInforme(int idInspeccion, string informe, int codigoUsuario)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbinspeccion
                    SET informe = @informe,
                        updated_at = CURRENT_TIMESTAMP,
                        updated_by = @codigoUsuario
                    WHERE codigo_inspeccion = @idInspeccion
                      AND deleted_at IS NULL;";

                return con.Execute(sql, new { idInspeccion, informe, codigoUsuario });
            }
        }

        // =========================================================
        // CERRAR INSPECCIÓN (APROBADA / RECHAZADA)
        // =========================================================
        public static int CerrarInspeccion(int idInspeccion, string resultado, int codigoUsuario)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbinspeccion
                    SET resultado = @resultado,
                        fecha_cierre = CURRENT_TIMESTAMP,
                        updated_at = CURRENT_TIMESTAMP,
                        updated_by = @codigoUsuario
                    WHERE codigo_inspeccion = @idInspeccion
                      AND deleted_at IS NULL;";

                return con.Execute(sql, new { idInspeccion, resultado, codigoUsuario });
            }
        }

        // =========================================================
        // SOFT DELETE
        // =========================================================
        public static int EliminarSoft(int idInspeccion, int codigoUsuario)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbinspeccion
                    SET deleted_at = CURRENT_TIMESTAMP,
                        deleted_by = @codigoUsuario
                    WHERE codigo_inspeccion = @idInspeccion
                      AND deleted_at IS NULL;";

                return con.Execute(sql, new { idInspeccion, codigoUsuario });
            }
        }

        // =========================================================
        // OBTENER POR TÉCNICO
        // =========================================================
        public static List<Inspeccion> ObtenerPorTecnico(int codigoTecnico)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                // ✅ IMPORTANTE:
                // - Confirmo el nombre de columna "codigotecnico"
                // - Si tu tabla usa otro nombre, cámbialo aquí.
                const string sql = @"
                    SELECT
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_solicitud  AS CodigoSolicitud,
                        codigotecnico     AS CodigoTecnico,
                        fecha_inspeccion  AS FechaInspeccion,
                        inspector         AS Inspector,
                        informe           AS Informe,
                        resultado         AS Resultado,
                        fecha_cierre      AS FechaCierre,
                        created_at        AS CreatedAt,
                        created_by        AS CreatedBy,
                        updated_at        AS UpdatedAt,
                        updated_by        AS UpdatedBy,
                        deleted_at        AS DeletedAt,
                        deleted_by        AS DeletedBy
                    FROM aocr_tbinspeccion
                    WHERE codigotecnico = @codigoTecnico
                      AND deleted_at IS NULL
                    ORDER BY codigo_inspeccion DESC;";

                return con.Query<Inspeccion>(sql, new { codigoTecnico }).ToList();
            }
        }
    }
}
