using System;

namespace CapaModelo
{
    public class Financiero
    {
        public int CodigoFinanciero { get; set; }

        public int? CodigoSolicitud { get; set; }

        // Tipo de movimiento: Ingreso / Egreso / Otro
        public string TipoMovimiento { get; set; }

        // Componentes del valor
        public decimal? MontoBase { get; set; }
        public decimal? Impuestos { get; set; }
        public decimal? Descuentos { get; set; }
        public decimal? Recargos { get; set; }

        // Totales y pagos
        public decimal? MontoTotal { get; set; }
        public decimal? MontoPagado { get; set; }
        public string EstadoPago { get; set; } // Pendiente / Pagado / Cancelado / Vencido

        // Fechas
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Auditoría
        public int? CodigoUsuarioRegistro { get; set; }
        public int? CodigoUsuarioActualiza { get; set; }
    }
}
