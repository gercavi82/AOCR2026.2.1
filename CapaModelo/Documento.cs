using System;

namespace CapaModelo
{
    public class Documento
    {
        public int CodigoDocumento { get; set; }
        public int CodigoSolicitud { get; set; }

        public string TipoDocumento { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaArchivo { get; set; }
        public long? TamanioArchivo { get; set; }
        public string ExtensionArchivo { get; set; }

        public string Estado { get; set; }
        public string Observaciones { get; set; }

        public DateTime FechaSubida { get; set; }

        public string UsuarioRegistro { get; set; }

        // Compatibilidad con campos extra de la tabla
        public string Tipo { get; set; }
        public string HashArchivo { get; set; }
        public bool? Validado { get; set; }
        public DateTime? FechaValidacion { get; set; }
        public string ValidadoPor { get; set; }
        public string Version { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Alias para auditoría
        public string CreatedBy
        {
            get => UsuarioRegistro;
            set => UsuarioRegistro = value;
        }
    }
}
