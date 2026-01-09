namespace CapaNegocio
{
    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public dynamic Datos { get; set; }

        public static ResultadoOperacion Ok(dynamic datos = null, string mensaje = "")
        {
            return new ResultadoOperacion { Exito = true, Datos = datos, Mensaje = mensaje };
        }

        public static ResultadoOperacion Error(string mensaje)
        {
            return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
        }
    }
}
