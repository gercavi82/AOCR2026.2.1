using System;

namespace CapaModelo
{
    public class Aeronave
    {
        public int CodigoAeronave { get; set; }
        public int CodigoSolicitud { get; set; }
        public string Fabricante { get; set; }  // Antes 'Marca'
        public string Modelo { get; set; }
        public string Serie { get; set; }       // Antes 'NumeroSerie'
        public string Matricula { get; set; }
        public string Configuracion { get; set; }
        public string EtapaRuido { get; set; }
        public decimal? PesoMax { get; set; }   // Propiedad crítica para MTOW
        public int? AnioFabricacion { get; set; }
        public int? CapacidadPasajeros { get; set; }
        public decimal? CapacidadCarga { get; set; }
        public string TipoMotor { get; set; }
        public int? NumeroMotores { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}