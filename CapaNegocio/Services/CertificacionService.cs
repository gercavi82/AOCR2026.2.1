using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio.Services
{
    public class CertificacionService
    {
        private readonly CertificadoDAO _certificadoDAO;

        public CertificacionService()
        {
            _certificadoDAO = new CertificadoDAO();
        }

        public Certificado ObtenerCertificado(int id)
        {
            return _certificadoDAO.ObtenerPorSolicitud(id);
        }

        public Certificado CrearCertificado(Certificado cert)
        {
            if (_certificadoDAO.Crear(cert))
                return cert;

            return null;
        }

        public bool ActualizarCertificado(Certificado cert)
        {
            return _certificadoDAO.Actualizar(cert);
        }
    }
}
