using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class PagoBL
    {
        private readonly PagoDAO _pagoDAO;

        public PagoBL()
        {
            _pagoDAO = new PagoDAO();
        }

        public List<Pago> ObtenerPorSolicitud(int codigoSolicitud)
        {
            return _pagoDAO.ObtenerPorSolicitud(codigoSolicitud);
        }

        public Pago ObtenerPorId(int codigoPago)
        {
            return _pagoDAO.ObtenerPorId(codigoPago);
        }

        public bool Registrar(Pago pago)
        {
            if (pago == null) return false;

            if (pago.FechaPago == null)
                pago.FechaPago = DateTime.Now;

            if (string.IsNullOrWhiteSpace(pago.Estado))
                pago.Estado = "PENDIENTE";

            return _pagoDAO.Insertar(pago);
        }

        // Overload simple
        public bool Actualizar(Pago pago)
        {
            if (pago == null || pago.CodigoPago <= 0) return false;
            return _pagoDAO.Actualizar(pago);
        }

        // Overload con mensaje para mantener compatibilidad
        public bool Actualizar(Pago pago, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var ok = Actualizar(pago);
                mensaje = ok ? "Pago actualizado correctamente." : "No se pudo actualizar el pago.";
                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public bool ExistePorNumeroTransaccion(string numeroTransaccion)
        {
            return _pagoDAO.ExistePorNumeroTransaccion(numeroTransaccion);
        }

        public List<Pago> ObtenerPagosValidadosHoy()
        {
            return _pagoDAO.ObtenerPagosValidadosHoy();
        }

        public decimal ObtenerMontoRecaudadoMes(int año, int mes)
        {
            return _pagoDAO.ObtenerMontoRecaudadoMes(año, mes);
        }
    }
}
