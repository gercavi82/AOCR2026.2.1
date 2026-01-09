using System;

namespace CapaModelo
{
    public class DocumentoHabilitante
    {
        public int IdDocumento { get; set; }
        public int CodigoSolicitud { get; set; }
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}
