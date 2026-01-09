using System;
using System.Web.Mvc;
using System.Web.Security;
using CapaPresentacion.Models;

namespace CapaPresentacion.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Verificación de seguridad de sesión
            if (Session["NombreUsuario"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Usuario = Session["NombreUsuario"];
            ViewBag.Rol = Session["Rol"];

            var model = new DashboardViewModel
            {
                NombreUsuario = Session["NombreUsuario"]?.ToString() ?? "Usuario",
                RolUsuario = Session["Rol"]?.ToString() ?? "Invitado",

                // Inicialización de contadores en cero para nuevos usuarios
                SolicitudesPendientes = 0,
                TramitesEnCurso = 0,
                NotificacionesNuevas = 0,

                // Permisos de visibilidad de módulos
                MostrarModuloOperador = true,
                MostrarModuloFinanciero = true,
                MostrarModuloCertificacion = true,
                MostrarModuloInspector = true
            };

            return View(model);
        }

        public ActionResult Salir()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                var cookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login", "Account");
        }
    }
}