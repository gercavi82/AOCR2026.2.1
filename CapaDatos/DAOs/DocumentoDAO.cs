using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Documentos (PostgreSQL + Dapper)
    /// Compatible con .NET Framework 4.7.2
    /// </summary>
    public class DocumentoDAO
    {
        // ✅ No instanciar ConexionDAO si ahora es estático
        private NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        // ============================================================
        // OBTENER TODOS
        // ============================================================
        public List<Documento> ObtenerTodos()
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT 
                        codigodocumento AS CodigoDocumento,
                        codigosolicitud AS CodigoSolicitud,
                        tipo_documento  AS TipoDocumento,
                        nombre_archivo  AS NombreArchivo,
                        ruta_archivo    AS RutaArchivo,
                        tamanio_archivo AS TamanioArchivo,
                        estado          AS Estado,
                        observaciones   AS Observaciones,
                        fecha_subida    AS FechaSubida,
                        usuario_registro AS UsuarioRegistro
                    FROM aocr_tbdocumento
                    ORDER BY fecha_subida DESC;";

                return con.Query<Documento>(sql).ToList();
            }
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================
        public Documento ObtenerPorId(int id)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigodocumento AS CodigoDocumento,
                        codigosolicitud AS CodigoSolicitud,
                        tipo_documento  AS TipoDocumento,
                        nombre_archivo  AS NombreArchivo,
                        ruta_archivo    AS RutaArchivo,
                        tamanio_archivo AS TamanioArchivo,
                        estado          AS Estado,
                        observaciones   AS Observaciones,
                        fecha_subida    AS FechaSubida,
                        usuario_registro AS UsuarioRegistro
                    FROM aocr_tbdocumento
                    WHERE codigodocumento = @id;";

                return con.QueryFirstOrDefault<Documento>(sql, new { id });
            }
        }

        // ============================================================
        // LISTAR POR SOLICITUD
        // ============================================================
        public List<Documento> ObtenerPorSolicitud(int solicitudId)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigodocumento AS CodigoDocumento,
                        codigosolicitud AS CodigoSolicitud,
                        tipo_documento  AS TipoDocumento,
                        nombre_archivo  AS NombreArchivo,
                        ruta_archivo    AS RutaArchivo,
                        tamanio_archivo AS TamanioArchivo,
                        estado          AS Estado,
                        observaciones   AS Observaciones,
                        fecha_subida    AS FechaSubida,
                        usuario_registro AS UsuarioRegistro
                    FROM aocr_tbdocumento
                    WHERE codigosolicitud = @solicitudId
                    ORDER BY fecha_subida DESC;";

                return con.Query<Documento>(sql, new { solicitudId }).ToList();
            }
        }

        // ============================================================
        // CREAR
        // ============================================================
        public int Crear(Documento d)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                // defensivo por si viene nulo desde capa superior
                if (d.FechaSubida == null)
                    d.FechaSubida = DateTime.Now;

                const string sql = @"
                    INSERT INTO aocr_tbdocumento
                    (
                        codigosolicitud,
                        tipo_documento,
                        nombre_archivo,
                        ruta_archivo,
                        tamanio_archivo,
                        estado,
                        observaciones,
                        fecha_subida,
                        usuario_registro
                    )
                    VALUES
                    (
                        @CodigoSolicitud,
                        @TipoDocumento,
                        @NombreArchivo,
                        @RutaArchivo,
                        @TamanioArchivo,
                        @Estado,
                        @Observaciones,
                        @FechaSubida,
                        @UsuarioRegistro
                    )
                    RETURNING codigodocumento;";

                return con.ExecuteScalar<int>(sql, d);
            }
        }

        // ============================================================
        // ACTUALIZAR
        // ============================================================
        public bool Actualizar(Documento d)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbdocumento SET
                        tipo_documento  = @TipoDocumento,
                        nombre_archivo  = @NombreArchivo,
                        ruta_archivo    = @RutaArchivo,
                        tamanio_archivo = @TamanioArchivo,
                        estado          = @Estado,
                        observaciones   = @Observaciones
                    WHERE codigodocumento = @CodigoDocumento;";

                return con.Execute(sql, d) > 0;
            }
        }

        // ============================================================
        // ELIMINAR (HARD DELETE)
        // ============================================================
        public bool Eliminar(int id)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"DELETE FROM aocr_tbdocumento WHERE codigodocumento = @id;";

                return con.Execute(sql, new { id }) > 0;
            }
        }
        // ============================================================
        // INSERTAR (wrapper para Crear) - COMPATIBILIDAD CON CONTROLLER
        // ============================================================
        public bool Insertar(Documento d)
        {
            // Reutiliza la lógica existente de Crear
            return Crear(d) > 0;
        }

        // Opcional: si en algún punto usaste Agregar(...) en el controller
        // también queda cubierto con este alias:
        public bool Agregar(Documento d)
        {
            return Crear(d) > 0;
        }

        public Documento ObtenerUltimoBorradorPorSolicitud(int solicitudId)
        {
            using (var con = CrearConexion())
            {
                con.Open();
                const string sql = @"
            SELECT
                codigodocumento AS CodigoDocumento,
                codigosolicitud AS CodigoSolicitud,
                tipo_documento  AS TipoDocumento,
                nombre_archivo  AS NombreArchivo,
                ruta_archivo    AS RutaArchivo,
                tamanio_archivo AS TamanioArchivo,
                estado          AS Estado,
                observaciones   AS Observaciones,
                fecha_subida    AS FechaSubida,
                usuario_registro AS UsuarioRegistro
            FROM aocr_tbdocumento
            WHERE codigosolicitud = @solicitudId 
              AND tipo_documento = 'BORRADOR_AOCR'
            ORDER BY fecha_subida DESC
            LIMIT 1;";

                return con.QueryFirstOrDefault<Documento>(sql, new { solicitudId });
            }
        }

    }
}
