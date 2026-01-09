namespace CapaPresentacion.Models
{
    public class DashboardViewModel
    {
        public string NombreUsuario { get; set; }
        public string RolUsuario { get; set; }
        public int SolicitudesPendientes { get; set; }
        public int TramitesEnCurso { get; set; }
        public int NotificacionesNuevas { get; set; }
        public bool MostrarModuloOperador { get; set; }
        public bool MostrarModuloFinanciero { get; set; }
        public bool MostrarModuloCertificacion { get; set; }
        public bool MostrarModuloInspector { get; set; }
    }
}
