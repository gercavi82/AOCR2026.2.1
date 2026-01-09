namespace CapaPresentacion.Models
{
    public class UsuarioRegistroViewModel
    {
        public string Correo { get; set; }
        public string CedulaIdentificacion { get; set; }
        public string NombreUsuario { get; set; }
        public string ApellidoUsuario { get; set; }
        public int Rol { get; set; } // Representa el codigorol (integer)
        public string Clave { get; set; }
        public string Empresa { get; set; }
      

    }
}