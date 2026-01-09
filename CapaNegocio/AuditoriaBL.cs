using System;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class AuditoriaBL
    {
        public static bool RegistrarAuditoria(Auditoria auditoria, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (auditoria == null)
                {
                    mensaje = "Los datos de auditoría son requeridos.";
                    return false;
                }

                auditoria.Fecha = DateTime.Now;

                var dao = new AuditoriaDAO();
                dao.Registrar(auditoria);

                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public static bool RegistrarAccion(string entidad, string accion,
            int? codigoUsuario, string datosPrevios = null, string datosNuevos = null)
        {
            try
            {
                var auditoria = new Auditoria
                {
                    Entidad = entidad,
                    Accion = accion,
                    Usuario = codigoUsuario?.ToString(), // conversión explícita para evitar CS0029
                    DatosPrevios = datosPrevios,
                    DatosNuevos = datosNuevos,
                    Fecha = DateTime.Now
                };

                var dao = new AuditoriaDAO();
                dao.Registrar(auditoria);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
