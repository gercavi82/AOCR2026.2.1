using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaModelo
{
    /// <summary>
    /// Modelo Inspección AOCR
    /// Unificado para MVC5 + .NET Framework 4.7.2
    /// Incluye alias FechaInspeccion para compatibilidad.
    /// </summary>
    [Table("aocr_tbinspeccion")]
    public class Inspeccion
    {
        // =========================
        // PK
        // =========================
        [Key]
        [Column("codigoinspeccion")]
        public int CodigoInspeccion { get; set; }

        // =========================
        // FK Solicitud
        // =========================
        [Required]
        [Column("codigosolicitud")]
        public int CodigoSolicitud { get; set; }

        // =========================
        // Fechas
        // =========================
        [Column("fechaprogramada")]
        public DateTime? FechaProgramada { get; set; }

        [Column("fechareal")]
        public DateTime? FechaReal { get; set; }

        // ---------------------------------------
        // Alias de compatibilidad
        // Si en algún lado usabas "FechaInspeccion",
        // ahora no romperá compilación.
        // ---------------------------------------
        [NotMapped]
        public DateTime? FechaInspeccion
        {
            get => FechaReal ?? FechaProgramada;
            set
            {
                // Si te asignan FechaInspeccion,
                // lo razonable es tratarlo como FechaReal.
                FechaReal = value;
            }
        }

        // =========================
        // Estado / Observaciones
        // =========================
        [MaxLength(30)]
        [Column("estado")]
        public string Estado { get; set; }  // EJ: PROGRAMADA, REALIZADA, ANULADA

        [Column("observaciones", TypeName = "text")]
        public string Observaciones { get; set; }

        // =========================
        // Auditoría
        // =========================
        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deletedat")]
        public DateTime? DeletedAt { get; set; }

        [Column("createdby")]
        public int? CreatedBy { get; set; }

        [Column("updatedby")]
        public int? UpdatedBy { get; set; }

        [Column("deletedby")]
        public int? DeletedBy { get; set; }

        // =========================
        // Navegación (si la usas)
        // =========================
        [ForeignKey("CodigoSolicitud")]
        public virtual SolicitudAOCR Solicitud { get; set; }
    }
}
