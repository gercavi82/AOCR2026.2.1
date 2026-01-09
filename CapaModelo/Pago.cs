using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaModelo
{
    /// <summary>
    /// Modelo Pago AOCR unificado
    /// Compatible con MVC5 + .NET Framework 4.7.2
    /// Diseñado para funcionar con DAOs manuales (PostgreSQL)
    /// y evitar conflictos de nombres entre capas.
    /// </summary>
    [Table("aocr_tbpago")]
    public class Pago
    {
        // =========================
        // PK
        // =========================
        [Key]
        [Column("codigopago")]
        public int CodigoPago { get; set; }

        // =========================
        // FK Solicitud
        // =========================
        [Required]
        [Column("codigosolicitud")]
        public int CodigoSolicitud { get; set; }

        // =========================
        // Datos principales
        // =========================
        [Required]
        [Column("monto")]
        public decimal Monto { get; set; }

        [MaxLength(30)]
        [Column("metodopago")]
        public string MetodoPago { get; set; }

        // Tu DAO ya lo maneja como nullable
        [Column("fechapago")]
        public DateTime? FechaPago { get; set; }

        [MaxLength(100)]
        [Column("numerotransaccion")]
        public string NumeroTransaccion { get; set; }

        [MaxLength(500)]
        [Column("rutacomprobante")]
        public string RutaComprobante { get; set; }

        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } // PENDIENTE, APROBADO, RECHAZADO, etc.

        // =========================
        // VALIDACIÓN (lo que te falta)
        // =========================
        [Column("fechavalidacion")]
        public DateTime? FechaValidacion { get; set; }

        [Column("usuariovalidacion")]
        public int? UsuarioValidacion { get; set; }

        [Column("observacionesvalidacion", TypeName = "text")]
        public string ObservacionesValidacion { get; set; }

        // =========================
        // Auditoría estándar AOCR
        // =========================
        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("createdby")]
        public int? CreatedBy { get; set; }

        [Column("updatedby")]
        public int? UpdatedBy { get; set; }

        // =========================
        // Soft delete (lo que te falta)
        // =========================
        [Column("deletedat")]
        public DateTime? DeletedAt { get; set; }

        [Column("deletedby")]
        public int? DeletedBy { get; set; }

        // =========================
        // Alias opcionales de compatibilidad
        // (si en algún punto usaste otros nombres)
        // =========================
        [NotMapped]
        public string TipoPago
        {
            get => MetodoPago;
            set => MetodoPago = value;
        }

        [NotMapped]
        public string EstadoPago
        {
            get => Estado;
            set => Estado = value;
        }

        // Navegación opcional
        [ForeignKey("CodigoSolicitud")]
        public virtual SolicitudAOCR Solicitud { get; set; }
    }
}
