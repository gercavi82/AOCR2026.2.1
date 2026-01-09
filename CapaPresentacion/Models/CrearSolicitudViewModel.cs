using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace CapaPresentacion.Models
{
    public class CrearSolicitudViewModel
    {
        // =========================
        // DATOS USUARIO / EMPRESA
        // =========================

        [Display(Name = "RUC")]
        public string Ruc { get; set; }

        [Display(Name = "Compañía")]
        public string Compania { get; set; }

        // =========================
        // DATOS DE SOLICITUD
        // =========================

        [Required(ErrorMessage = "Debe seleccionar el tipo de solicitud")]
        [Display(Name = "Tipo de Solicitud")]
        public string TipoSolicitud { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de operación")]
        [Display(Name = "Tipo de Operación")]
        public string TipoOperacion { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Inicio de Operación")]
        public DateTime FechaInicioOperacion { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Fin de Operación")]
        public DateTime FechaFinOperacion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }

        // =========================
        // COMBOS (SELECTS)
        // =========================

        public IEnumerable<SelectListItem> TiposSolicitudLista { get; set; }
        public IEnumerable<SelectListItem> TiposOperacionLista { get; set; }

        // =========================
        // AERONAVES (SI APLICA)
        // =========================

        public List<AeronaveViewModel> Aeronaves { get; set; }

        // =========================
        // ARCHIVOS ADJUNTOS
        // =========================

        public List<HttpPostedFileBase> ArchivosAdjuntos { get; set; }

        // =========================
        // CONSTRUCTOR
        // =========================

        public CrearSolicitudViewModel()
        {
            FechaInicioOperacion = DateTime.Today;
            FechaFinOperacion = DateTime.Today;
            Aeronaves = new List<AeronaveViewModel>();
            ArchivosAdjuntos = new List<HttpPostedFileBase>();
        }
    }

    // ⚠️ SOLO SI NO EXISTE EN OTRO ARCHIVO
    public class AeronaveViewModel
    {
        public string Fabricante { get; set; }
        public string Modelo { get; set; }
        public string Matricula { get; set; }
        public string Configuracion { get; set; }
        public string Ruido { get; set; }
        public decimal Peso { get; set; }
    }
}
