using System.Collections.Generic;
using System.Web;
using CapaModelo;

namespace CapaPresentacion.Models
{
    public class SolicitudAOCRViewModel
    {
        public SolicitudAOCR Solicitud { get; set; } = new SolicitudAOCR();
        public List<Aeronave> Aeronaves { get; set; } = new List<Aeronave>();

        // Para mostrar archivos ya guardados en la DB
        public List<Documento> DocumentosExistentes { get; set; } = new List<Documento>();

        // Para subir nuevos archivos
        public List<HttpPostedFileBase> ArchivosSubidos { get; set; }

        // Facturación
        public string TipoIdentificacionFactura { get; set; }
        public string NumeroIdentificacionFactura { get; set; }
        public string RazonSocialFactura { get; set; }
        public string FormaPago { get; set; }
        public string Banco { get; set; }
        public string NumeroComprobante { get; set; }
        public string TipoSolicitud { get; set; }

    }
}