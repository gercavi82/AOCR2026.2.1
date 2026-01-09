using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public static class UsuarioRolDAO
    {
        private static string ConnStr => ConexionDAO.ObtenerCadenaConexion();

        // ======================================================
        // ASIGNAR ROL A USUARIO
        // ======================================================
        public static bool Asignar(int codigoUsuario, int codigoRol)
        {
            using (IDbConnection db = new NpgsqlConnection(ConnStr))
            {
                string sql = @"
                    INSERT INTO usuariorol
                        (codigousuario, codigorol, fecha_asignacion)
                    VALUES
                        (@codigoUsuario, @codigoRol, NOW());";

                return db.Execute(sql, new { codigoUsuario, codigoRol }) > 0;
            }
        }

        // ======================================================
        // OBTENER ROLES POR USUARIO  ✅ (SOLUCIONA TU ERROR)
        // ======================================================
        public static List<UsuarioRol> ObtenerPorUsuario(int codigoUsuario)
        {
            using (IDbConnection db = new NpgsqlConnection(ConnStr))
            {
                string sql = @"
                    SELECT
                        codigousuariorol AS CodigoUsuarioRol,
                        codigousuario    AS CodigoUsuario,
                        codigorol        AS CodigoRol,
                        fecha_asignacion AS FechaAsignacion
                    FROM usuariorol
                    WHERE codigousuario = @codigoUsuario;";

                return db.Query<UsuarioRol>(sql, new { codigoUsuario }).ToList();
            }
        }

        // ======================================================
        // OBTENER USUARIOS POR ROL (ya lo usas en RolBL)
        // ======================================================
        public static List<UsuarioRol> ObtenerPorRol(int codigoRol)
        {
            using (IDbConnection db = new NpgsqlConnection(ConnStr))
            {
                string sql = @"
                    SELECT
                        codigousuariorol AS CodigoUsuarioRol,
                        codigousuario    AS CodigoUsuario,
                        codigorol        AS CodigoRol,
                        fecha_asignacion AS FechaAsignacion
                    FROM usuariorol
                    WHERE codigorol = @codigoRol;";

                return db.Query<UsuarioRol>(sql, new { codigoRol }).ToList();
            }
        }

        // ======================================================
        // ELIMINAR ROL DE USUARIO
        // ======================================================
        public static bool Eliminar(int codigoUsuario, int codigoRol)
        {
            using (IDbConnection db = new NpgsqlConnection(ConnStr))
            {
                string sql = @"
                    DELETE FROM usuariorol
                    WHERE codigousuario = @codigoUsuario
                      AND codigorol = @codigoRol;";

                return db.Execute(sql, new { codigoUsuario, codigoRol }) > 0;
            }
        }
    }
}
