using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class ChecklistItemDAO
    {
        private NpgsqlConnection CrearConexion() => ConexionDAO.CrearConexion();

        public List<ChecklistItem> ObtenerActivos(string codigoFormulario)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        codigo_item       AS CodigoItem,
                        codigo_formulario AS CodigoFormulario,
                        descripcion       AS Descripcion,
                        orden,
                        activo
                    FROM aocr_tbchecklist_item
                    WHERE codigo_formulario = @codigoFormulario
                      AND activo = TRUE
                    ORDER BY orden;";

                return con.Query<ChecklistItem>(sql, new { codigoFormulario }).ToList();
            }
        }
    }
}
