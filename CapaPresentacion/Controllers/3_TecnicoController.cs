using System;
using System.Web.Mvc;
using CapaModelo;
using CapaNegocio;

namespace CapaPresentacion.Controllers
{
    public class TecnicoController : Controller
    {
        // Puedes dejar esto aunque ya no lo usemos (no da error, solo warning)
        private readonly TecnicoBL _tecnicoBL;

        public TecnicoController()
        {
            _tecnicoBL = new TecnicoBL();
        }

        // =======================================================
        // LISTADO
        // =======================================================
        public ActionResult Index()
        {
            // ✅ TecnicoBL.ObtenerTodos() es estático → se llama por el tipo
            var lista = TecnicoBL.ObtenerTodos();
            return View(lista);
        }

        // =======================================================
        // CREAR
        // =======================================================
        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crear(Tecnico modelo)
        {
            if (ModelState.IsValid)
            {
                // Asumimos que en TecnicoBL existe:
                // public static bool Insertar(Tecnico t, out string mensaje)
                string mensaje;
                bool ok = TecnicoBL.Insertar(modelo, out mensaje);

                if (!ok)
                {
                    ViewBag.Error = mensaje;
                    return View(modelo);
                }

                TempData["Success"] = "Técnico creado correctamente.";
                return RedirectToAction("Index");
            }

            return View(modelo);
        }

        // =======================================================
        // EDITAR
        // =======================================================
        public ActionResult Editar(int id)
        {
            // ✅ También estático
            var modelo = TecnicoBL.ObtenerPorId(id);

            if (modelo == null)
            {
                TempData["Error"] = "Técnico no encontrado.";
                return RedirectToAction("Index");
            }

            return View(modelo);
        }

        [HttpPost]
        public ActionResult Editar(Tecnico modelo)
        {
            if (ModelState.IsValid)
            {
                // ✅ Firma real: Actualizar(Tecnico, out string mensaje)
                string mensaje;
                bool ok = TecnicoBL.Actualizar(modelo, out mensaje);

                if (!ok)
                {
                    ViewBag.Error = mensaje;
                    return View(modelo);
                }

                TempData["Success"] = "Técnico actualizado correctamente.";
                return RedirectToAction("Index");
            }

            return View(modelo);
        }

        // =======================================================
        // ELIMINAR
        // =======================================================
        public ActionResult Eliminar(int id)
        {
            // ✅ Firma real: Eliminar(int, out string mensaje)
            string mensaje;
            bool ok = TecnicoBL.Eliminar(id, out mensaje);

            TempData[ok ? "Success" : "Error"] =
                ok ? "Técnico eliminado correctamente." : mensaje;

            return RedirectToAction("Index");
        }
    }
}
