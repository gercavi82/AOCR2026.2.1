using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CapaNegocio;
using CapaModelo;
using CapaPresentacion.Models;

namespace CapaPresentacion.Controllers
{
    [Authorize]
    public class SolicitudController : Controller
    {
        // ✅ 1. INSTANCIAMOS LA LÓGICA DE NEGOCIO (¡Muy Importante!)
        private readonly SolicitudBL _solicitudBL = new SolicitudBL();

        // ==============================
        // Helpers de sesión/rol
        // ==============================
        private int ObtenerCodigoUsuario()
        {
            if (Session["CodigoUsuario"] != null)
                return Convert.ToInt32(Session["CodigoUsuario"]);
            return 0;
        }

        private string ObtenerRol()
        {
            if (Session["Rol"] != null)
                return Session["Rol"].ToString();

            if (User != null && User.IsInRole("Administrador")) return "Administrador";
            if (User != null && User.IsInRole("Financiero")) return "Financiero";
            if (User != null && User.IsInRole("Solicitante")) return "Solicitante";

            return string.Empty;
        }

        // ==============================
        // Combos (para ViewBag)
        // ==============================
        private void CargarCombos()
        {
            ViewBag.TiposSolicitud = new[]
            {
                "CERTIFICADO_OPERADOR_AEREO", "RENOVACION_AOC", "MODIFICACION_AOC"
            };

            ViewBag.TiposOperacion = new[]
            {
                "TRANSPORTE_PASAJEROS", "CARGA", "MIXTO", "CHARTER"
            };
        }

        private IEnumerable<SelectListItem> ObtenerTiposSolicitudSelect()
        {
            return new[]
            {
                new SelectListItem { Value = "CERTIFICADO_OPERADOR_AEREO", Text = "Certificado de Operador Aéreo" },
                new SelectListItem { Value = "RENOVACION_AOC", Text = "Renovación AOC" },
                new SelectListItem { Value = "MODIFICACION_AOC", Text = "Modificación AOC" }
            };
        }

        private IEnumerable<SelectListItem> ObtenerTiposOperacionSelect()
        {
            return new[]
            {
                new SelectListItem { Value = "TRANSPORTE_PASAJEROS", Text = "Transporte de Pasajeros" },
                new SelectListItem { Value = "CARGA", Text = "Carga" },
                new SelectListItem { Value = "MIXTO", Text = "Mixto" },
                new SelectListItem { Value = "CHARTER", Text = "Charter" }
            };
        }

        // =====================================
        // GET: Listar solicitudes
        // =====================================
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var codigoUsuario = ObtenerCodigoUsuario();
                var rol = ObtenerRol();

                // ✅ CORREGIDO: Usamos _solicitudBL (instancia)
                var solicitudes = _solicitudBL.ObtenerTodasActivas();

                if (rol == "Solicitante" && codigoUsuario > 0)
                {
                    solicitudes = solicitudes
                        .Where(s => s.CodigoUsuario == codigoUsuario)
                        .OrderByDescending(s => s.FechaSolicitud)
                        .ToList();
                }
                else
                {
                    solicitudes = solicitudes
                        .OrderByDescending(s => s.FechaSolicitud)
                        .ToList();
                }

                return View(solicitudes);
            }
            catch
            {
                TempData["Error"] = "Error al cargar las solicitudes";
                return View(new List<SolicitudAOCR>());
            }
        }

        // =====================================
        // GET: Ver detalle
        // =====================================
        [HttpGet]
        public ActionResult Detalle(int id)
        {
            try
            {
                // ✅ CORREGIDO: Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(id);

                if (solicitud == null)
                    return HttpNotFound();

                var codigoUsuario = ObtenerCodigoUsuario();
                var rol = ObtenerRol();

                if (rol == "Solicitante" && codigoUsuario > 0 && solicitud.CodigoUsuario != codigoUsuario)
                    return new HttpStatusCodeResult(403);

                return View(solicitud);
            }
            catch
            {
                TempData["Error"] = "Error al cargar el detalle";
                return RedirectToAction("Index");
            }
        }

        // =====================================
        // GET: Formulario crear
        // =====================================
        [HttpGet]
        [Authorize(Roles = "Solicitante,Administrador")]
        public ActionResult Crear()
        {
            CargarCombos();

            var vm = new CrearSolicitudViewModel
            {
                FechaInicioOperacion = DateTime.Today,
                FechaFinOperacion = DateTime.Today,
                TiposSolicitudLista = ObtenerTiposSolicitudSelect(),
                TiposOperacionLista = ObtenerTiposOperacionSelect()
            };

            return View(vm);
        }

        // =====================================
        // POST: Crear
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Solicitante,Administrador")]
        public ActionResult Crear(CrearSolicitudViewModel vm)
        {
            try
            {
                CargarCombos();
                vm.TiposSolicitudLista = ObtenerTiposSolicitudSelect();
                vm.TiposOperacionLista = ObtenerTiposOperacionSelect();

                if (!ModelState.IsValid)
                    return View(vm);

                var codigoUsuario = ObtenerCodigoUsuario();
                if (codigoUsuario <= 0)
                {
                    TempData["Error"] = "No se pudo identificar el usuario.";
                    return View(vm);
                }

                var entidad = new SolicitudAOCR
                {
                    CodigoUsuario = codigoUsuario,
                    Estado = "BORRADOR",
                    FechaSolicitud = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedBy = codigoUsuario.ToString(),

                    TipoSolicitud = Convert.ToInt32(vm.TipoSolicitud),
                    TipoOperacion = vm.TipoOperacion,
                    FechaInicioOperacion = vm.FechaInicioOperacion,
                    FechaFinOperacion = vm.FechaFinOperacion,
                    ObservacionesGenerales = vm.Observaciones
                };

                var year = DateTime.Now.Year;
                // ✅ CORREGIDO: Usamos _solicitudBL
                entidad.NumeroSolicitud = _solicitudBL.GenerarNumeroSolicitud(year);

                string mensaje;
                // ✅ CORREGIDO: Usamos _solicitudBL
                var ok = _solicitudBL.Crear(entidad, codigoUsuario, out mensaje);

                if (!ok)
                {
                    TempData["Error"] = mensaje;
                    return View(vm);
                }

                TempData["Success"] = "Solicitud creada correctamente.";
                return RedirectToAction("Detalle", new { id = entidad.CodigoSolicitud });
            }
            catch
            {
                TempData["Error"] = "Error al crear la solicitud.";
                return View(vm);
            }
        }

        // =====================================
        // POST: Enviar (cambiar estado)
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Solicitante,Administrador")]
        public ActionResult Enviar(int id)
        {
            try
            {
                var codigoUsuario = ObtenerCodigoUsuario();
                var esAdmin = User != null && User.IsInRole("Administrador");

                string mensaje;
                // ✅ CORREGIDO: Usamos _solicitudBL
                var ok = _solicitudBL.Enviar(id, codigoUsuario, esAdmin, out mensaje);

                TempData[ok ? "Success" : "Error"] = mensaje;
                return RedirectToAction("Detalle", new { id });
            }
            catch
            {
                TempData["Error"] = "Error al enviar la solicitud.";
                return RedirectToAction("Detalle", new { id });
            }
        }

        // =====================================
        // GET: Editar
        // =====================================
        [HttpGet]
        [Authorize(Roles = "Solicitante,Administrador")]
        public ActionResult Editar(int id)
        {
            try
            {
                CargarCombos();

                // ✅ CORREGIDO: Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(id);
                if (solicitud == null)
                    return HttpNotFound();

                var codigoUsuario = ObtenerCodigoUsuario();
                var rol = ObtenerRol();

                if (rol == "Solicitante" && solicitud.CodigoUsuario != codigoUsuario)
                    return new HttpStatusCodeResult(403);

                if (solicitud.Estado != "BORRADOR")
                {
                    TempData["Error"] = "Solo se pueden editar solicitudes en estado BORRADOR.";
                    return RedirectToAction("Detalle", new { id });
                }

                return View(solicitud);
            }
            catch
            {
                TempData["Error"] = "Error al cargar la edición.";
                return RedirectToAction("Index");
            }
        }

        // =====================================
        // POST: Editar
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Solicitante,Administrador")]
        public ActionResult Editar(int id, SolicitudAOCR modelo)
        {
            try
            {
                CargarCombos();

                if (id != modelo.CodigoSolicitud)
                    return new HttpStatusCodeResult(400);

                var codigoUsuario = ObtenerCodigoUsuario();
                var esAdmin = User != null && User.IsInRole("Administrador");

                if (!ModelState.IsValid)
                    return View(modelo);

                string mensaje;
                // ✅ CORREGIDO: Usamos _solicitudBL
                var ok = _solicitudBL.Actualizar(modelo, codigoUsuario, out mensaje, esAdmin);

                TempData[ok ? "Success" : "Error"] = mensaje;

                if (!ok)
                    return View(modelo);

                return RedirectToAction("Detalle", new { id });
            }
            catch
            {
                TempData["Error"] = "Error al actualizar la solicitud.";
                return View(modelo);
            }
        }

        // =====================================
        // POST: Eliminar (soft delete)
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult Eliminar(int id)
        {
            try
            {
                var codigoUsuario = ObtenerCodigoUsuario();
                string mensaje;
                // ✅ CORREGIDO: Usamos _solicitudBL
                var ok = _solicitudBL.EliminarSoft(id, codigoUsuario, out mensaje);

                TempData[ok ? "Success" : "Error"] = mensaje;
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Error al eliminar la solicitud.";
                return RedirectToAction("Index");
            }
        }

        // =====================================
        // GET: Mis Solicitudes (para Solicitante)
        // =====================================
        [HttpGet]
        [Authorize(Roles = "Solicitante")]
        public ActionResult MisSolicitudes()
        {
            try
            {
                var codigoUsuario = ObtenerCodigoUsuario();
                // ✅ CORREGIDO: Usamos _solicitudBL
                var solicitudes = _solicitudBL.ObtenerTodasActivas()
                    .Where(s => s.CodigoUsuario == codigoUsuario)
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToList();

                ViewBag.Titulo = "Mis Solicitudes";
                return View("Index", solicitudes);
            }
            catch
            {
                TempData["Error"] = "Error al cargar tus solicitudes.";
                return RedirectToAction("Index");
            }
        }

        // =======================================================
        //  SECCIÓN ASIGNAR TÉCNICO
        // =======================================================

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public ActionResult AsignarTecnico(int id)
        {
            // ✅ CORREGIDO: Usamos _solicitudBL
            var solicitud = _solicitudBL.ObtenerDetalle(id);
            if (solicitud == null) return HttpNotFound();

            // NOTA: Si UsuarioBL sigue siendo static, esta línea está bien.
            // Si también lo cambiaste a instancia, tendrás que instanciarlo arriba.
            ViewBag.Tecnicos = new SelectList(UsuarioBL.ListarTecnicos(), "Id", "NombreCompleto");

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult AsignarTecnico(int CodigoSolicitud, int tecnicoSeleccionado)
        {
            try
            {
                var codigoUsuario = ObtenerCodigoUsuario();
                string mensaje;

                // ✅ CORREGIDO: Usamos _solicitudBL
                var ok = _solicitudBL.AsignarTecnico(CodigoSolicitud, tecnicoSeleccionado, codigoUsuario, out mensaje);

                TempData[ok ? "Success" : "Error"] = mensaje;
                return RedirectToAction("Detalle", new { id = CodigoSolicitud });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al asignar técnico: " + ex.Message;
                return RedirectToAction("Detalle", new { id = CodigoSolicitud });
            }
        }
    }
}