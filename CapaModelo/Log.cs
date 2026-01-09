using System;

namespace CapaModelo
{
    public class Log
    {
        public int CodigoLog { get; set; }

        public string Nivel { get; set; }
        public string Categoria { get; set; }
        public string Mensaje { get; set; }
        public string Detalle { get; set; }

        public int? CodigoUsuario { get; set; }

        public string IpAddress { get; set; }
        public string Url { get; set; }

        public DateTime? FechaRegistro { get; set; }
    }
}
