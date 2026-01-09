using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;
using System.Linq;
using CapaDatos.DAOs;
using CapaModelo;
using CapaPresentacion.Models;
using CapaNegocio;
using CapaNegocio.Helpers;

namespace CapaPresentacion.Controllers
{
    [Authorize]
    public class SolicitudAOCRController : Controller
    {
        private readonly SolicitudBL _solicitudBL = new SolicitudBL();
        private readonly SolicitudAOCRDAO _solicitudDAO = new SolicitudAOCRDAO();
        private readonly DocumentoDAO _documentoDAO = new DocumentoDAO();

        public ActionResult Index() => View();

        public ActionResult FormularioEmisionAOCR(int? oid)
        {
            var vm = new SolicitudAOCRViewModel();

            if (oid.HasValue && oid > 0)
            {
                vm.Solicitud = _solicitudBL.ObtenerDetalle(oid.Value);
                if (vm.Solicitud == null)
                    return Content("<div class='alert alert-danger'>Error: Solicitud no encontrada.</div>");

                vm.Aeronaves = AeronaveDAO.ObtenerPorSolicitud(oid.Value);
                vm.DocumentosExistentes = _documentoDAO.ObtenerPorSolicitud(oid.Value);

                vm.Banco = vm.Solicitud.Banco;
                vm.NumeroComprobante = vm.Solicitud.NumComp;
            }

            return PartialView("_FormularioEmisionAOCR", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FormularioCompleto(SolicitudAOCRViewModel vm)
        {
            try
            {
                if (Session["CodigoUsuario"] == null)
                    return Json(new { success = false, mensaje = "Sesión expirada." });

                int usuarioId = Convert.ToInt32(Session["CodigoUsuario"]);
                string mensajeOut;

                if (vm.Solicitud == null)
                    return Json(new { success = false, mensaje = "Datos de solicitud incompletos." });

                // Asignar campos requeridos manualmente
                vm.Solicitud.TipoSolicitud = 1;
                vm.Solicitud.Banco = vm.Banco;
                vm.Solicitud.NumComp = vm.NumeroComprobante;

                if (string.IsNullOrWhiteSpace(vm.Solicitud.NombreOperador))
                    return Json(new { success = false, mensaje = "Nombre del operador es obligatorio." });

                bool exito;
                if (vm.Solicitud.CodigoSolicitud > 0)
                {
                    exito = _solicitudBL.Actualizar(vm.Solicitud, usuarioId, out mensajeOut, true);
                }
                else
                {
                    exito = _solicitudBL.Crear(vm.Solicitud, usuarioId, out mensajeOut);
                }

                if (!exito)
                    return Json(new { success = false, mensaje = mensajeOut });

                int idFinal = vm.Solicitud.CodigoSolicitud;

                if (vm.Aeronaves != null && vm.Aeronaves.Any())
                {
                    foreach (var nave in vm.Aeronaves)
                    {
                        nave.CodigoSolicitud = idFinal;
                        AeronaveDAO.Insertar(nave);
                    }
                }

                ProcesarArchivos(vm.ArchivosSubidos, idFinal);

                return Json(new { success = true, mensaje = "Solicitud AOCR registrada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error crítico: " + ex.Message });
            }
        }

        private void ProcesarArchivos(IEnumerable<HttpPostedFileBase> archivos, int solicitudId)
        {
            if (archivos == null) return;
            string path = Server.MapPath("~/Uploads/AOCR/" + solicitudId);
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            foreach (var file in archivos)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(file.FileName);
                    file.SaveAs(System.IO.Path.Combine(path, fileName));

                    _documentoDAO.Crear(new Documento
                    {
                        CodigoSolicitud = solicitudId,
                        NombreArchivo = fileName,
                        RutaArchivo = "/Uploads/AOCR/" + solicitudId + "/" + fileName,
                        FechaSubida = DateTime.Now,
                        Estado = "PENDIENTE"
                    });
                }
            }
        }

        public ActionResult MisSolicitudes()
        {
            if (Session["CodigoUsuario"] == null)
                return RedirectToAction("Login", "Account");

            return View(_solicitudDAO.ObtenerPorUsuario(Convert.ToInt32(Session["CodigoUsuario"])));
        }
        public ActionResult RevisarSolicitudes()
        {
            var pendientes = _solicitudDAO.ObtenerPendientesRevision();
            return View("RevisarSolicitudes", pendientes);
        }

        [HttpPost]
        [Authorize(Roles = "Inspector")]
        [ValidateAntiForgeryToken]
        public ActionResult Aprobar(string id)
        {
            var solicitud = _solicitudDAO.ObtenerPorCodigo(id);

            if (solicitud == null)
                return HttpNotFound();

            solicitud.Estado = "APROBADO_POR_INSPECTOR";
            solicitud.FechaRevisionInspector = DateTime.Now;
            solicitud.UsuarioRevisor = Session["Correo"]?.ToString();

            _solicitudDAO.Actualizar(solicitud);

            TempData["NotificacionTipo"] = "success";
            TempData["NotificacionMensaje"] = "Solicitud aprobada correctamente.";

            return RedirectToAction("RevisarSolicitudes");
        }

        [HttpPost]
        [Authorize(Roles = "Inspector")]
        [ValidateAntiForgeryToken]
        public ActionResult Observar(string id, string observacion)
        {
            var solicitud = _solicitudDAO.ObtenerPorCodigo(id);

            if (solicitud == null)
                return HttpNotFound();

            solicitud.Estado = "OBSERVADO";
            solicitud.ObservacionesInspector = observacion;
            solicitud.FechaRevisionInspector = DateTime.Now;
            solicitud.UsuarioRevisor = Session["Correo"]?.ToString();

            _solicitudDAO.Actualizar(solicitud);

            TempData["NotificacionTipo"] = "warning";
            TempData["NotificacionMensaje"] = "Solicitud marcada como observada.";

            return RedirectToAction("RevisarSolicitudes");

            // Enviar notificación por correo
            EmailHelper.EnviarEmail(
                solicitud.Email,
                "Observación a su Solicitud AOCR",
                $"Estimado operador,<br><br>Su solicitud <strong>#{solicitud.CodigoSolicitud}</strong> ha sido <b>observada</b>.<br><br><b>Observación:</b> {observacion}<br><br>Por favor revise y actualice su información.<br><br>Saludos."
            );

            return RedirectToAction("RevisarSolicitudes");
        }

        [Authorize(Roles = "JefaturaTecnica")]
        public ActionResult RevisarPorJefatura()
        {
            var pendientes = _solicitudDAO.ObtenerPorEstado("ENVIADO_A_JEFATURA");
            return View(pendientes);
        }

        [HttpPost]
        [Authorize(Roles = "JefaturaTecnica")]
        [ValidateAntiForgeryToken]
        public ActionResult AprobarPorJefatura(int id)
        {
            int userId = Convert.ToInt32(Session["CodigoUsuario"]);
            _solicitudDAO.CambiarEstado(id, "VALIDADO_TECNICAMENTE", userId);
            TempData["Exito"] = "La solicitud ha sido validada técnicamente.";
            return RedirectToAction("RevisarPorJefatura");
        }

        [HttpPost]
        [Authorize(Roles = "JefaturaTecnica")]
        [ValidateAntiForgeryToken]
        public ActionResult ObservarPorJefatura(int id, string observaciones)
        {
            int userId = Convert.ToInt32(Session["CodigoUsuario"]);
            _solicitudDAO.CambiarEstado(id, "OBSERVADO_JEFATURA", userId, observaciones);
            TempData["Exito"] = "Se ha enviado una observación a la solicitud.";
            return RedirectToAction("RevisarPorJefatura");
        }

        [Authorize(Roles = "JefaturaTecnica")]
        public ActionResult PendientesJefatura()
        {
            try
            {
                var solicitudes = _solicitudDAO.ObtenerPorEstado("ENVIADO_A_JEFATURA");
                return View(solicitudes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar solicitudes: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [Authorize(Roles = "JefaturaTecnica")]
        public ActionResult ValidarTecnicamente(int id)
        {
            try
            {
                _solicitudDAO.CambiarEstado(id, "LEGALIZACION", ObtenerUsuarioActualId(), "Validación técnica aprobada");
                TempData["Exito"] = "La solicitud fue validada técnicamente.";

                var solicitud = _solicitudDAO.ObtenerPorId(id);
                EmailHelper.EnviarEmail(
                    solicitud.Email,
                    "Su solicitud AOCR fue validada técnicamente",
                    $"Estimado operador,<br><br>Su solicitud AOCR <strong>#{solicitud.CodigoSolicitud}</strong> ha sido <b>validada técnicamente</b> por la Jefatura Técnica.<br><br>Ahora continuará con el proceso de legalización.<br><br>Saludos cordiales."
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al validar: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { id });
        }


        [HttpPost]
        [Authorize(Roles = "JefaturaTecnica")]
        public ActionResult RechazarTecnicamente(int id, string observacion)
        {
            try
            {
                _solicitudDAO.CambiarEstado(id, "RECHAZADA_POR_JEFATURA", ObtenerUsuarioActualId(), observacion);
                TempData["Error"] = "La solicitud fue rechazada por Jefatura Técnica.";

                var solicitud = _solicitudDAO.ObtenerPorId(id);
                EmailHelper.EnviarEmail(
                    solicitud.Email,
                    "Solicitud AOCR rechazada por Jefatura Técnica",
                    $"Estimado operador,<br><br>Su solicitud AOCR <strong>#{solicitud.CodigoSolicitud}</strong> ha sido <b>rechazada</b> por la Jefatura Técnica.<br><br><b>Motivo:</b> {observacion}<br><br>Le invitamos a revisar la información y volver a presentar la solicitud.<br><br>Saludos."
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al rechazar: " + ex.Message;
            }

            return RedirectToAction("Detalle", new { id });
        }
        private int ObtenerUsuarioActualId()
        {
            if (Session["CodigoUsuario"] != null && int.TryParse(Session["CodigoUsuario"].ToString(), out int idUsuario))
            {
                return idUsuario;
            }

            throw new Exception("No se pudo obtener el ID del usuario actual.");
        }
        public ActionResult Detalle(int id)
        {
            var solicitud = _solicitudDAO.ObtenerPorId(id);
            if (solicitud == null)
                return HttpNotFound();

            var historialDAO = new HistorialEstadoDAO();
            ViewBag.HistorialEstados = historialDAO.ObtenerPorSolicitud(id);

            return View(solicitud);
        }
        [Authorize(Roles = "CoordinacionLegal")]
        public ActionResult RevisarLegalizacion()
        {
            var lista = _solicitudDAO.ObtenerPorEstado("ENVIADO_A_LEGALIZACION");
            return View(lista);
        }

        [HttpPost]
        [Authorize(Roles = "CoordinacionLegal")]
        [ValidateAntiForgeryToken]
        public ActionResult Legalizar(int id)
        {
            try
            {
                int userId = ObtenerUsuarioActualId();
                var solicitud = _solicitudDAO.ObtenerPorId(id);

                string estadoAnterior = solicitud.Estado;
                solicitud.Estado = "LEGALIZADO";
                _solicitudDAO.Actualizar(solicitud);

                // Registrar en historial
                new HistorialEstadoDAO().RegistrarCambio(
                    id,
                    estadoAnterior,
                    "LEGALIZADO",
                    userId,
                    "Legalizado por Coordinación Legal"
                );

                // Notificar al operador
                EmailHelper.EnviarEmail(
                    solicitud.Email,
                    "AOCR Legalizado",
                    $"Estimado operador,<br><br>Su solicitud AOCR #{id} ha sido <strong>legalizada</strong> por Coordinación Legal.<br><br>Gracias por su gestión."
                );

                TempData["Exito"] = "Solicitud legalizada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al legalizar: " + ex.Message;
            }

            return RedirectToAction("RevisarLegalizacion");
        }
        [HttpPost]
        [Authorize(Roles = "CoordinacionLegal")]
        [ValidateAntiForgeryToken]
        public ActionResult Legalizar(int id, string observacionLegal)
        {
            try
            {
                int userId = Convert.ToInt32(Session["CodigoUsuario"]);

                // Registrar cambio de estado
                _solicitudDAO.CambiarEstado(id, "LEGALIZADO", userId, observacionLegal);

                // Registrar en historial si quieres duplicar
                new HistorialEstadoDAO().RegistrarCambio(
                    id,
                    "ENVIADO_A_LEGALIZACION",
                    "LEGALIZADO",
                    userId,
                    observacionLegal
                );

                // Obtener solicitud para correo
                var solicitud = _solicitudDAO.ObtenerPorId(id);

                // Enviar notificación por correo al operador
                if (!string.IsNullOrWhiteSpace(solicitud.Email))
                {
                    EmailHelper.EnviarEmail(
                        solicitud.Email,
                        "Su AOCR ha sido legalizado",
                        $@"
                <p>Estimado operador,</p>
                <p>Su solicitud AOCR <strong>#{solicitud.CodigoSolicitud}</strong> ha sido <b>legalizada</b> con éxito.</p>
                <p>Puede ingresar al sistema para descargar su certificado final.</p>
                <p><strong>Observaciones de legalización:</strong><br/>{observacionLegal}</p>
                <p>Saludos.</p>"
                    );
                }

                TempData["Exito"] = "Solicitud legalizada y notificación enviada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al legalizar: " + ex.Message;
            }

            return RedirectToAction("RevisarLegalizacion");
        }
        [Authorize(Roles = "Inspector,Administrador")]
        public ActionResult SolicitarInspeccion(int id)
        {
            var solicitud = SolicitudAOCRBL.ObtenerPorId(id);
            if (solicitud == null)
                return HttpNotFound();

            bool ok = SolicitudAOCRBL.MarcarParaInspeccion(id);
            if (ok)
                TempData["NotificacionMensaje"] = "Inspección solicitada correctamente.";
            else
                TempData["NotificacionMensaje"] = "Error al solicitar inspección.";

            return RedirectToAction("Detalle", new { id });
        }


    }
}
