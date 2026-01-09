using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class ChecklistSolicitudDAO
    {
        private NpgsqlConnection CrearConexion() => ConexionDAO.CrearConexion();

        public List<ChecklistSolicitud> ObtenerPorSolicitud(int codigoSolicitud)
        {
            using (var con = CrearConexion())
            {
                con.Open();

                const string sql = @"
                    SELECT
                        cs.codigo_checklist_solicitud AS CodigoChecklistSolicitud,
                        cs.codigosolicitud            AS CodigoSolicitud,
                        cs.codigo_item                AS CodigoItem,
                        cs.cumple                     AS Cumple,
                        cs.observacion                AS Observacion,
                        cs.fecha_registro             AS FechaRegistro,
                        cs.usuario_registro           AS UsuarioRegistro,
                        ci.descripcion                AS DescripcionItem,
                        ci.orden                      AS Orden
                    FROM aocr_tbchecklist_solicitud cs
                    INNER JOIN aocr_tbchecklist_item ci
                        ON cs.codigo_item = ci.codigo_item
                    WHERE cs.codigosolicitud = @codigoSolicitud
                    ORDER BY ci.orden;";

                return con.Query<ChecklistSolicitud>(sql, new { codigoSolicitud }).ToList();
            }
        }

        public bool GuardarRespuestas(int codigoSolicitud, List<ChecklistSolicitud> respuestas)
        {
            using (var con = CrearConexion())
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    // Borrar respuestas anteriores
                    const string sqlDelete = @"DELETE FROM aocr_tbchecklist_solicitud WHERE codigosolicitud = @codigoSolicitud;";
                    con.Execute(sqlDelete, new { codigoSolicitud }, tran);

                    const string sqlInsert = @"
                        INSERT INTO aocr_tbchecklist_solicitud
                        (codigosolicitud, codigo_item, cumple, observacion, fecha_registro, usuario_registro)
                        VALUES
                        (@CodigoSolicitud, @CodigoItem, @Cumple, @Observacion, NOW(), @UsuarioRegistro);";

                    foreach (var r in respuestas)
                    {
                        con.Execute(sqlInsert, r, tran);
                    }

                    tran.Commit();
                    return true;
                }
            }
        }
    }
}
