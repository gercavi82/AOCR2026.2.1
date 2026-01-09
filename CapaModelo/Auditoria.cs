using System;

namespace CapaModelo
{
    public class Auditoria
    {
        public string Entidad { get; set; }
        public string Accion { get; set; }
        public string Usuario { get; set; }           // Se espera como string (para evitar CS0029)
        public DateTime Fecha { get; set; }
        public string DatosPrevios { get; set; }
        public string DatosNuevos { get; set; }
    }
}
