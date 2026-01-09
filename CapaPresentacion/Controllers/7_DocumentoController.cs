using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaPresentacion.Controllers
{
    /// <summary>
    /// Controlador para gestionar documentos de solicitudes AOCR
    /// </summary>
    [Authorize]
    public class DocumentoController : Controller
    {
        private readonly DocumentoDAO _documentoDAO;
        // ✅ CORRECCIÓN: Cambiado de SolicitudDAO a SolicitudAOCRDAO
        private readonly SolicitudAOCRDAO _solicitudDAO;
        private readonly string _rutaDocumentos;

        public DocumentoController()
        {
            _documentoDAO = new DocumentoDAO();
            // ✅ CORRECCIÓN: Instanciamos la clase correcta
            _solicitudDAO = new SolicitudAOCRDAO();

            // Usamos HttpContext.Current para evitar problemas de contexto en el constructor
            if (System.Web.HttpContext.Current != null)
            {
                _rutaDocumentos = System.Web.HttpContext.Current.Server.MapPath("~/Documentos/");

                // Crear directorio si no existe
                if (!Directory.Exists(_rutaDocumentos))
                {
                    Directory.CreateDirectory(_rutaDocumentos);
                }
            }
            else
            {
                // Fallback básico (no debería usarse en ejecución normal)
                _rutaDocumentos = "Documentos";
            }
        }

        // GET: Documento/Lista/5
        public ActionResult Lista(int solicitudId)
        {
            try
            {
                var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index", "Solicitud");
                }

                var documentos = _documentoDAO.ObtenerPorSolicitud(solicitudId) ?? new List<Documento>();

                // Calculamos tamaño total con LINQ
                long tamanioTotal = 0;
                if (documentos.Count > 0)
                {
                    tamanioTotal = documentos.Sum(d => (long)(d.TamanioArchivo));
                }

                var estadisticas = new
                {
                    Total = documentos.Count,
                    Pendientes = documentos.Count(d => d.Estado == "Pendiente"),
                    Aprobados = documentos.Count(d => d.Estado == "Aprobado"),
                    Rechazados = documentos.Count(d => d.Estado == "Rechazado"),
                    TamanioTotal = tamanioTotal
                };

                ViewBag.Solicitud = solicitud;
                ViewBag.Estadisticas = estadisticas;
                ViewBag.SolicitudId = solicitudId;

                return View(documentos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar documentos: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        // GET: Documento/Detalle/5
        public ActionResult Detalle(int id)
        {
            try
            {
                var documento = _documentoDAO.ObtenerPorId(id);

                if (documento == null)
                {
                    TempData["Error"] = "Documento no encontrado";
                    return RedirectToAction("Index", "Solicitud");
                }

                return View(documento);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        // GET: Documento/Subir/5
        public ActionResult Subir(int solicitudId)
        {
            try
            {
                var solicitud = _solicitudDAO.ObtenerPorId(solicitudId);
                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index", "Solicitud");
                }

                ViewBag.Solicitud = solicitud;
                ViewBag.SolicitudId = solicitudId;
                ViewBag.TiposDocumento = ObtenerTiposDocumento();

                var documento = new Documento
                {
                    CodigoSolicitud = solicitudId,
                    FechaSubida = DateTime.Now
                };

                return View(documento);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        // POST: Documento/Subir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Subir(int solicitudId, string tipoDocumento, HttpPostedFileBase archivo, string observaciones)
        {
            try
            {
                // ✅ Validación: solo un borrador por solicitud
                if (tipoDocumento == "BORRADOR_AOCR")
                {
                    var existentes = _documentoDAO.ObtenerPorSolicitud(solicitudId);
                    bool yaExisteBorrador = existentes.Any(d => d.TipoDocumento == "BORRADOR_AOCR");

                    if (yaExisteBorrador)
                    {
                        TempData["Error"] = "Ya existe un borrador de AOCR para esta solicitud. Solo se permite uno.";
                        return RedirectToAction("Lista", new { solicitudId });
                    }
                }

                if (archivo != null && archivo.ContentLength > 0)
                {
                    if (archivo.ContentLength > 10 * 1024 * 1024)
                    {
                        TempData["Error"] = "El archivo no puede superar los 10 MB";
                        return RedirectToAction("Subir", new { solicitudId });
                    }

                    string extension = Path.GetExtension(archivo.FileName).ToLower();
                    var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".zip" };

                    if (Array.IndexOf(extensionesPermitidas, extension) == -1)
                    {
                        TempData["Error"] = "Formato no permitido. Use: PDF, JPG, PNG, DOC, DOCX o ZIP";
                        return RedirectToAction("Subir", new { solicitudId });
                    }

                    string nombreArchivo = $"{solicitudId}_{tipoDocumento}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    string rutaCompleta = Path.Combine(_rutaDocumentos, nombreArchivo);

                    archivo.SaveAs(rutaCompleta);

                    var documento = new Documento
                    {
                        CodigoSolicitud = solicitudId,
                        TipoDocumento = tipoDocumento,
                        NombreArchivo = archivo.FileName,
                        RutaArchivo = rutaCompleta,
                        TamanioArchivo = archivo.ContentLength,
                        ExtensionArchivo = extension,
                        FechaSubida = DateTime.Now,
                        Estado = "Pendiente",
                        Observaciones = observaciones,
                        CreatedBy = User.Identity.Name ?? "SYSTEM"
                    };

                    bool resultado = _documentoDAO.Insertar(documento);

                    if (resultado)
                    {
                        TempData["Exito"] = "Documento subido exitosamente";
                        return RedirectToAction("Lista", new { solicitudId });
                    }
                    else
                    {
                        if (System.IO.File.Exists(rutaCompleta))
                            System.IO.File.Delete(rutaCompleta);

                        TempData["Error"] = "No se pudo registrar el documento en la base de datos";
                    }
                }
                else
                {
                    TempData["Error"] = "Debe seleccionar un archivo";
                }

                return RedirectToAction("Subir", new { solicitudId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al subir documento: {ex.Message}";
                return RedirectToAction("Subir", new { solicitudId });
            }
        }


        // GET: Documento/Descargar/5
        public ActionResult Descargar(int id)
        {
            try
            {
                var documento = _documentoDAO.ObtenerPorId(id);

                if (documento == null)
                {
                    TempData["Error"] = "Documento no encontrado";
                    return RedirectToAction("Index", "Solicitud");
                }

                if (!System.IO.File.Exists(documento.RutaArchivo))
                {
                    TempData["Error"] = "Archivo físico no encontrado en el servidor";
                    return RedirectToAction("Lista", new { solicitudId = documento.CodigoSolicitud });
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(documento.RutaArchivo);
                string contentType = ObtenerContentType(documento.ExtensionArchivo);

                return File(fileBytes, contentType, documento.NombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al descargar documento: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        // GET: Documento/Editar/5
        [Authorize(Roles = "Administrador,Coordinador")]
        public ActionResult Editar(int id)
        {
            try
            {
                var documento = _documentoDAO.ObtenerPorId(id);

                if (documento == null)
                {
                    TempData["Error"] = "Documento no encontrado";
                    return RedirectToAction("Index", "Solicitud");
                }

                ViewBag.TiposDocumento = ObtenerTiposDocumento();
                ViewBag.Estados = ObtenerEstadosDocumento();

                return View(documento);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }


        // POST: Documento/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Coordinador")]
        public ActionResult Editar(Documento documento)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool resultado = _documentoDAO.Actualizar(documento);

                    if (resultado)
                    {
                        TempData["Exito"] = "Documento actualizado correctamente";
                        return RedirectToAction("Lista", new { solicitudId = documento.CodigoSolicitud });
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo actualizar el documento";
                    }
                }

                ViewBag.TiposDocumento = ObtenerTiposDocumento();
                ViewBag.Estados = ObtenerEstadosDocumento();
                return View(documento);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                ViewBag.TiposDocumento = ObtenerTiposDocumento();
                ViewBag.Estados = ObtenerEstadosDocumento();
                return View(documento);
            }
        }

        // POST: Documento/CambiarEstado
        [HttpPost]
        [Authorize(Roles = "Administrador,Coordinador,Revisor")]
        public JsonResult CambiarEstado(int id, string estado, string observaciones)
        {
            try
            {
                var documento = _documentoDAO.ObtenerPorId(id);
                if (documento == null)
                {
                    return Json(new
                    {
                        success = false,
                        mensaje = "Documento no encontrado"
                    });
                }

                documento.Estado = estado;
                documento.Observaciones = observaciones;

                var resultado = _documentoDAO.Actualizar(documento);

                return Json(new
                {
                    success = resultado,
                    mensaje = resultado ? "Estado actualizado correctamente" : "No se pudo actualizar el estado"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = $"Error: {ex.Message}" });
            }
        }

        // POST: Documento/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult Eliminar(int id, string motivo)
        {
            try
            {
                var documento = _documentoDAO.ObtenerPorId(id);

                if (documento == null)
                {
                    TempData["Error"] = "Documento no encontrado";
                    return RedirectToAction("Index", "Solicitud");
                }

                bool resultado = _documentoDAO.Eliminar(id);

                if (resultado)
                {
                    // Eliminar archivo físico
                    if (System.IO.File.Exists(documento.RutaArchivo))
                    {
                        System.IO.File.Delete(documento.RutaArchivo);
                    }

                    TempData["Exito"] = "Documento eliminado correctamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el documento";
                }

                return RedirectToAction("Lista", new { solicitudId = documento.CodigoSolicitud });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        // GET: Documento/Estadisticas
        [Authorize(Roles = "Administrador,Coordinador")]
        public ActionResult Estadisticas()
        {
            try
            {
                var documentos = _documentoDAO.ObtenerTodos() ?? new List<Documento>();

                var estadisticas = new
                {
                    Total = documentos.Count,
                    Pendientes = documentos.Count(d => d.Estado == "Pendiente"),
                    Aprobados = documentos.Count(d => d.Estado == "Aprobado"),
                    Rechazados = documentos.Count(d => d.Estado == "Rechazado"),
                    TamanioTotal = documentos.Sum(d => (long)(d.TamanioArchivo))
                };

                var estadisticasPorTipo = documentos
                    .GroupBy(d => d.TipoDocumento)
                    .Select(g => new
                    {
                        TipoDocumento = g.Key,
                        Total = g.Count(),
                        TamanioTotal = g.Sum(d => (long)(d.TamanioArchivo))
                    })
                    .ToList();

                ViewBag.EstadisticasPorTipo = estadisticasPorTipo;

                return View(estadisticas);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return View();
            }
        }

        // GET: Documento/Buscar
        public ActionResult Buscar(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    TempData["Warning"] = "Ingrese un término de búsqueda";
                    return RedirectToAction("Index", "Solicitud");
                }

                var documentos = _documentoDAO.ObtenerTodos() ?? new List<Documento>();

                termino = termino.Trim();
                var resultados = documentos
                    .Where(d =>
                        (!string.IsNullOrEmpty(d.NombreArchivo) &&
                         d.NombreArchivo.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(d.TipoDocumento) &&
                         d.TipoDocumento.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(d.Observaciones) &&
                         d.Observaciones.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0)
                    )
                    .ToList();

                ViewBag.Termino = termino;

                // Reutilizamos la vista "Lista" pero sin solicitud específica
                return View("Lista", resultados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error en la búsqueda: {ex.Message}";
                return RedirectToAction("Index", "Solicitud");
            }
        }

        #region Métodos Auxiliares

        private SelectList ObtenerTiposDocumento()
        {
            var tipos = new[]
            {
        new { Value = "BORRADOR_AOCR", Text = "Borrador de Certificado AOCR" },
        // …otros tipos existentes
        new { Value = "Otro", Text = "Otro Documento" }
    };

            return new SelectList(tipos, "Value", "Text");
        }


        private SelectList ObtenerEstadosDocumento()
        {
            var estados = new[]
            {
                new { Value = "Pendiente", Text = "Pendiente de Revisión" },
                new { Value = "Aprobado", Text = "Aprobado" },
                new { Value = "Rechazado", Text = "Rechazado" },
                new { Value = "Observado", Text = "Con Observaciones" }
            };

            return new SelectList(estados, "Value", "Text");
        }

        private string ObtenerContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".zip":
                    return "application/zip";
                default:
                    return "application/octet-stream";
            }
        }

        #endregion
    }
}