using System;

namespace CapaModelo
{
    public class Sesion
    {
        // =========================
        // Identificadores
        // =========================
        public int CodigoSesion { get; set; }
        public int CodigoUsuario { get; set; }

        // =========================
        // Sesión / seguridad
        // =========================
        public string Token { get; set; }
        public bool? Activa { get; set; }

        // =========================
        // Campos usados por SesionDAO ✅
        // =========================
        public DateTime? FechaInicio { get; set; }      // ✅ NUEVO
        public DateTime? FechaExpiracion { get; set; }
        public DateTime? UltimaActividad { get; set; }
        public DateTime? FechaCierre { get; set; }

        public string IpAddress { get; set; }
        public string UserAgent { get; set; }          // ✅ NUEVO

        // =========================
        // Campos de apoyo por JOIN ✅
        // =========================
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }

        // =========================
        // Auditoría opcional
        // =========================
        public DateTime? FechaRegistro { get; set; }
    }
}
