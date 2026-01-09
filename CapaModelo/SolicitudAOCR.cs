using System;
using System.Collections.Generic;

namespace CapaModelo
{
    public class SolicitudAOCR
    {
        // Identificación y Control
        public int CodigoSolicitud { get; set; }
        public string NumeroSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int TipoSolicitud { get; set; }
        public string Estado { get; set; }

        // Datos del Operador (Coincide con DAO y DB)
        public string NombreOperador { get; set; }
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }

        // Representante y Operación
        public string RepresentanteLegal { get; set; }
        public string CedulaRepresentante { get; set; }
        public string TipoOperacion { get; set; }
        public string DescripcionOperacion { get; set; }

        // ✅ CORRECCIÓN: Nombres exactos solicitados por el DAO (Errores 58, 59, 61, 147, 148, 150)
        public DateTime? FechaInicioOperacion { get; set; }
        public DateTime? FechaFinOperacion { get; set; }
        public string ObservacionesGenerales { get; set; }
        public string Observaciones { get; set; } // Campo adicional si se usa en otras vistas

        // Auditoría y Control
        public int CodigoUsuario { get; set; }
        public int? CodigoTecnico { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        // Propiedades de Ayuda para la Vista
        public string NombreRepresentante { get { return RepresentanteLegal; } set { RepresentanteLegal = value; } }
        public string RucRepresentante { get { return CedulaRepresentante; } set { CedulaRepresentante = value; } }
        public string DireccionEcuador { get { return Direccion; } set { Direccion = value; } }
        public string Banco { get; set; }
        public string NumComp { get; set; }
        public string UsuarioRevisor { get; set; }
        public string ObservacionesInspector { get; set; }
        public DateTime? FechaRevisionInspector { get; set; }


    }
}