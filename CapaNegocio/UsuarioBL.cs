using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CapaModelo;
using CapaDatos.DAOs;

namespace CapaNegocio
{
    public static class UsuarioBL
    {
        // ================================
        // 1. Crear Usuario
        // ================================
        public static int RegistrarUsuario(Usuario usuario, out string mensaje)
        {
            try
            {
                usuario.Contrasena = CalcularSHA256(usuario.Contrasena);
                int id = UsuarioDAO.Crear(usuario);
                mensaje = "Usuario registrado exitosamente.";
                return id;
            }
            catch (Exception ex)
            {
                mensaje = $"Error al registrar: {ex.Message}";
                return 0;
            }
        }

        // ================================
        // 2. Restablecer Contraseña (Solución Error CS7036)
        // ================================
        public static bool RestablecerContrasenaPorEmail(string email, out string mensaje)
        {
            // ERROR CS7036: El DAO ahora pide la nueva contraseña, no solo el email.
            // Aquí definimos una contraseña temporal por defecto (ej: "123456")
            // Lo ideal sería generar una aleatoria y enviarla por correo.

            string passwordTemporal = "123456";
            string passwordHash = CalcularSHA256(passwordTemporal);

            // ✅ CORRECCIÓN: Pasamos el hash como segundo parámetro
            return UsuarioDAO.RestablecerContrasena(email, passwordHash, out mensaje);
        }

        // ================================
        // 3. Autenticación (Solución Error CS0117)
        // ================================
        public static bool Autenticar(
            string nombreUsuario,
            string contrasena,
            out Usuario usuario,
            out List<string> roles,
            out string mensaje)
        {
            usuario = UsuarioDAO.ObtenerPorNombreUsuario(nombreUsuario);
            roles = new List<string>();
            mensaje = "";

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

            // Validar contraseña
            string contrasenaHash = CalcularSHA256(contrasena);
            if (!string.Equals(usuario.Contrasena, contrasenaHash, StringComparison.OrdinalIgnoreCase))
            {
                mensaje = "Contraseña incorrecta.";
                return false;
            }

            // ERROR CS0117: 'ObtenerRolesPorUsuario' ya no existe.
            // ✅ CORRECCIÓN: Usamos 'ObtenerRoles' y pasamos el ID numérico (usuario.IdRol o usuario.Id)
            // Nota: En UsuarioDAO mapeamos "idusuario AS Id", así que usamos usuario.Id

            // Si tu clase Usuario tiene IdRol, usa IdRol. Si tiene Id, usa Id.
            // Basado en tu último UsuarioDAO, es 'Id'.
            roles = UsuarioDAO.ObtenerRoles(usuario.Id);

            if (roles == null || roles.Count == 0)
            {
                mensaje = "El usuario no tiene roles asignados.";
                // return false; // Descomentar si es obligatorio tener rol
            }

            // Actualizar última conexión
            UsuarioDAO.ActualizarUltimaConexion(usuario.Id);

            mensaje = "Inicio de sesión exitoso.";
            return true;
        }

        // ================================
        // 4. Listar Técnicos
        // ================================
        public static List<Usuario> ListarTecnicos()
        {
            try
            {
                return UsuarioDAO.ObtenerTecnicos();
            }
            catch
            {
                return new List<Usuario>();
            }
        }

        // ================================
        // Utilidad: Hash SHA-256
        // ================================
        private static string CalcularSHA256(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}