using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Principal;
using System.Linq;
using Dapper;
using CapaDatos.DAOs;

namespace CapaPresentacion
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // 1. Verificar si hay una identidad autenticada
            if (HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
                return;

            if (HttpContext.Current.User.Identity is FormsIdentity identity)
            {
                try
                {
                    // 2. Leer el ticket y obtener roles del campo UserData (Método optimizado)
                    FormsAuthenticationTicket ticket = identity.Ticket;
                    string userData = ticket.UserData;

                    string[] roles;

                    if (!string.IsNullOrEmpty(userData))
                    {
                        // Si el ticket ya tiene los roles (vienen del AccountController)
                        roles = userData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        // Si por alguna razón el ticket está vacío, los buscamos en la DB
                        roles = GetRolesFromDB(identity.Name);
                    }

                    // 3. Crear el Principal con los roles inyectados
                    HttpContext.Current.User = new GenericPrincipal(identity, roles);
                }
                catch (Exception)
                {
                    // En caso de error, asignar principal sin roles para evitar caídas
                    HttpContext.Current.User = new GenericPrincipal(identity, new string[] { });
                }
            }
        }

        // Método movido DENTRO de la clase MvcApplication
        private string[] GetRolesFromDB(string username)
        {
            try
            {
                using (var cn = ConexionDAO.CrearConexion())
                {
                    cn.Open();
                    // SQL CORREGIDO para PostgreSQL: Casteo ::text para evitar errores de tipo
                    string sql = @"
                        SELECT r.descripcion
                        FROM usuario u
                        INNER JOIN usuario_rol ur ON u.codigousuario::text = ur.codigousuario::text
                        INNER JOIN rol r ON r.codigorol = ur.codigorol
                        WHERE (LOWER(u.nombreusuario) = LOWER(@user) OR LOWER(u.correo) = LOWER(@user))
                          AND ur.activo = true 
                          AND r.activo = true;";

                    return cn.Query<string>(sql, new { user = username }).ToArray();
                }
            }
            catch
            {
                return new string[] { };
            }
        }
    }
}