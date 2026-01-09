using System;

namespace CapaModelo
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string CreadoPor { get; set; }
        public string ActualizadoPor { get; set; }
        public string CodigoRol { get; set; }
        public string Descripcion { get; set; }

    }
}
