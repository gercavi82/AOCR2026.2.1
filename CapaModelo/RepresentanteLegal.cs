using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaModelo
{
    public class RepresentanteLegal
    {
        [Key]
        public int Id { get; set; }

        public int OidCiaAviacion { get; set; }

        public string NombreCompania { get; set; }

        public string PathArchivo { get; set; }
    }
}
