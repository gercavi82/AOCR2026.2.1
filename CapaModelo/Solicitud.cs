using System;
using System.Collections.Generic;

namespace CapaModelo
{
    /// <summary>
    /// MODELO: Representa una solicitud AOCR
    /// NO tiene acceso a base de datos
    /// </summary>
    public class Solicitud
    {
        // PROPIEDADES (solo datos)
        public int CodigoSolicitud { get; set; }
        public string NumeroSolicitud { get; set; }
        public int CodigoUsuario { get; set; }
        public string TipoSolicitud { get; set; }
        public string Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRevisionFinanciera { get; set; }
        public int? UsuarioRevisionFinanciera { get; set; }
        public string MotivoRechazo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navegación (relaciones)
        public virtual Usuario Usuario { get; set; }
        public virtual List<Pago> Pagos { get; set; }
        public virtual List<Documento> Documentos { get; set; }
    }
}