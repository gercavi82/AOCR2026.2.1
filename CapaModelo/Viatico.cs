using System;

namespace CapaModelo
{
    /// <summary>
    /// Modelo unificado de Viático
    /// Compatible con:
    /// - CapaNegocio (ViaticoBL)
    /// - CapaDatos (ViaticoDAO)
    /// - ASP.NET MVC5 / .NET Framework 4.7.2
    ///
    /// Incluye propiedades de compatibilidad porque tu DAO
    /// está usando Viatico también como detalle de gastos.
    /// </summary>
    public class Viatico
    {
        // ==========================================================
        // IDENTIDAD / RELACIÓN PRINCIPAL
        // ==========================================================
        public int CodigoViatico { get; set; }
        public int CodigoSolicitud { get; set; }

        /// <summary>
        /// Usuario solicitante / responsable principal.
        /// </summary>
        public int CodigoUsuario { get; set; }

        // ==========================================================
        // DATOS PRINCIPALES DE COMISIÓN
        // ==========================================================
        public string NombreTecnico { get; set; }   // usado por tu ViaticoDAO
        public string Destino { get; set; }
        public string Motivo { get; set; }
        public int? DiasComision { get; set; }

        // ==========================================================
        // FECHAS PRINCIPALES
        // ==========================================================
        public DateTime? FechaSalida { get; set; }
        public DateTime? FechaRetorno { get; set; }

        // ==========================================================
        // MONTOS DESGLOSADOS
        // ==========================================================
        public decimal? MontoTransporte { get; set; }
        public decimal? MontoAlimentacion { get; set; }
        public decimal? MontoHospedaje { get; set; }
        public decimal? OtrosGastos { get; set; }

        public decimal? MontoSolicitado { get; set; }
        public decimal? MontoAprobado { get; set; }
        public decimal? MontoPagado { get; set; }

        // ==========================================================
        // ESTADO / APROBACIÓN
        // ==========================================================
        private string _estado;

        public string Estado
        {
            get => _estado;
            set => _estado = value;
        }

        public DateTime? FechaAprobacion { get; set; }
        public int? UsuarioAprobacion { get; set; }
        public string ObservacionesAprobacion { get; set; }

        // ==========================================================
        // PAGO
        // ==========================================================
        public DateTime? FechaPago { get; set; }
        public string NumeroComprobante { get; set; }
        public string RutaComprobante { get; set; }

        // ==========================================================
        // AUDITORÍA
        // ==========================================================
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // ==========================================================
        // ✅ BLOQUE DE COMPATIBILIDAD PARA TU ViaticoDAO
        // (Tu DAO está armando Viatico como si fuera también
        //  un detalle de gasto asociado a inspección/técnico)
        // ==========================================================

        /// <summary>
        /// Si el viático está asociado a una inspección concreta.
        /// </summary>
        public int? CodigoInspeccion { get; set; }

        /// <summary>
        /// Técnico responsable del gasto/detalle.
        /// (No confundir con CodigoUsuario principal del viático)
        /// </summary>
        public int? CodigoTecnico { get; set; }

        /// <summary>
        /// Concepto del gasto (ej: transporte, alimentación, hospedaje).
        /// </summary>
        public string Concepto { get; set; }

        /// <summary>
        /// Monto del detalle (cuando el DAO usa Viatico como item).
        /// </summary>
        public decimal? Monto { get; set; }

        /// <summary>
        /// Fecha del gasto/detalle.
        /// </summary>
        public DateTime? Fecha { get; set; }

        /// <summary>
        /// Tipo del gasto/detalle.
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Número o referencia de comprobante del detalle.
        /// </summary>
        public string Comprobante { get; set; }

        /// <summary>
        /// Observaciones generales del detalle.
        /// </summary>
        public string Observaciones { get; set; }

        // ==========================================================
        // ALIAS LEGADOS (para no romper otros módulos)
        // ==========================================================

        public DateTime? FechaSolicitud
        {
            get => CreatedAt;
            set => CreatedAt = value;
        }

        public string EstadoViatico
        {
            get => _estado;
            set => _estado = value;
        }

        public DateTime? FechaRegistro
        {
            get => CreatedAt;
            set => CreatedAt = value;
        }

        /// <summary>
        /// Total calculado del desglose principal.
        /// </summary>
        public decimal? MontoTotal
        {
            get
            {
                decimal total = 0m;
                if (MontoTransporte.HasValue) total += MontoTransporte.Value;
                if (MontoAlimentacion.HasValue) total += MontoAlimentacion.Value;
                if (MontoHospedaje.HasValue) total += MontoHospedaje.Value;
                if (OtrosGastos.HasValue) total += OtrosGastos.Value;
                return total;
            }
        }
    }
}
