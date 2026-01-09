using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class AeronaveSolicitudDAO
    {
        private NpgsqlConnection CrearConexion() => ConexionDAO.CrearConexion();

        // ============================================================
        // OBTENER POR SOLICITUD
        // ============================================================
        public List<AeronaveSolicitud> ObtenerPorSolicitud(int codigoSolicitud)
        {
            using (var con = CrearConexion())
            {
                const string sql = @"
                    SELECT
                        codigo_aeronave_solicitud AS CodigoAeronaveSolicitud,
                        codigosolicitud           AS CodigoSolicitud,
                        marca,
                        modelo,
                        serie,
                        matricula,
                        configuracion,
                        etapa_ruido              AS EtapaRuido,
                        fecha_registro           AS FechaRegistro,
                        usuario_registro         AS UsuarioRegistro
                    FROM aocr_tbaeronave_solicitud
                    WHERE codigosolicitud = @codigoSolicitud
                    ORDER BY codigo_aeronave_solicitud;";

                return con.Query<AeronaveSolicitud>(sql, new { codigoSolicitud }).ToList();
            }
        }

        // ============================================================
        // CREAR
        // ============================================================
        public int Crear(AeronaveSolicitud a)
        {
            using (var con = CrearConexion())
            {
                const string sql = @"
                    INSERT INTO aocr_tbaeronave_solicitud
                    (
                        codigosolicitud,
                        marca,
                        modelo,
                        serie,
                        matricula,
                        configuracion,
                        etapa_ruido,
                        fecha_registro,
                        usuario_registro
                    )
                    VALUES
                    (
                        @CodigoSolicitud,
                        @Marca,
                        @Modelo,
                        @Serie,
                        @Matricula,
                        @Configuracion,
                        @EtapaRuido,
                        NOW(),
                        @UsuarioRegistro
                    )
                    RETURNING codigo_aeronave_solicitud;";

                return con.ExecuteScalar<int>(sql, a);
            }
        }

        // ============================================================
        // ELIMINAR TODAS LAS AERONAVES DE UNA SOLICITUD
        // ============================================================
        public bool EliminarPorSolicitud(int codigoSolicitud)
        {
            using (var con = CrearConexion())
            {
                const string sql = @"
                    DELETE FROM aocr_tbaeronave_solicitud
                    WHERE codigosolicitud = @codigoSolicitud;";

                return con.Execute(sql, new { codigoSolicitud }) > 0;
            }
        }

        // ============================================================
        // NUEVO: ELIMINAR UNA AERONAVE POR ID
        // (para que AeronaveSolicitudBL.Elminar(...) compile)
        // ============================================================
        public bool Eliminar(int codigoAeronaveSolicitud)
        {
            using (var con = CrearConexion())
            {
                const string sql = @"
                    DELETE FROM aocr_tbaeronave_solicitud
                    WHERE codigo_aeronave_solicitud = @codigoAeronaveSolicitud;";

                return con.Execute(sql, new { codigoAeronaveSolicitud }) > 0;
            }
        }
    }
}
