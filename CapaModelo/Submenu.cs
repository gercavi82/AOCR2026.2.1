using System;

namespace CapaModelo
{
    public class Submenu
    {
        public int IdSubmenu { get; set; }

        public int? IdMenu { get; set; }

        public string NombreSubmenu { get; set; }
        public string Icono { get; set; }
        public string Url { get; set; }

        public int? Orden { get; set; }

        public bool? Activo { get; set; }

        // Campos auxiliares de joins/UI
        public string NombreMenu { get; set; }
    }
}
