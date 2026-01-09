// CapaPresentacion\Models\ErrorViewModel.cs

namespace CapaPresentacion.Models
{
    public class ErrorViewModel
    {
        // En .NET Framework 4.7.2 no usamos "string?"
        public string RequestId { get; set; }

        // Esta expresión ya compila bien con string normal
        public bool ShowRequestId
        {
            get { return !string.IsNullOrEmpty(RequestId); }
        }
    }
}
