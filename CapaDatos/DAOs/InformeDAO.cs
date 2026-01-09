using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Informes de Inspección (aocr_tbinforme) + reportes de aeronaves.
    /// Diseñado para trabajar con InformeBL (métodos estáticos).
    /// </summary>
    public class InformeDAO
    {
        // ============================================================
        // Helper de conexión (usa tu helper centralizado)
        // ============================================================
        private static NpgsqlConnection CrearConexion()
        {
            // Debes tener este método en tu ConexionDAO
            return ConexionDAO.CrearConexion();
        }

        #region CRUD INFORME (llamados desde InformeBL)

        /// <summary>
        /// Inserta un informe y devuelve el ID generado.
        /// </summary>
        public static int Insertar(Informe informe)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                // ⚠ Ajusta nombres de tabla/columnas según tu BD real.
                const string sql = @"
                    INSERT INTO aocr_tbinforme
                    (
                        codigoinforme,          -- opcional si es identity (quitar si es serial)
                        codigo_inspeccion,
                        codigo_tecnico,
                        titulo,
                        descripcion,
                        ruta_pdf,
                        estado,
                        fecha_informe,
                        fecha_registro,
                        usuario_registro
                    )
                    VALUES
                    (
                        @CodigoInforme,         -- si tu PK es serial, ELIMINA esta línea
                        @CodigoInspeccion,
                        @CodigoTecnico,
                        @Titulo,
                        @Descripcion,
                        @RutaPDF,
                        @Estado,
                        @FechaInforme,
                        @FechaRegistro,
                        @UsuarioRegistro
                    )
                    RETURNING codigoinforme;";

                // Dapper toma las propiedades del objeto "informe" como parámetros
                return con.ExecuteScalar<int>(sql, informe);
            }
        }

        /// <summary>
        /// Actualiza un informe existente.
        /// </summary>
        public static bool Actualizar(Informe informe)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbinforme SET
                        codigo_inspeccion = @CodigoInspeccion,
                        codigo_tecnico    = @CodigoTecnico,
                        titulo            = @Titulo,
                        descripcion       = @Descripcion,
                        ruta_pdf          = @RutaPDF,
                        estado            = @Estado,
                        fecha_informe     = @FechaInforme,
                        fecha_actualiza   = @FechaActualiza,
                        usuario_actualiza = @UsuarioActualiza
                    WHERE codigoinforme = @CodigoInforme;";

                return con.Execute(sql, informe) > 0;
            }
        }

        /// <summary>
        /// Elimina un informe por ID.
        /// (Si quieres soft-delete, cambia el DELETE por un UPDATE de estado).
        /// </summary>
        public static bool Eliminar(int codigoInforme)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    DELETE FROM aocr_tbinforme
                    WHERE codigoinforme = @codigoInforme;";

                return con.Execute(sql, new { codigoInforme }) > 0;
            }
        }

        /// <summary>
        /// Obtiene un informe por ID.
        /// </summary>
        public static Informe ObtenerPorId(int codigoInforme)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigoinforme    AS CodigoInforme,
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_tecnico    AS CodigoTecnico,
                        titulo            AS Titulo,
                        descripcion       AS Descripcion,
                        ruta_pdf          AS RutaPDF,
                        estado            AS Estado,
                        fecha_informe     AS FechaInforme,
                        fecha_registro    AS FechaRegistro,
                        usuario_registro  AS UsuarioRegistro,
                        fecha_actualiza   AS FechaActualiza,
                        usuario_actualiza AS UsuarioActualiza
                    FROM aocr_tbinforme
                    WHERE codigoinforme = @codigoInforme;";

                return con.QueryFirstOrDefault<Informe>(sql, new { codigoInforme });
            }
        }

        /// <summary>
        /// Devuelve todos los informes.
        /// </summary>
        public static List<Informe> ObtenerTodos()
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigoinforme    AS CodigoInforme,
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_tecnico    AS CodigoTecnico,
                        titulo            AS Titulo,
                        descripcion       AS Descripcion,
                        ruta_pdf          AS RutaPDF,
                        estado            AS Estado,
                        fecha_informe     AS FechaInforme,
                        fecha_registro    AS FechaRegistro,
                        usuario_registro  AS UsuarioRegistro,
                        fecha_actualiza   AS FechaActualiza,
                        usuario_actualiza AS UsuarioActualiza
                    FROM aocr_tbinforme
                    ORDER BY fecha_informe DESC;";

                return con.Query<Informe>(sql).ToList();
            }
        }

        /// <summary>
        /// Obtiene los informes de una inspección específica.
        /// </summary>
        public static List<Informe> ObtenerPorInspeccion(int codigoInspeccion)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigoinforme    AS CodigoInforme,
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_tecnico    AS CodigoTecnico,
                        titulo            AS Titulo,
                        descripcion       AS Descripcion,
                        ruta_pdf          AS RutaPDF,
                        estado            AS Estado,
                        fecha_informe     AS FechaInforme,
                        fecha_registro    AS FechaRegistro,
                        usuario_registro  AS UsuarioRegistro,
                        fecha_actualiza   AS FechaActualiza,
                        usuario_actualiza AS UsuarioActualiza
                    FROM aocr_tbinforme
                    WHERE codigo_inspeccion = @codigoInspeccion
                    ORDER BY fecha_informe DESC;";

                return con.Query<Informe>(sql, new { codigoInspeccion }).ToList();
            }
        }

        /// <summary>
        /// Obtiene los informes de un técnico específico.
        /// </summary>
        public static List<Informe> ObtenerPorTecnico(int codigoTecnico)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigoinforme    AS CodigoInforme,
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_tecnico    AS CodigoTecnico,
                        titulo            AS Titulo,
                        descripcion       AS Descripcion,
                        ruta_pdf          AS RutaPDF,
                        estado            AS Estado,
                        fecha_informe     AS FechaInforme,
                        fecha_registro    AS FechaRegistro,
                        usuario_registro  AS UsuarioRegistro,
                        fecha_actualiza   AS FechaActualiza,
                        usuario_actualiza AS UsuarioActualiza
                    FROM aocr_tbinforme
                    WHERE codigo_tecnico = @codigoTecnico
                    ORDER BY fecha_informe DESC;";

                return con.Query<Informe>(sql, new { codigoTecnico }).ToList();
            }
        }

        /// <summary>
        /// Obtiene los informes dentro de un rango de fechas.
        /// </summary>
        public static List<Informe> ObtenerPorFecha(DateTime fechaDesde, DateTime fechaHasta)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigoinforme    AS CodigoInforme,
                        codigo_inspeccion AS CodigoInspeccion,
                        codigo_tecnico    AS CodigoTecnico,
                        titulo            AS Titulo,
                        descripcion       AS Descripcion,
                        ruta_pdf          AS RutaPDF,
                        estado            AS Estado,
                        fecha_informe     AS FechaInforme,
                        fecha_registro    AS FechaRegistro,
                        usuario_registro  AS UsuarioRegistro,
                        fecha_actualiza   AS FechaActualiza,
                        usuario_actualiza AS UsuarioActualiza
                    FROM aocr_tbinforme
                    WHERE fecha_informe BETWEEN @fechaDesde AND @fechaHasta
                    ORDER BY fecha_informe DESC;";

                return con.Query<Informe>(sql, new { fechaDesde, fechaHasta }).ToList();
            }
        }

        #endregion

        #region REPORTES EXISTENTES (Aeronaves por solicitud)

        /// <summary>
        /// Devuelve las aeronaves asociadas a una solicitud específica.
        /// </summary>
        public static List<AeronaveSolicitud> ObtenerAeronavesPorSolicitud(int codigoSolicitud)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_aeronave_solicitud AS CodigoAeronaveSolicitud,
                        codigosolicitud           AS CodigoSolicitud,
                        marca                     AS Marca,
                        modelo                    AS Modelo,
                        serie                     AS Serie,
                        matricula                 AS Matricula,
                        configuracion             AS Configuracion,
                        etapa_ruido               AS EtapaRuido,
                        fecha_registro            AS FechaRegistro,
                        usuario_registro          AS UsuarioRegistro
                    FROM aocr_tbaeronave_solicitud
                    WHERE codigosolicitud = @codigoSolicitud
                    ORDER BY codigo_aeronave_solicitud;";

                return con.Query<AeronaveSolicitud>(sql, new { codigoSolicitud }).ToList();
            }
        }

        /// <summary>
        /// Resumen de cantidad de aeronaves por solicitud.
        /// </summary>
        public static List<ResumenAeronavesPorSolicitud> ObtenerResumenAeronavesPorSolicitud()
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigosolicitud           AS CodigoSolicitud,
                        COUNT(*)                  AS CantidadAeronaves
                    FROM aocr_tbaeronave_solicitud
                    GROUP BY codigosolicitud
                    ORDER BY codigosolicitud;";

                return con.Query<ResumenAeronavesPorSolicitud>(sql).ToList();
            }
        }

        #endregion
    }

    /// <summary>
    /// DTO simple para resumen de aeronaves por solicitud.
    /// </summary>
    public class ResumenAeronavesPorSolicitud
    {
        public int CodigoSolicitud { get; set; }
        public int CantidadAeronaves { get; set; }
    }
}
