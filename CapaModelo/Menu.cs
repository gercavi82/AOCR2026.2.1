using System;

namespace CapaModelo
{
    public class Menu
    {
        public int IdMenu { get; set; }

        public string NombreMenu { get; set; }
        public string Icono { get; set; }
        public string Url { get; set; }

        public int? Orden { get; set; }
        public bool? Activo { get; set; }

        public DateTime? FechaRegistro { get; set; }
    }
}
