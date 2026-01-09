using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class AeronaveSolicitudBL
    {
        private readonly AeronaveSolicitudDAO _dao;

        public AeronaveSolicitudBL()
        {
            _dao = new AeronaveSolicitudDAO();
        }

        public List<AeronaveSolicitud> ObtenerPorSolicitud(int codigoSolicitud)
        {
            return _dao.ObtenerPorSolicitud(codigoSolicitud);
        }

        public bool Crear(AeronaveSolicitud a, int codigoUsuario)
        {
            a.UsuarioRegistro = codigoUsuario.ToString();
            int id = _dao.Crear(a);
            return id > 0;
        }

        public bool Eliminar(int codigoAeronaveSolicitud)
        {
            return _dao.Eliminar(codigoAeronaveSolicitud);
        }

        public bool ReemplazarLista(int codigoSolicitud, List<AeronaveSolicitud> lista, int codigoUsuario)
        {
            // Borra todas y vuelve a insertar
            _dao.EliminarPorSolicitud(codigoSolicitud);

            foreach (var a in lista)
            {
                a.CodigoSolicitud = codigoSolicitud;
                a.UsuarioRegistro = codigoUsuario.ToString();
                _dao.Crear(a);
            }

            return true;
        }
    }
}
