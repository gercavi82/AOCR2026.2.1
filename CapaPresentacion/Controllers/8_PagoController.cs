using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaModelo;

namespace CapaPresentacion.Controllers
{
    public class PagoController : Controller
    {
        private readonly PagoBL _bl = new PagoBL();

        // ============================================================
        // DETALLE: lista de pagos por solicitud
        // ============================================================
        public ActionResult Detalle(int solicitudId)
        {
            // En tu BL, ObtenerPorSolicitud ya se usa como lista en FinancieroController
            var pagos = _bl.ObtenerPorSolicitud(solicitudId);

            if (pagos == null)
                pagos = new System.Collections.Generic.List<Pago>();

            ViewBag.SolicitudId = solicitudId;
            return View(pagos);   // <- la vista debería ser @model List<Pago>
        }

        // ============================================================
        // SUBIR COMPROBANTE
        // ============================================================
        [HttpPost]
        public ActionResult SubirComprobante(int id, int solicitudId, HttpPostedFileBase archivo)
        {
            try
            {
                if (archivo != null && archivo.ContentLength > 0)
                {
                    string carpetaVirtual = "/PDF/Pagos/";
                    string nombreArchivo = id + Path.GetExtension(archivo.FileName);
                    string rutaVirtual = carpetaVirtual + nombreArchivo;
                    string rutaFisica = Server.MapPath(rutaVirtual);

                    // Crear carpeta si no existe
                    var carpetaFisica = Path.GetDirectoryName(rutaFisica);
                    if (!Directory.Exists(carpetaFisica))
                    {
                        Directory.CreateDirectory(carpetaFisica);
                    }

                    // Guardar archivo
                    archivo.SaveAs(rutaFisica);

                    // Actualizar pago en BD
                    var pago = _bl.ObtenerPorId(id);
                    if (pago != null)
                    {
                        var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");

                        pago.RutaComprobante = rutaVirtual;
                        pago.UpdatedBy = codigoUsuario;
                        pago.UpdatedAt = DateTime.Now;

                        _bl.Actualizar(pago);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al subir comprobante: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { solicitudId });
        }

        // ============================================================
        // VALIDAR (APROBAR) PAGO
        // ============================================================
        public ActionResult Validar(int id, int solicitudId)
        {
            try
            {
                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");
                var pago = _bl.ObtenerPorId(id);

                if (pago != null)
                {
                    pago.Estado = "APROBADO";
                    pago.FechaValidacion = DateTime.Now;
                    pago.UsuarioValidacion = codigoUsuario;
                    pago.UpdatedBy = codigoUsuario;
                    pago.UpdatedAt = DateTime.Now;
                    // Si tienes campo ObservacionesValidacion puedes dejarlo nulo o texto fijo
                    // pago.ObservacionesValidacion = "Pago aprobado desde módulo de pagos.";

                    _bl.Actualizar(pago);
                    TempData["Success"] = "Pago aprobado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Pago no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al aprobar pago: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { solicitudId });
        }

        // ============================================================
        // RECHAZAR PAGO
        // ============================================================
        public ActionResult Rechazar(int id, int solicitudId, string motivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivo))
                {
                    TempData["Error"] = "Debe indicar el motivo del rechazo.";
                    return RedirectToAction("Detalle", new { solicitudId });
                }

                var codigoUsuario = int.Parse(Session["CodigoUsuario"]?.ToString() ?? "0");
                var pago = _bl.ObtenerPorId(id);

                if (pago != null)
                {
                    pago.Estado = "RECHAZADO";
                    pago.FechaValidacion = DateTime.Now;
                    pago.UsuarioValidacion = codigoUsuario;
                    pago.ObservacionesValidacion = motivo;
                    pago.UpdatedBy = codigoUsuario;
                    pago.UpdatedAt = DateTime.Now;

                    _bl.Actualizar(pago);
                    TempData["Success"] = "Pago rechazado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Pago no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al rechazar pago: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { solicitudId });
        }
    }
}
