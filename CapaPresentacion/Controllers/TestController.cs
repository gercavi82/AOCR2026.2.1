// Archivo: CapaPresentacion/Controllers/TestController.cs
using System;
using System.Web.Mvc;
using CapaDatos;

namespace CapaPresentacion.Controllers
{
    public class TestController : Controller
    {
        // GET: Test/ConexionDB
        public ActionResult ConexionDB()
        {
            try
            {
                using (var context = new AOCRContext())
                {
                    // Probar conexión
                    bool exists = context.Database.Exists();

                    if (exists)
                    {
                        // Obtener nombre de la base de datos
                        string dbName = context.Database.Connection.Database;

                        ViewBag.Mensaje = $"✅ Conexión exitosa a la base de datos: {dbName}";
                        ViewBag.Estado = "success";
                    }
                    else
                    {
                        ViewBag.Mensaje = "⚠️ La base de datos no existe";
                        ViewBag.Estado = "warning";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $"❌ Error: {ex.Message}";
                ViewBag.Detalle = ex.StackTrace;
                ViewBag.Estado = "error";
            }

            return View();
        }
    }
}