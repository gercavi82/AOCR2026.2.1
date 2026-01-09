using System;
using System.Collections.Generic;
using System.Linq;
using CapaModelo;
using CapaDatos.DAOs;

// ✅ Alias explícito del tipo para evitar conflictos de nombre
using FinancieroDAOType = CapaDatos.DAOs.FinancieroDAO;

namespace CapaNegocio
{
    public class FinancieroBL
    {
        // ✅ Instancia estable para usar en métodos static
        private static readonly FinancieroDAOType _dao = new FinancieroDAOType();

        #region CRUD Básico

        public static bool Insertar(Financiero financiero, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarFinanciero(financiero, out mensaje))
                    return false;

                bool resultado = _dao.Insertar(financiero);

                if (resultado)
                {
                    mensaje = "Registro financiero creado exitosamente.";
                    LogBL.RegistrarInfo($"Registro financiero creado: Solicitud {financiero.CodigoSolicitud}", "Financiero");
                }
                else
                {
                    mensaje = "Error al crear el registro financiero.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar financiero", ex.ToString(), "Financiero");
                return false;
            }
        }

        public static bool Actualizar(Financiero financiero, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarFinanciero(financiero, out mensaje))
                    return false;

                bool resultado = _dao.Actualizar(financiero);

                if (resultado)
                {
                    mensaje = "Registro financiero actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Registro financiero actualizado: ID {financiero.CodigoFinanciero}", "Financiero");
                }
                else
                {
                    mensaje = "Error al actualizar el registro financiero.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar financiero", ex.ToString(), "Financiero");
                return false;
            }
        }

        public static bool Eliminar(int codigoFinanciero, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = _dao.Eliminar(codigoFinanciero);

                if (resultado)
                {
                    mensaje = "Registro financiero eliminado exitosamente.";
                    LogBL.RegistrarInfo($"Registro financiero eliminado: ID {codigoFinanciero}", "Financiero");
                }
                else
                {
                    mensaje = "Error al eliminar el registro financiero.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar financiero", ex.ToString(), "Financiero");
                return false;
            }
        }

        public static Financiero ObtenerPorId(int codigoFinanciero)
        {
            try
            {
                return _dao.ObtenerPorId(codigoFinanciero);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener registro financiero", ex.ToString(), "Financiero");
                return null;
            }
        }

        public static List<Financiero> ObtenerTodos()
        {
            try
            {
                return _dao.ObtenerTodos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener registros financieros", ex.ToString(), "Financiero");
                return new List<Financiero>();
            }
        }

        #endregion

        #region Métodos Específicos

        /// <summary>
        /// Obtiene TODOS los movimientos financieros de una solicitud.
        /// </summary>
        public static List<Financiero> ObtenerPorSolicitud(int codigoSolicitud)
        {
            try
            {
                return _dao.ObtenerPorSolicitud(codigoSolicitud);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener financiero por solicitud", ex.ToString(), "Financiero");
                return new List<Financiero>();
            }
        }

        public static List<Financiero> ObtenerPorEstadoPago(string estadoPago)
        {
            try
            {
                return _dao.ObtenerPorEstadoPago(estadoPago);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener financieros por estado de pago", ex.ToString(), "Financiero");
                return new List<Financiero>();
            }
        }

        public static List<Financiero> ObtenerPendientesPago()
        {
            try
            {
                return _dao.ObtenerPorEstadoPago("Pendiente");
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener pagos pendientes", ex.ToString(), "Financiero");
                return new List<Financiero>();
            }
        }

        public static bool ActualizarEstadoPago(int codigoFinanciero, string nuevoEstado, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var estadosPermitidos = new[] { "Pendiente", "Pagado", "Cancelado", "Vencido" };
                if (!estadosPermitidos.Contains(nuevoEstado))
                {
                    mensaje = "Estado de pago no válido.";
                    return false;
                }

                bool resultado = _dao.ActualizarEstadoPago(codigoFinanciero, nuevoEstado);

                if (resultado)
                {
                    mensaje = $"Estado de pago actualizado a: {nuevoEstado}";
                    LogBL.RegistrarInfo($"Estado de pago actualizado: {codigoFinanciero} -> {nuevoEstado}", "Financiero");
                }
                else
                {
                    mensaje = "Error al actualizar estado de pago.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar estado de pago", ex.ToString(), "Financiero");
                return false;
            }
        }

        public static bool RegistrarPago(int codigoFinanciero, decimal montoPagado, string metodoPago, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var financiero = _dao.ObtenerPorId(codigoFinanciero);
                if (financiero == null)
                {
                    mensaje = "Registro financiero no encontrado.";
                    return false;
                }

                if (montoPagado <= 0)
                {
                    mensaje = "El monto pagado debe ser mayor a cero.";
                    return false;
                }

                if (montoPagado > financiero.MontoTotal.GetValueOrDefault())
                {
                    mensaje = "El monto pagado no puede exceder el monto total.";
                    return false;
                }

                bool resultado = _dao.RegistrarPago(codigoFinanciero, montoPagado, metodoPago);

                if (resultado)
                {
                    mensaje = "Pago registrado exitosamente.";
                    LogBL.RegistrarInfo($"Pago registrado: {codigoFinanciero} - ${montoPagado}", "Financiero");

                    // Actualizar estado si está completamente pagado
                    if (montoPagado >= financiero.MontoTotal.GetValueOrDefault())
                    {
                        ActualizarEstadoPago(codigoFinanciero, "Pagado", out _);
                    }
                }
                else
                {
                    mensaje = "Error al registrar el pago.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al registrar pago", ex.ToString(), "Financiero");
                return false;
            }
        }

        #endregion

        #region Cálculos y Reportes

        /// <summary>
        /// Calcula el monto total de TODOS los movimientos de una solicitud.
        /// </summary>
        public static decimal CalcularMontoTotal(int codigoSolicitud)
        {
            try
            {
                var movimientos = _dao.ObtenerPorSolicitud(codigoSolicitud);

                if (movimientos == null || movimientos.Count == 0)
                    return 0;

                decimal subtotal = movimientos.Sum(m => m.MontoBase ?? 0m);
                decimal impuestos = movimientos.Sum(m => m.Impuestos ?? 0m);
                decimal descuentos = movimientos.Sum(m => m.Descuentos ?? 0m);
                decimal recargos = movimientos.Sum(m => m.Recargos ?? 0m);

                return subtotal + impuestos - descuentos + recargos;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al calcular monto total", ex.ToString(), "Financiero");
                return 0;
            }
        }

        public static decimal ObtenerTotalIngresos(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return _dao.ObtenerTotalIngresos(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener total de ingresos", ex.ToString(), "Financiero");
                return 0;
            }
        }

        public static decimal ObtenerTotalPendiente()
        {
            try
            {
                var pendientes = _dao.ObtenerPorEstadoPago("Pendiente");
                return pendientes.Sum(f => f.MontoTotal.GetValueOrDefault() - f.MontoPagado.GetValueOrDefault());
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener total pendiente", ex.ToString(), "Financiero");
                return 0;
            }
        }

        public static Dictionary<string, decimal> ObtenerResumenFinanciero(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var resumen = new Dictionary<string, decimal>();
                var financieros = _dao.ObtenerTodos()
                    .Where(f => f.FechaEmision.HasValue &&
                               f.FechaEmision.Value >= fechaInicio &&
                               f.FechaEmision.Value <= fechaFin)
                    .ToList();

                resumen["Total Facturado"] = financieros.Sum(f => f.MontoTotal.GetValueOrDefault());
                resumen["Total Pagado"] = financieros.Sum(f => f.MontoPagado.GetValueOrDefault());
                resumen["Total Pendiente"] = financieros
                    .Where(f => f.EstadoPago == "Pendiente")
                    .Sum(f => f.MontoTotal.GetValueOrDefault() - f.MontoPagado.GetValueOrDefault());
                resumen["Total Impuestos"] = financieros.Sum(f => f.Impuestos.GetValueOrDefault());
                resumen["Total Descuentos"] = financieros.Sum(f => f.Descuentos.GetValueOrDefault());

                return resumen;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener resumen financiero", ex.ToString(), "Financiero");
                return new Dictionary<string, decimal>();
            }
        }

        #endregion

        #region Validaciones

        private static bool ValidarFinanciero(Financiero financiero, out string mensaje)
        {
            mensaje = string.Empty;

            if (financiero == null)
            {
                mensaje = "Los datos financieros son requeridos.";
                return false;
            }

            if (!financiero.CodigoSolicitud.HasValue || financiero.CodigoSolicitud <= 0)
            {
                mensaje = "El código de solicitud es requerido.";
                return false;
            }

            if (financiero.MontoBase.HasValue && financiero.MontoBase < 0)
            {
                mensaje = "El monto base no puede ser negativo.";
                return false;
            }

            if (financiero.Impuestos.HasValue && financiero.Impuestos < 0)
            {
                mensaje = "Los impuestos no pueden ser negativos.";
                return false;
            }

            if (financiero.Descuentos.HasValue && financiero.Descuentos < 0)
            {
                mensaje = "Los descuentos no pueden ser negativos.";
                return false;
            }

            if (financiero.MontoPagado.HasValue && financiero.MontoPagado < 0)
            {
                mensaje = "El monto pagado no puede ser negativo.";
                return false;
            }

            if (financiero.MontoPagado.HasValue && financiero.MontoTotal.HasValue &&
                financiero.MontoPagado > financiero.MontoTotal)
            {
                mensaje = "El monto pagado no puede exceder el monto total.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
