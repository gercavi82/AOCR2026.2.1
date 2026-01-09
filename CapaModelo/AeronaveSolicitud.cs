using System;

namespace CapaModelo
{
    public class AeronaveSolicitud
    {
        public int CodigoAeronaveSolicitud { get; set; }
        public int CodigoSolicitud { get; set; }

        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Serie { get; set; }
        public string Matricula { get; set; }
        public string Configuracion { get; set; }
        public string EtapaRuido { get; set; }

        public DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; }
    }
}
