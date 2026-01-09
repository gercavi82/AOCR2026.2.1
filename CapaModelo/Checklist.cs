using System;

namespace CapaModelo
{
    public class Checklist
    {
        public int CodigoChecklist { get; set; }

        public int? CodigoInspeccion { get; set; }

        public string Seccion { get; set; }

        // En tu DAO lo estás leyendo como string:
        // ItemNumero = reader["item_numero"]?.ToString()
        public string ItemNumero { get; set; }

        // ✅ FALTABA ESTA PROPIEDAD
        public string Descripcion { get; set; }

        // Tu lógica SQL usa 'Si'/'No'/'N/A'
        public string Cumple { get; set; }

        public string Observaciones { get; set; }

        public string Criticidad { get; set; }

        // Si luego quieres auditoría:
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public int CodigoSolicitud { get; set; }
       
    }
}
