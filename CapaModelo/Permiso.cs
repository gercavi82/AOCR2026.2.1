using System;

namespace CapaModelo
{
    public class Permiso
    {
        // PK
        public int IdPermiso { get; set; }

        // Relación con Rol
        public int CodigoRol { get; set; }

        // Suele existir relación con menú/submenú
        // (déjalos nullable por si tu diseño permite permisos por menú o por submenú)
        public int? IdMenu { get; set; }
        public int? IdSubmenu { get; set; }

        // Campos de presentación que muchos DAOs cargan por JOIN
        public string NombreMenu { get; set; }
        public string NombreSubmenu { get; set; }
        public string Url { get; set; }

        // Opcionales útiles si tu tabla los maneja
        public bool? Activo { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}
