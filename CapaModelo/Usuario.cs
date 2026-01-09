using System;

namespace CapaModelo
{
    public class Usuario
    {
        public int Id { get; set; } // Usaremos Id como estándar
        public string NombreUsuario { get; set; }
        public string Email { get; set; }
        public string Contrasena { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaUltimaConexion { get; set; }
        public string CodigoUsuario { get; set; }

    }
}