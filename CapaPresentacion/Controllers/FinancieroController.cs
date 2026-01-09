using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaModelo;

namespace CapaPresentacion.Controllers
{
    [Authorize]
    public class FinancieroController : Controller
    {
        // ✅ 1. Declaramos la instancia de la Lógica de Negocio de Solicitudes
        private readonly SolicitudBL _solicitudBL;
        private readonly PagoBL _pagoBL;
        private readonly HistorialEstadoBL _historialBL;

        public FinancieroController()
        {
            // ✅ 2. Inicializamos la instancia en el constructor
            _solicitudBL = new SolicitudBL();
            _pagoBL = new PagoBL();
            _historialBL = new HistorialEstadoBL();
        }

        // GET: Financiero/Index
        [HttpGet]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult Index()
        {
            try
            {
                // ✅ 3. Usamos _solicitudBL (instancia) en lugar de SolicitudBL (clase)
                var solicitudesPendientes = _solicitudBL.ObtenerTodasActivas();

                if (solicitudesPendientes != null)
                {
                    solicitudesPendientes = solicitudesPendientes
                        .Where(s => string.Equals(s.Estado, "ENVIADO", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                int pendientesRevision = (solicitudesPendientes != null)
                    ? solicitudesPendientes.Count
                    : 0;

                var pagosHoy = _pagoBL.ObtenerPagosValidadosHoy();
                int pagosValidadosHoy = (pagosHoy != null) ? pagosHoy.Count : 0;

                decimal montoRecaudadoMes = _pagoBL.ObtenerMontoRecaudadoMes(DateTime.Now.Year, DateTime.Now.Month);

                var estadisticas = new
                {
                    PendientesRevision = pendientesRevision,
                    PagosValidadosHoy = pagosValidadosHoy,
                    MontoRecaudadoMes = montoRecaudadoMes
                };

                ViewBag.Estadisticas = estadisticas;

                return View(solicitudesPendientes != null ? solicitudesPendientes : new List<SolicitudAOCR>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar dashboard: {ex.Message}";
                return View(new List<SolicitudAOCR>());
            }
        }

        // GET: Financiero/RevisarSolicitud/5
        [HttpGet]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult RevisarSolicitud(int id)
        {
            try
            {
                // ✅ Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(id);

                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index");
                }

                if (!string.Equals(solicitud.Estado, "ENVIADO", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Warning"] = $"Esta solicitud está en estado {solicitud.Estado}";
                }

                var pagos = _pagoBL.ObtenerPorSolicitud(id);
                ViewBag.Pagos = pagos;

                ViewBag.MontoSugerido = ObtenerMontoSegunTipo(null);

                return View(solicitud);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Financiero/AprobarRevisionFinanciera
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult AprobarRevisionFinanciera(int solicitudId, string observaciones)
        {
            try
            {
                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");

                // ✅ Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(solicitudId);

                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index");
                }

                if (!string.Equals(solicitud.Estado, "ENVIADO", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Solo se pueden aprobar solicitudes en estado ENVIADO";
                    return RedirectToAction("RevisarSolicitud", new { id = solicitudId });
                }

                var pagos = _pagoBL.ObtenerPorSolicitud(solicitudId);
                var pagoAprobado = pagos != null && pagos.Any(p => p.Estado == "APROBADO");

                if (!pagoAprobado)
                {
                    TempData["Error"] = "Debe validar y aprobar el pago antes de continuar";
                    return RedirectToAction("RevisarSolicitud", new { id = solicitudId });
                }

                solicitud.Estado = "EN_REVISION_TECNICA";

                string mensaje;
                // ✅ Usamos _solicitudBL
                bool resultado = _solicitudBL.Actualizar(solicitud, codigoUsuario, out mensaje, esAdmin: true);

                if (resultado)
                {
                    TempData["Success"] = "Revisión financiera aprobada. Solicitud enviada al área técnica.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = string.IsNullOrEmpty(mensaje) ? "No se pudo aprobar la revisión" : mensaje;
                    return RedirectToAction("RevisarSolicitud", new { id = solicitudId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Financiero/RechazarRevisionFinanciera
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult RechazarRevisionFinanciera(int solicitudId, string motivoRechazo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["Error"] = "Debe especificar el motivo del rechazo";
                    return RedirectToAction("RevisarSolicitud", new { id = solicitudId });
                }

                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");

                // ✅ Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(solicitudId);

                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index");
                }

                solicitud.Estado = "RECHAZADO";

                string mensaje;
                // ✅ Usamos _solicitudBL
                bool resultado = _solicitudBL.Actualizar(solicitud, codigoUsuario, out mensaje, esAdmin: true);

                if (resultado)
                {
                    TempData["Success"] = "Solicitud rechazada correctamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = string.IsNullOrEmpty(mensaje) ? "No se pudo rechazar la solicitud" : mensaje;
                    return RedirectToAction("RevisarSolicitud", new { id = solicitudId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Financiero/RegistrarPago/5
        [HttpGet]
        public ActionResult RegistrarPago(int solicitudId)
        {
            try
            {
                // ✅ Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(solicitudId);

                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index", "Solicitud");
                }

                var pagos = _pagoBL.ObtenerPorSolicitud(solicitudId);
                Pago pagoAprobado = null;

                if (pagos != null && pagos.Count > 0)
                {
                    pagoAprobado = pagos.FirstOrDefault(p => p.Estado == "APROBADO");
                }

                if (pagoAprobado != null)
                {
                    TempData["Warning"] = "Ya existe un pago aprobado para esta solicitud";
                    return RedirectToAction("Detalle", "Solicitud", new { id = solicitudId });
                }

                ViewBag.Solicitud = solicitud;

                var montoSugerido = ObtenerMontoSegunTipo(null);

                ViewBag.MontoSugerido = montoSugerido;
                ViewBag.MetodosPago = ObtenerMetodosPago();

                var pago = new Pago
                {
                    CodigoSolicitud = solicitudId,
                    FechaPago = DateTime.Now,
                    Monto = montoSugerido,
                    Estado = "PENDIENTE"
                };

                return View(pago);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        // POST: Financiero/RegistrarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarPago(Pago pago, HttpPostedFileBase comprobanteArchivo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.MetodosPago = ObtenerMetodosPago();
                    return View(pago);
                }

                if (pago.Monto <= 0)
                {
                    ModelState.AddModelError("Monto", "El monto debe ser mayor a cero");
                    ViewBag.MetodosPago = ObtenerMetodosPago();
                    return View(pago);
                }

                if (!string.IsNullOrWhiteSpace(pago.NumeroTransaccion))
                {
                    var existe = _pagoBL.ExistePorNumeroTransaccion(pago.NumeroTransaccion);
                    if (existe)
                    {
                        ModelState.AddModelError("NumeroTransaccion", "Este número de transacción ya está registrado");
                        ViewBag.MetodosPago = ObtenerMetodosPago();
                        return View(pago);
                    }
                }

                if (comprobanteArchivo != null && comprobanteArchivo.ContentLength > 0)
                {
                    var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var extension = System.IO.Path.GetExtension(comprobanteArchivo.FileName).ToLower();

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError("", "Solo se permiten archivos PDF o imágenes (JPG, PNG)");
                        ViewBag.MetodosPago = ObtenerMetodosPago();
                        return View(pago);
                    }

                    if (comprobanteArchivo.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "El archivo no debe superar los 5MB");
                        ViewBag.MetodosPago = ObtenerMetodosPago();
                        return View(pago);
                    }

                    var rutaComprobante = GuardarArchivo(comprobanteArchivo, "comprobantes", $"pago_{pago.CodigoSolicitud}");
                    pago.RutaComprobante = rutaComprobante;
                }
                else
                {
                    ModelState.AddModelError("", "Debe adjuntar el comprobante de pago");
                    ViewBag.MetodosPago = ObtenerMetodosPago();
                    return View(pago);
                }

                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");
                pago.Estado = "PENDIENTE";
                pago.CreatedBy = codigoUsuario;
                pago.CreatedAt = DateTime.Now;

                bool resultado = _pagoBL.Registrar(pago);

                if (resultado)
                {
                    TempData["Success"] = "Pago registrado exitosamente. En espera de validación.";
                    return RedirectToAction("Detalle", "Solicitud", new { id = pago.CodigoSolicitud });
                }
                else
                {
                    TempData["Error"] = "No se pudo registrar el pago";
                    ViewBag.MetodosPago = ObtenerMetodosPago();
                    return View(pago);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                ViewBag.MetodosPago = ObtenerMetodosPago();
                return View(pago);
            }
        }

        // GET: Financiero/ValidarPago/5
        [HttpGet]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult ValidarPago(int id)
        {
            try
            {
                var pago = _pagoBL.ObtenerPorId(id);

                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction("Index");
                }

                if (!string.Equals(pago.Estado, "PENDIENTE", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Warning"] = $"Este pago ya fue validado (Estado: {pago.Estado})";
                }

                // ✅ Usamos _solicitudBL
                var solicitud = _solicitudBL.ObtenerDetalle(pago.CodigoSolicitud);
                ViewBag.Solicitud = solicitud;

                return View(pago);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Financiero/AprobarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult AprobarPago(int id, string observaciones)
        {
            try
            {
                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");
                var pago = _pagoBL.ObtenerPorId(id);

                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction("Index");
                }

                if (!string.Equals(pago.Estado, "PENDIENTE", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Solo se pueden aprobar pagos en estado PENDIENTE";
                    return RedirectToAction("ValidarPago", new { id });
                }

                pago.Estado = "APROBADO";
                pago.FechaValidacion = DateTime.Now;
                pago.UsuarioValidacion = codigoUsuario;
                pago.ObservacionesValidacion = observaciones;
                pago.UpdatedBy = codigoUsuario;
                pago.UpdatedAt = DateTime.Now;

                bool resultado = _pagoBL.Actualizar(pago);

                if (resultado)
                {
                    TempData["Success"] = "Pago aprobado correctamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "No se pudo aprobar el pago";
                    return RedirectToAction("ValidarPago", new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Financiero/RechazarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Financiero,Administrador")]
        public ActionResult RechazarPago(int id, string motivoRechazo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["Error"] = "Debe especificar el motivo del rechazo";
                    return RedirectToAction("ValidarPago", new { id });
                }

                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");
                var pago = _pagoBL.ObtenerPorId(id);

                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction("Index");
                }

                if (!string.Equals(pago.Estado, "PENDIENTE", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Solo se pueden rechazar pagos en estado PENDIENTE";
                    return RedirectToAction("ValidarPago", new { id });
                }

                pago.Estado = "RECHAZADO";
                pago.FechaValidacion = DateTime.Now;
                pago.UsuarioValidacion = codigoUsuario;
                pago.ObservacionesValidacion = motivoRechazo;
                pago.UpdatedBy = codigoUsuario;
                pago.UpdatedAt = DateTime.Now;

                bool resultado = _pagoBL.Actualizar(pago);

                if (resultado)
                {
                    TempData["Success"] = "Pago rechazado correctamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "No se pudo rechazar el pago";
                    return RedirectToAction("ValidarPago", new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Métodos auxiliares
        private SelectList ObtenerMetodosPago()
        {
            var metodos = new[]
            {
                new { Value = "TRANSFERENCIA", Text = "Transferencia Bancaria" },
                new { Value = "DEPOSITO", Text = "Depósito Bancario" },
                new { Value = "TARJETA", Text = "Tarjeta de Crédito/Débito" },
                new { Value = "CHEQUE", Text = "Cheque" },
                new { Value = "EFECTIVO", Text = "Efectivo" }
            };

            return new SelectList(metodos, "Value", "Text");
        }

        private decimal ObtenerMontoSegunTipo(string tipoSolicitud)
        {
            switch (tipoSolicitud?.ToUpper())
            {
                case "CERTIFICADO_OPERADOR_AEREO":
                    return 5000.00m;
                case "RENOVACION_AOC":
                    return 3000.00m;
                case "MODIFICACION_AOC":
                    return 2000.00m;
                default:
                    return 0m;
            }
        }

        private string GuardarArchivo(HttpPostedFileBase archivo, string carpeta, string prefijo)
        {
            try
            {
                var rutaBase = Server.MapPath($"~/uploads/{carpeta}");

                if (!System.IO.Directory.Exists(rutaBase))
                    System.IO.Directory.CreateDirectory(rutaBase);

                var extension = System.IO.Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{prefijo}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var rutaCompleta = System.IO.Path.Combine(rutaBase, nombreArchivo);

                archivo.SaveAs(rutaCompleta);

                return $"/uploads/{carpeta}/{nombreArchivo}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar archivo: {ex.Message}");
            }
        }
    }
}