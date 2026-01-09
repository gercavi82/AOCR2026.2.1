using System;
using System.Linq;
using System.Web.Mvc;
using CapaDatos.DAOs;
using Dapper;

namespace CapaPresentacion.Controllers
{
    [AllowAnonymous]
    public class UsuarioRolController : Controller
    {
        [HttpGet]
        public JsonResult ObtenerRoles()
        {
            try
            {
                // Conexión a PostgreSQL (Npgsql + Dapper)
                using (var cn = ConexionDAO.CrearConexion())
                {
                    cn.Open();

                    var rolesRaw = cn.Query(@"
                        SELECT codigorol, descripcion
                        FROM rol
                        WHERE activo IS TRUE
                          AND descripcion ILIKE '%representante%'
                        ORDER BY descripcion ASC
                    ").ToList();

                    var rolesFormateados = rolesRaw.Select(r => new
                    {
                        Value = r.codigorol.ToString(),
                        Text = r.descripcion
                    }).ToList();

                    return Json(rolesFormateados, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = "Fallo Postgres: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}