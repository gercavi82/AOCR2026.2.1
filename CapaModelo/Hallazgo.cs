using System;

namespace CapaModelo
{
    public class Hallazgo
    {
        public int CodigoHallazgo { get; set; }
        public int CodigoInspeccion { get; set; }

        public string Descripcion { get; set; }
        public string Criticidad { get; set; }   // ALTA / MEDIA / BAJA
        public string Estado { get; set; }       // ABIERTO | CERRADO

        public DateTime FechaDeteccion { get; set; }
        public DateTime? FechaCierre { get; set; }

        // AUDITORÍA
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
    }
}
