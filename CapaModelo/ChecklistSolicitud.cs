namespace CapaModelo
{
    public class ChecklistSolicitud
    {
        public int CodigoChecklistSolicitud { get; set; }
        public int CodigoSolicitud { get; set; }
        public int CodigoItem { get; set; }
        public bool? Cumple { get; set; }
        public string Observacion { get; set; }
        public System.DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; }

        // Para vistas
        public string DescripcionItem { get; set; }
        public int Orden { get; set; }
    }
}
