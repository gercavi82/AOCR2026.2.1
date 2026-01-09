using System;

namespace CapaModelo
{
    public class Informe
    {
        public int CodigoInforme { get; set; }

        // ✅ FALTABA
        public int? CodigoInspeccion { get; set; }

        // ✅ FALTABAN CAMPOS DE CONTENIDO
        public string ResumenEjecutivo { get; set; }
        public string Conclusiones { get; set; }
        public string Recomendaciones { get; set; }
        public string Hallazgos { get; set; }
        public string AccionesCorrectivas { get; set; }
        public string Resultado { get; set; }

        // ✅ FALTABA
        public DateTime? FechaEmision { get; set; }

        // ✅ Auditoría estándar (por consistencia con tus DAOs)
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
    }
}
