using System;
using System.Web.Mvc;
using CapaNegocio;
using CapaModelo;

namespace CapaPresentacion.Controllers
{
    public class HallazgoController : Controller
    {
        private readonly HallazgoBL _bl;

        public HallazgoController()
        {
            _bl = new HallazgoBL();
        }

        // Lista de hallazgos por inspección
        public ActionResult Index(int inspeccionId)
        {
            ViewBag.InspeccionId = inspeccionId;
            var lista = _bl.ObtenerPorInspeccion(inspeccionId);
            return View(lista);
        }

        public ActionResult Crear(int inspeccionId)
        {
            var h = new Hallazgo
            {
                CodigoInspeccion = inspeccionId
            };

            return View(h);
        }

        [HttpPost]
        public ActionResult Crear(Hallazgo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Pasamos el usuario requerido por HallazgoBL.Crear(Hallazgo, string)
            var usuario = User != null && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "SYSTEM";

            if (_bl.Crear(model, usuario))
                return RedirectToAction("Index", new { inspeccionId = model.CodigoInspeccion });

            ViewBag.Error = "No se pudo registrar el hallazgo.";
            return View(model);
        }

        public ActionResult Editar(int id)
        {
            // HallazgoBL tiene ObtenerPorId, no Obtener
            var modelo = _bl.ObtenerPorId(id);

            if (modelo == null)
                return HttpNotFound();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult Editar(Hallazgo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = User != null && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "SYSTEM";

            _bl.Actualizar(model, usuario);
            return RedirectToAction("Index", new { inspeccionId = model.CodigoInspeccion });
        }

        [HttpPost]
        public ActionResult Cerrar(int id, int inspeccionId)
        {
            var usuario = User != null && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "SYSTEM";

            _bl.CerrarHallazgo(id, usuario);
            return RedirectToAction("Index", new { inspeccionId });
        }

        [HttpPost]
        public ActionResult Eliminar(int id, int inspeccionId)
        {
            var usuario = User != null && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "SYSTEM";

            _bl.Eliminar(id, usuario);
            return RedirectToAction("Index", new { inspeccionId });
        }
    }
}
