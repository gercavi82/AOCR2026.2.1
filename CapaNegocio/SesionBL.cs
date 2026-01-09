using System;
using CapaModelo;
using CapaDatos.DAOs;
using CapaUtilidades;

namespace CapaNegocio
{
    public class SesionBL
    {
        public static bool IniciarSesion(
            string nombreUsuario,
            string contrasena,
            string ipAddress,
            out string mensaje,
            out Sesion sesion)
        {
            mensaje = string.Empty;
            sesion = null;

            try
            {
                // 1. Obtener usuario
                Usuario usuario = UsuarioDAO.ObtenerPorNombreUsuario(nombreUsuario);

                if (usuario == null)
                {
                    mensaje = "Usuario no encontrado.";
                    return false;
                }

                if (!usuario.Activo)
                {
                    mensaje = "Usuario inactivo.";
                    return false;
                }

                // 2. ✅ Verificación con bcrypt
                if (!Seguridad.VerificarBCrypt(contrasena, usuario.Contrasena))
                {
                    mensaje = "Contraseña incorrecta.";
                    return false;
                }

                // 3. Crear sesión
                sesion = new Sesion
                {
                    CodigoUsuario = usuario.Id,
                    FechaInicio = DateTime.Now,
                    IpAddress = ipAddress,
                    Activa = true,
                    Token = Guid.NewGuid().ToString()
                };

                // 4. Registrar sesión y última conexión
                SesionDAO.Insertar(sesion);
                UsuarioDAO.ActualizarUltimaConexion(usuario.Id);

                mensaje = "Éxito";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error en el servidor: " + ex.Message;
                return false;
            }
        }
    }
}
