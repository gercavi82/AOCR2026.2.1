using System;

namespace CapaModelo
{
    public class Notificacion
    {
        public int CodigoNotificacion { get; set; }
        public int CodigoUsuario { get; set; } // yo lo dejaría obligatorio
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Url { get; set; }
        public bool Leida { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }

    }
}
