using System;
using System.Web.Mvc;
using CapaDatos.DAOs; // Donde reside EmpresaAS400DAO

namespace CapaPresentacion.Controllers
{
    [AllowAnonymous] // Permite el acceso desde el Login sin estar autenticado
    public class EmpresaController : Controller
    {
        [HttpGet]
        public JsonResult ObtenerEmpresas()
        {
            try
            {
                // Conexión directa al AS/400 (IP 190.152.8.185)
                var dao = new EmpresaAS400DAO();
                var empresas = dao.ObtenerEmpresas();

                // Retorna la lista de empresas (Codigo y Nombre)
                return Json(empresas, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Si falla el AS/400, no afecta el resto de la página
                Response.StatusCode = 500;
                return Json(new { error = "Fallo conexión AS400: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}