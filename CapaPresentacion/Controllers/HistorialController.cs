using System.Web.Mvc;
using CapaNegocio;

namespace CapaPresentacion.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        // ✅ 1. Declaramos la variable privada
        private readonly HistorialEstadoBL _historialBL;

        public HistorialController()
        {
            // ✅ 2. La instanciamos en el constructor
            _historialBL = new HistorialEstadoBL();
        }

        [HttpGet]
        public ActionResult Ver(int id)
        {
            // ✅ 3. Usamos la instancia (_historialBL)
            var historial = _historialBL.ObtenerPorSolicitud(id);

            ViewBag.SolicitudId = id;
            return PartialView("_HistorialEstados", historial);
        }
    }
}