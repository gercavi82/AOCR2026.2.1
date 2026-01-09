using System.Web.Mvc;
using CapaNegocio;

namespace CapaPresentacion.Controllers
{
    public class HistorialEstadoController : Controller
    {
        // ✅ 1. Declaramos la variable privada
        private readonly HistorialEstadoBL _historialBL;

        public HistorialEstadoController()
        {
            // ✅ 2. Instanciamos la lógica de negocio en el constructor
            _historialBL = new HistorialEstadoBL();
        }

        [HttpGet]
        public ActionResult PorSolicitud(int id)
        {
            // ✅ 3. Usamos la instancia (_historialBL) en lugar de la clase estática
            var lista = _historialBL.ObtenerPorSolicitud(id);
            return View(lista);
        }
    }
}