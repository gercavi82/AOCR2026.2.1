using System.ComponentModel.DataAnnotations;

namespace CapaPresentacion.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario o correo es obligatorio")]
        [Display(Name = "Usuario o Correo")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }
}
