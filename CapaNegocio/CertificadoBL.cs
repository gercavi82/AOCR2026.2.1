using System;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class CertificadoBL
    {
        private readonly CertificadoDAO _dao;

        public CertificadoBL()
        {
            _dao = new CertificadoDAO();
        }

        public Certificado ObtenerPorSolicitud(int solicitudId)
        {
            return _dao.ObtenerPorSolicitud(solicitudId);
        }

        public Certificado Obtener(int id)
        {
            return _dao.ObtenerPorId(id);
        }

        public int GenerarCertificado(int solicitudId, string usuario)
        {
            var cert = new Certificado
            {
                CodigoSolicitud = solicitudId,
                NumeroCertificado = "AOCR-" + DateTime.Now.Ticks,
                FechaEmision = DateTime.Now,
                VigenciaAnios = 1,
                FechaVencimiento = DateTime.Now.AddYears(1),
                Estado = "VIGENTE",
                FirmadoPor = usuario,
                CodigoVerificacion = Guid.NewGuid().ToString("N"),
                RutaPdf = null
            };

            return _dao.Crear(cert);
        }

        public bool SubirPDF(int id, string ruta)
        {
            var cert = _dao.ObtenerPorId(id);
            if (cert == null) return false;

            cert.RutaPdf = ruta;
            return _dao.Actualizar(cert);
        }
    }
}
