using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaModelo;

namespace CapaPresentacion.Controllers
{
    public class CertificadoController : Controller
    {
        private readonly CertificadoBL _bl = new CertificadoBL();

        // ============================================================
        //        MOSTRAR DETALLE DEL CERTIFICADO POR SOLICITUD
        // ============================================================
        public ActionResult Detalle(int solicitudId)
        {
            var certificado = _bl.ObtenerPorSolicitud(solicitudId);

            ViewBag.SolicitudId = solicitudId;

            return View(certificado);
        }

        // ============================================================
        //        GENERAR CERTIFICADO AUTOMÁTICAMENTE
        // ============================================================
        public ActionResult Generar(int solicitudId)
        {
            string usuario = User?.Identity?.Name ?? "Sistema";

            int id = _bl.GenerarCertificado(solicitudId, usuario);

            return RedirectToAction("Detalle", new { solicitudId });
        }

        // ============================================================
        //                   SUBIR PDF DE CERTIFICADO
        // ============================================================
        [HttpPost]
        public ActionResult SubirPDF(int id, int solicitudId, HttpPostedFileBase archivo)
        {
            try
            {
                if (archivo == null || archivo.ContentLength == 0)
                {
                    TempData["Error"] = "Debe seleccionar un archivo PDF.";
                    return RedirectToAction("Detalle", new { solicitudId });
                }

                // Validar extensión
                string extension = Path.GetExtension(archivo.FileName).ToLower();
                if (extension != ".pdf")
                {
                    TempData["Error"] = "Solo se permiten archivos PDF.";
                    return RedirectToAction("Detalle", new { solicitudId });
                }

                // Construcción de ruta segura
                string carpeta = "~/PDF/Certificados/";
                string nombreArchivo = $"{id}.pdf";
                string rutaRelativa = carpeta + nombreArchivo;

                string rutaFisica = Server.MapPath(rutaRelativa);

                // Crear carpeta si no existe
                string carpetaFisica = Path.GetDirectoryName(rutaFisica);
                if (!Directory.Exists(carpetaFisica))
                    Directory.CreateDirectory(carpetaFisica);

                // Guardar archivo
                archivo.SaveAs(rutaFisica);

                // Registrar en BD
                _bl.SubirPDF(id, rutaRelativa);

                TempData["OK"] = "Archivo PDF subido correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al subir PDF: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { solicitudId });
        }

        // ============================================================
        //                   DESCARGAR PDF DE CERTIFICADO
        // ============================================================
        public ActionResult DescargarPDF(int id)
        {
            var cert = _bl.Obtener(id);

            if (cert == null)
                return Content("El certificado no existe.");

            if (string.IsNullOrWhiteSpace(cert.RutaPdf))
                return Content("El archivo PDF no está registrado.");

            string rutaFisica = Server.MapPath(cert.RutaPdf);

            if (!System.IO.File.Exists(rutaFisica))
                return Content("El archivo PDF no se encuentra en el servidor.");

            return File(rutaFisica, "application/pdf", "certificado.pdf");
        }
    }
}
