using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CapaModelo;
using CapaNegocio;
using CapaPresentacion.Models;

namespace CapaPresentacion.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            string mensaje;
            Usuario usuario;
            List<string> roles;

            // 1. Intentamos autenticar con la BD
            bool ok = UsuarioBL.Autenticar(
                model.Usuario,
                model.Contrasena,
                out usuario,
                out roles,
                out mensaje
            );

            if (!ok)
            {
                ModelState.AddModelError("", mensaje);
                return View(model);
            }

            // =========================================================================
            // CORRECCIÓN 1: LÓGICA ESPECIAL PARA USU_ADMIN
            // Si entra el admin supremo, ignoramos lo que diga la BD y le damos TODOS los roles
            // =========================================================================
            if (usuario.NombreUsuario.Equals("USU_ADMIN", StringComparison.InvariantCultureIgnoreCase))
            {
                // NOTA: Asegúrate que estos nombres sean IDÉNTICOS a los de tu Base de Datos
                roles = new List<string>
                {
                    "Administrador",
                    "Tecnico",
                    "Solicitante",
                    "Financiero",
                    "Aprobador"
                };
            }
            // =========================================================================


            // Preparamos los roles para la cookie
            string rolesString = (roles != null && roles.Count > 0)
                ? string.Join(",", roles)
                : string.Empty;

            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.NombreUsuario,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                model.Recordarme,
                rolesString,
                FormsAuthentication.FormsCookiePath
            );

            string encrypted = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
            {
                HttpOnly = true
            };

            if (model.Recordarme)
                cookie.Expires = DateTime.Now.AddDays(7);

            Response.Cookies.Add(cookie);

            // =========================================================================
            // CORRECCIÓN 2: GUARDAR LA LISTA EN SESIÓN
            // Sin esta línea, la vista _SeleccionarRol.cshtml siempre recibe null
            // =========================================================================
            Session["CodigoUsuario"] = usuario.Id;
            Session["NombreUsuario"] = usuario.NombreCompleto;
            Session["Correo"] = usuario.Email;

            // Rol activo actual
            Session["Rol"] = (roles != null && roles.Count > 0) ? roles[0] : null;

            // Lista completa para el menú desplegable (¡ESTO FALTABA!)
            Session["TodosLosRoles"] = roles;
            // =========================================================================

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear(); // Borra TodosLosRoles, Rol, Usuario, etc.
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult EnviarRecuperar(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Json(new { ok = false, mensaje = "Debe ingresar un correo electrónico válido." });

            string mensaje;
            bool enviado = UsuarioBL.RestablecerContrasenaPorEmail(email, out mensaje);

            return Json(new { ok = enviado, mensaje });
        }

        public ActionResult _ModalRegistroUsuario()
        {
            var model = new UsuarioCreateViewModel
            {
                RolesDisponibles = RolBL.ObtenerActivos()
            };
            return PartialView("_ModalRegistroUsuario", model);
        }

        [Authorize]
        public ActionResult CambiarRol(string rolSeleccionado)
        {
            // Recuperamos la lista que guardamos en el Login
            var roles = Session["TodosLosRoles"] as List<string>;

            // Verificamos que el usuario realmente tenga permiso para ese rol
            if (roles != null && roles.Contains(rolSeleccionado))
            {
                Session["Rol"] = rolSeleccionado;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}