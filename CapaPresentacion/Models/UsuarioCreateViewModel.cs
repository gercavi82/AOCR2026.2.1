using System.Collections.Generic;
using CapaModelo;

namespace CapaPresentacion.Models
{
    public class UsuarioCreateViewModel
    {
        public Usuario Usuario { get; set; }
        public int RolSeleccionado { get; set; }
        public IEnumerable<Rol> RolesDisponibles { get; set; }
    }
}
