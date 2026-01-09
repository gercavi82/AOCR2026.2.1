using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Certificado (PostgreSQL + Dapper)
    /// Compatible con .NET Framework 4.7.2
    /// Usa ConexionDAO estático.
    /// </summary>
    public class CertificadoDAO
    {
        // ✅ Si ConexionDAO es estático, NO se instancia
        private NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================
        public Certificado ObtenerPorId(int id)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_certificado   AS CodigoCertificado,
                        codigo_solicitud     AS CodigoSolicitud,
                        numero_certificado   AS NumeroCertificado,
                        fecha_emision        AS FechaEmision,
                        fecha_vencimiento    AS FechaVencimiento,
                        estado               AS Estado,
                        created_at           AS CreatedAt,
                        created_by           AS CreatedBy,
                        updated_at           AS UpdatedAt,
                        updated_by           AS UpdatedBy,
                        deleted_at           AS DeletedAt,
                        deleted_by           AS DeletedBy
                    FROM aocr_tbcertificado
                    WHERE codigo_certificado = @id
                      AND (deleted_at IS NULL);";

                return con.QueryFirstOrDefault<Certificado>(sql, new { id });
            }
        }

        // ============================================================
        // OBTENER POR SOLICITUD
        // ============================================================
        public Certificado ObtenerPorSolicitud(int codigoSolicitud)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_certificado   AS CodigoCertificado,
                        codigo_solicitud     AS CodigoSolicitud,
                        numero_certificado   AS NumeroCertificado,
                        fecha_emision        AS FechaEmision,
                        fecha_vencimiento    AS FechaVencimiento,
                        estado               AS Estado,
                        created_at           AS CreatedAt,
                        created_by           AS CreatedBy,
                        updated_at           AS UpdatedAt,
                        updated_by           AS UpdatedBy,
                        deleted_at           AS DeletedAt,
                        deleted_by           AS DeletedBy
                    FROM aocr_tbcertificado
                    WHERE codigo_solicitud = @codigoSolicitud
                      AND (deleted_at IS NULL)
                    ORDER BY codigo_certificado DESC
                    LIMIT 1;";

                return con.QueryFirstOrDefault<Certificado>(sql, new { codigoSolicitud });
            }
        }

        // ============================================================
        // CREAR
        // ============================================================
        public int Crear(Certificado c)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    INSERT INTO aocr_tbcertificado
                    (
                        codigo_solicitud,
                        numero_certificado,
                        fecha_emision,
                        fecha_vencimiento,
                        estado,
                        created_at,
                        created_by
                    )
                    VALUES
                    (
                        @CodigoSolicitud,
                        @NumeroCertificado,
                        @FechaEmision,
                        @FechaVencimiento,
                        @Estado,
                        CURRENT_TIMESTAMP,
                        @CreatedBy
                    )
                    RETURNING codigo_certificado;";

                return con.ExecuteScalar<int>(sql, c);
            }
        }

        // ============================================================
        // ACTUALIZAR
        // ============================================================
        public bool Actualizar(Certificado c)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbcertificado SET
                        numero_certificado = @NumeroCertificado,
                        fecha_emision = @FechaEmision,
                        fecha_vencimiento = @FechaVencimiento,
                        estado = @Estado,
                        updated_at = CURRENT_TIMESTAMP,
                        updated_by = @UpdatedBy
                    WHERE codigo_certificado = @CodigoCertificado
                      AND (deleted_at IS NULL);";

                return con.Execute(sql, c) > 0;
            }
        }
    }
}
