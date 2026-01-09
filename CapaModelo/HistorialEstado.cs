using System;

namespace CapaModelo
{
    public class HistorialEstado
    {
        public int CodigoHistorial { get; set; }

        // 👉 Nullable, porque en BL usas .HasValue
        public int? CodigoSolicitud { get; set; }

        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }

        // 👉 Nullable, porque usas FechaCambio.HasValue / .Value
        public DateTime? FechaCambio { get; set; }


        // Usuario que hizo el cambio (opcional)
        public int? CodigoUsuario { get; set; }

        // Comentarios u observaciones del cambio de estado
        public string Observaciones { get; set; }
        public string NombreUsuario { get; set; } // Nuevo


    }
}
