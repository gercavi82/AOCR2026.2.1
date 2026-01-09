using System;

namespace CapaModelo
{
    public class Observacion
    {
        // Clave primaria
        public int CodigoObservacion { get; set; }

        // Relación con Inspección
        public int? CodigoInspeccion { get; set; }

        // Datos principales
        public string Descripcion { get; set; }      // Texto de la observación
        public string Gravedad { get; set; }         // Alta / Media / Baja

        // 👉 Campos que reclama ObservacionBL
        public string Estado { get; set; }           // Pendiente / En Proceso / Resuelta / Cerrada
        public DateTime? FechaObservacion { get; set; }
        public DateTime? FechaResolucion { get; set; }

        /// <summary>
        /// Campo de notas / seguimiento / solución.
        /// ObservacionBL lo usa para ir concatenando texto:
        ///   observacion.Observaciones = (observacion.Observaciones ?? "") + "..."
        /// </summary>
        public string Observaciones { get; set; }

        // (Opcionales) Auditoría
        public int? CodigoUsuarioRegistro { get; set; }
        public int? CodigoUsuarioResolvio { get; set; }
    }
}
