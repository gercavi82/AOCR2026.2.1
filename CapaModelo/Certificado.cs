using System;

namespace CapaModelo
{
    public class Certificado
    {
        public int CodigoCertificado { get; set; }
        public int CodigoSolicitud { get; set; }
        public string NumeroCertificado { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int? VigenciaAnios { get; set; }
        public string Estado { get; set; }
        public string CondicionesEspeciales { get; set; }
        public string FirmadoPor { get; set; }
        public string RutaPdf { get; set; }
        public string CodigoVerificacion { get; set; }
    }
}