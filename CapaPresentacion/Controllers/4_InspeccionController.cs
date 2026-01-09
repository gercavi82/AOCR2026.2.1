using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CapaNegocio;
using CapaModelo;
using CapaDatos.DAOs;

namespace CapaPresentacion.Controllers
{
    [Authorize]
    public class InspeccionController : Controller
    {
        private readonly InspeccionBL _bl;
        private readonly HallazgoBL _hallazgoBL;

        public InspeccionController()
        {
            _bl = new InspeccionBL();
            _hallazgoBL = new HallazgoBL();
        }

        // =====================================
        // Helpers de sesión
        // =====================================
        private int ObtenerCodigoUsuario()
        {
            if (Session["CodigoUsuario"] != null &&
                int.TryParse(Session["CodigoUsuario"].ToString(), out var id))
            {
                return id;
            }

            return 0;
        }

        // =====================================
        // GET: Inspeccion/Index
        // (por ahora lista vacía, porque InspeccionBL NO tiene Listar())
        // =====================================
        public ActionResult Index()
        {
            var lista = new List<Inspeccion>();
            return View(lista);
        }

        // =====================================
        // GET: Inspeccion/Detalle/5
        // =====================================
        public ActionResult Detalle(int id)
        {
            // Usamos directamente el DAO porque InspeccionBL NO tiene "Obtener"
            var inspeccion = InspeccionDAO.ObtenerPorId(id);
            if (inspeccion == null)
                return HttpNotFound();

            // Hallazgos asociados usando HallazgoBL
            ViewBag.Hallazgos = _hallazgoBL.ObtenerPorInspeccion(id);

            return View(inspeccion);
        }

        // =====================================
        // GET: Inspeccion/Crear?codigoSolicitud=123
        // =====================================
        public ActionResult Crear(int codigoSolicitud)
        {
            var modelo = new Inspeccion
            {
                CodigoSolicitud = codigoSolicitud
                // FechaProgramada si existe en tu modelo:
                // FechaProgramada = DateTime.Today.AddDays(1)
            };

            return View(modelo);
        }

        // =====================================
        // POST: Inspeccion/Crear
        // Usa InspeccionBL.Crear(Inspeccion, int codigoUsuario)
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Inspeccion model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var codigoUsuario = ObtenerCodigoUsuario();
            if (codigoUsuario <= 0)
            {
                ViewBag.Error = "No se pudo identificar el usuario en sesión.";
                return View(model);
            }

            // 🔴 ANTES: bool ok = _bl.Crear(model, codigoUsuario);
            // ✅ AHORA (método estático):
            bool ok = InspeccionBL.Crear(model, codigoUsuario);

            if (ok)
            {
                return RedirectToAction("Detalle", new { id = model.CodigoInspeccion });
            }

            ViewBag.Error = "No se pudo crear la inspección.";
            return View(model);
        }

        // =====================================
        // GET: Inspeccion/Editar/5
        // =====================================
        public ActionResult Editar(int id)
        {
            var inspeccion = InspeccionDAO.ObtenerPorId(id);
            if (inspeccion == null)
                return HttpNotFound();

            return View(inspeccion);
        }

        // =====================================
        // POST: Inspeccion/Editar
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Inspeccion model)
        {
            // Sin lógica en BL por ahora
            ModelState.AddModelError("", "La edición de inspecciones aún no está implementada en la capa de negocio.");
            return View(model);
        }

        // =====================================
        // POST: Inspeccion/CambiarEstado
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string estado)
        {
            TempData["Warning"] = "La funcionalidad de cambio de estado aún no está implementada en la capa de negocio.";
            return RedirectToAction("Detalle", new { id });
        }

        // =====================================
        // POST: Inspeccion/SubirInforme
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubirInforme(int id)
        {
            var archivo = Request.Files["Informe"];
            if (archivo != null && archivo.ContentLength > 0)
            {
                string carpetaVirtual = "~/Uploads/Inspecciones";
                string carpetaFisica = Server.MapPath(carpetaVirtual);

                if (!System.IO.Directory.Exists(carpetaFisica))
                    System.IO.Directory.CreateDirectory(carpetaFisica);

                string nombreArchivo = Guid.NewGuid().ToString("N") + ".pdf";
                string rutaFisica = System.IO.Path.Combine(carpetaFisica, nombreArchivo);
                archivo.SaveAs(rutaFisica);

                string rutaRelativa = $"{carpetaVirtual.TrimStart('~')}/{nombreArchivo}";

                // Aquí podrías luego llamar a un método BL para asociar el PDF
                // InspeccionBL.SubirInforme(id, rutaRelativa, ObtenerCodigoUsuario());

                TempData["Success"] = "Informe cargado en el servidor. Falta asociarlo en la lógica de negocio.";
            }
            else
            {
                TempData["Error"] = "No se recibió ningún archivo.";
            }

            return RedirectToAction("Detalle", new { id });
        }

        // =====================================
        // POST: Inspeccion/RegistrarHallazgo
        // Usa HallazgoBL.Crear(Hallazgo, string usuario)
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarHallazgo(Hallazgo h)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Detalle", new { id = h.CodigoInspeccion });

            var codigoUsuario = ObtenerCodigoUsuario();
            string usuarioNombre = User?.Identity?.Name ?? codigoUsuario.ToString();

            bool ok = _hallazgoBL.Crear(h, usuarioNombre);

            if (ok)
            {
                TempData["Success"] = "Hallazgo registrado correctamente.";
                return RedirectToAction("Detalle", new { id = h.CodigoInspeccion });
            }

            TempData["Error"] = "Error al registrar hallazgo.";
            return RedirectToAction("Detalle", new { id = h.CodigoInspeccion });
        }

        // =====================================
        // GET: Inspeccion/Cerrar/5
        // Usa InspeccionBL.CerrarInspeccion(int, string, int)
        // =====================================
        public ActionResult Cerrar(int id)
        {
            var codigoUsuario = ObtenerCodigoUsuario();
            string usuarioNombre = User?.Identity?.Name ?? codigoUsuario.ToString();

            // 🔴 ANTES: _bl.CerrarInspeccion(id, usuarioNombre);
            // ✅ AHORA (método estático con firma real):
            InspeccionBL.CerrarInspeccion(id, usuarioNombre, codigoUsuario);

            return RedirectToAction("Detalle", new { id });
        }
    }
}
