using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Dirección (PostgreSQL + Dapper)
    /// Compatible con .NET Framework 4.7.2
    /// </summary>
    public class DireccionDAO
    {
        // ✅ Si ConexionDAO es estático, no se debe instanciar
        private NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        // ============================================================
        // OBTENER TODOS
        // ============================================================
        public List<Direccion> ObtenerTodos()
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT 
                        codigo_direccion AS CodigoDireccion,
                        calle AS Calle,
                        ciudad AS Ciudad,
                        provincia AS Provincia,
                        pais AS Pais,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        deleted_at AS DeletedAt
                    FROM aocr_tbdireccion
                    WHERE deleted_at IS NULL
                    ORDER BY codigo_direccion DESC;";

                return con.Query<Direccion>(sql).ToList();
            }
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================
        public Direccion ObtenerPorId(int id)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT 
                        codigo_direccion AS CodigoDireccion,
                        calle AS Calle,
                        ciudad AS Ciudad,
                        provincia AS Provincia,
                        pais AS Pais
                    FROM aocr_tbdireccion
                    WHERE codigo_direccion = @id
                      AND deleted_at IS NULL;";

                return con.QueryFirstOrDefault<Direccion>(sql, new { id });
            }
        }

        // ============================================================
        // CREAR
        // ============================================================
        public int Crear(Direccion d)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    INSERT INTO aocr_tbdireccion
                    (calle, ciudad, provincia, pais, created_at, created_by)
                    VALUES
                    (@Calle, @Ciudad, @Provincia, @Pais, CURRENT_TIMESTAMP, @CreatedBy)
                    RETURNING codigo_direccion;";

                return con.ExecuteScalar<int>(sql, d);
            }
        }

        // ============================================================
        // ACTUALIZAR
        // ============================================================
        public bool Actualizar(Direccion d)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbdireccion SET
                        calle = @Calle,
                        ciudad = @Ciudad,
                        provincia = @Provincia,
                        pais = @Pais,
                        updated_at = CURRENT_TIMESTAMP,
                        updated_by = @UpdatedBy
                    WHERE codigo_direccion = @CodigoDireccion;";

                return con.Execute(sql, d) > 0;
            }
        }

        // ============================================================
        // ELIMINAR (SOFT DELETE)
        // ============================================================
        public bool Eliminar(int id, string usuario)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    UPDATE aocr_tbdireccion SET
                        deleted_at = CURRENT_TIMESTAMP,
                        deleted_by = @usuario
                    WHERE codigo_direccion = @id;";

                return con.Execute(sql, new { id, usuario }) > 0;
            }
        }
    }
}
