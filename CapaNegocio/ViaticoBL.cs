using System;
using System.Collections.Generic;
using System.Linq;
using CapaDatos;
using CapaModelo;

namespace CapaNegocio
{
    public class ViaticoBL
    {
        #region CRUD Básico

        public static bool Insertar(Viatico viatico, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarViatico(viatico, out mensaje))
                    return false;

                viatico.FechaSolicitud = DateTime.Now;
                viatico.Estado = "Pendiente";

                bool resultado = ViaticoDAO.Insertar(viatico);

                if (resultado)
                {
                    mensaje = "Viático registrado exitosamente.";
                    LogBL.RegistrarInfo($"Viático registrado: Inspección {viatico.CodigoInspeccion}", "Viatico");
                }
                else
                {
                    mensaje = "Error al registrar el viático.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar viático", ex.ToString(), "Viatico");
                return false;
            }
        }

        public static bool Actualizar(Viatico viatico, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones
                if (!ValidarViatico(viatico, out mensaje))
                    return false;

                bool resultado = ViaticoDAO.Actualizar(viatico);

                if (resultado)
                {
                    mensaje = "Viático actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Viático actualizado: ID {viatico.CodigoViatico}", "Viatico");
                }
                else
                {
                    mensaje = "Error al actualizar el viático.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar viático", ex.ToString(), "Viatico");
                return false;
            }
        }

        public static bool Eliminar(int codigoViatico, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = ViaticoDAO.Eliminar(codigoViatico);

                if (resultado)
                {
                    mensaje = "Viático eliminado exitosamente.";
                    LogBL.RegistrarInfo($"Viático eliminado: ID {codigoViatico}", "Viatico");
                }
                else
                {
                    mensaje = "Error al eliminar el viático.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar viático", ex.ToString(), "Viatico");
                return false;
            }
        }

        public static Viatico ObtenerPorId(int codigoViatico)
        {
            try
            {
                return ViaticoDAO.ObtenerPorId(codigoViatico);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viático", ex.ToString(), "Viatico");
                return null;
            }
        }

        public static List<Viatico> ObtenerTodos()
        {
            try
            {
                return ViaticoDAO.ObtenerTodos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viáticos", ex.ToString(), "Viatico");
                return new List<Viatico>();
            }
        }

        #endregion

        #region Métodos Específicos

        public static List<Viatico> ObtenerPorInspeccion(int codigoInspeccion)
        {
            try
            {
                return ViaticoDAO.ObtenerPorInspeccion(codigoInspeccion);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viáticos por inspección", ex.ToString(), "Viatico");
                return new List<Viatico>();
            }
        }

        public static List<Viatico> ObtenerPorTecnico(int codigoTecnico)
        {
            try
            {
                return ViaticoDAO.ObtenerPorTecnico(codigoTecnico);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viáticos por técnico", ex.ToString(), "Viatico");
                return new List<Viatico>();
            }
        }

        public static List<Viatico> ObtenerPorEstado(string estado)
        {
            try
            {
                return ViaticoDAO.ObtenerPorEstado(estado);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viáticos por estado", ex.ToString(), "Viatico");
                return new List<Viatico>();
            }
        }

        public static List<Viatico> ObtenerPendientes()
        {
            try
            {
                return ViaticoDAO.ObtenerPorEstado("Pendiente");
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viáticos pendientes", ex.ToString(), "Viatico");
                return new List<Viatico>();
            }
        }

        public static List<Viatico> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return ViaticoDAO.ObtenerPorFecha(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener viáticos por fecha", ex.ToString(), "Viatico");
                return new List<Viatico>();
            }
        }

        #endregion

        #region Gestión de Estados

        public static bool Aprobar(int codigoViatico, int codigoAprobador, string observaciones, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var viatico = ViaticoDAO.ObtenerPorId(codigoViatico);
                if (viatico == null)
                {
                    mensaje = "Viático no encontrado.";
                    return false;
                }

                if (viatico.Estado == "Aprobado")
                {
                    mensaje = "El viático ya está aprobado.";
                    return false;
                }

                viatico.Estado = "Aprobado";
                viatico.FechaAprobacion = DateTime.Now;
                viatico.Observaciones = (viatico.Observaciones ?? "") +
                    $"\n\nAPROBADO ({DateTime.Now:dd/MM/yyyy HH:mm}): {observaciones}";

                bool resultado = ViaticoDAO.Actualizar(viatico);

                if (resultado)
                {
                    mensaje = "Viático aprobado exitosamente.";
                    LogBL.RegistrarInfo($"Viático aprobado: ID {codigoViatico} por usuario {codigoAprobador}", "Viatico");
                }
                else
                {
                    mensaje = "Error al aprobar el viático.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al aprobar viático", ex.ToString(), "Viatico");
                return false;
            }
        }

        public static bool Rechazar(int codigoViatico, int codigoAprobador, string motivo, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var viatico = ViaticoDAO.ObtenerPorId(codigoViatico);
                if (viatico == null)
                {
                    mensaje = "Viático no encontrado.";
                    return false;
                }

                viatico.Estado = "Rechazado";
                viatico.Observaciones = (viatico.Observaciones ?? "") +
                    $"\n\nRECHAZADO ({DateTime.Now:dd/MM/yyyy HH:mm}): {motivo}";

                bool resultado = ViaticoDAO.Actualizar(viatico);

                if (resultado)
                {
                    mensaje = "Viático rechazado.";
                    LogBL.RegistrarInfo($"Viático rechazado: ID {codigoViatico} por usuario {codigoAprobador}", "Viatico");
                }
                else
                {
                    mensaje = "Error al rechazar el viático.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al rechazar viático", ex.ToString(), "Viatico");
                return false;
            }
        }

        public static bool Pagar(int codigoViatico, decimal montoPagado, string metodoPago, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var viatico = ViaticoDAO.ObtenerPorId(codigoViatico);
                if (viatico == null)
                {
                    mensaje = "Viático no encontrado.";
                    return false;
                }

                if (viatico.Estado != "Aprobado")
                {
                    mensaje = "El viático debe estar aprobado para poder pagarse.";
                    return false;
                }

                viatico.Estado = "Pagado";
                viatico.MontoPagado = montoPagado;
                viatico.FechaPago = DateTime.Now;
                viatico.Observaciones = (viatico.Observaciones ?? "") +
                    $"\n\nPAGADO ({DateTime.Now:dd/MM/yyyy HH:mm}): ${montoPagado} - Método: {metodoPago}";

                bool resultado = ViaticoDAO.Actualizar(viatico);

                if (resultado)
                {
                    mensaje = "Viático pagado exitosamente.";
                    LogBL.RegistrarInfo($"Viático pagado: ID {codigoViatico} - ${montoPagado}", "Viatico");
                }
                else
                {
                    mensaje = "Error al registrar el pago del viático.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al pagar viático", ex.ToString(), "Viatico");
                return false;
            }
        }

        #endregion

        #region Cálculos

        public static decimal CalcularMontoTotal(Viatico viatico)
        {
            try
            {
                decimal total = 0;

                total += viatico.MontoTransporte.GetValueOrDefault();
                total += viatico.MontoAlimentacion.GetValueOrDefault();
                total += viatico.MontoHospedaje.GetValueOrDefault();
                total += viatico.OtrosGastos.GetValueOrDefault();

                return total;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al calcular monto total", ex.ToString(), "Viatico");
                return 0;
            }
        }

        public static decimal ObtenerTotalPorInspeccion(int codigoInspeccion)
        {
            try
            {
                var viaticos = ViaticoDAO.ObtenerPorInspeccion(codigoInspeccion);
                return viaticos.Sum(v => CalcularMontoTotal(v));
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener total por inspección", ex.ToString(), "Viatico");
                return 0;
            }
        }

        public static decimal ObtenerTotalPorTecnico(int codigoTecnico, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var viaticos = ViaticoDAO.ObtenerPorTecnico(codigoTecnico)
                    .Where(v => v.FechaSolicitud.HasValue &&
                               v.FechaSolicitud.Value >= fechaInicio &&
                               v.FechaSolicitud.Value <= fechaFin)
                    .ToList();

                return viaticos.Sum(v => CalcularMontoTotal(v));
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener total por técnico", ex.ToString(), "Viatico");
                return 0;
            }
        }

        #endregion

        #region Reportes y Estadísticas

        public static Dictionary<string, decimal> ObtenerEstadisticasViaticosPorEstado()
        {
            try
            {
                var viaticos = ViaticoDAO.ObtenerTodos();
                var estadisticas = new Dictionary<string, decimal>();

                var estados = viaticos.Select(v => v.Estado ?? "Sin Estado").Distinct();

                foreach (var estado in estados)
                {
                    var viaticosEstado = viaticos.Where(v => v.Estado == estado).ToList();
                    decimal total = viaticosEstado.Sum(v => CalcularMontoTotal(v));
                    estadisticas[estado] = total;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener estadísticas por estado", ex.ToString(), "Viatico");
                return new Dictionary<string, decimal>();
            }
        }

        public static Dictionary<string, decimal> ObtenerGastosPorConcepto(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var viaticos = ViaticoDAO.ObtenerPorFecha(fechaInicio, fechaFin);
                var gastos = new Dictionary<string, decimal>
                {
                    ["Transporte"] = viaticos.Sum(v => v.MontoTransporte.GetValueOrDefault()),
                    ["Alimentación"] = viaticos.Sum(v => v.MontoAlimentacion.GetValueOrDefault()),
                    ["Hospedaje"] = viaticos.Sum(v => v.MontoHospedaje.GetValueOrDefault()),
                    ["Otros Gastos"] = viaticos.Sum(v => v.OtrosGastos.GetValueOrDefault())
                };

                return gastos;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener gastos por concepto", ex.ToString(), "Viatico");
                return new Dictionary<string, decimal>();
            }
        }

        public static int ContarPendientes()
        {
            try
            {
                return ViaticoDAO.ObtenerPorEstado("Pendiente").Count;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al contar viáticos pendientes", ex.ToString(), "Viatico");
                return 0;
            }
        }

        public static decimal ObtenerTotalPendientePago()
        {
            try
            {
                var viaticosAprobados = ViaticoDAO.ObtenerPorEstado("Aprobado");
                return viaticosAprobados.Sum(v => CalcularMontoTotal(v));
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener total pendiente de pago", ex.ToString(), "Viatico");
                return 0;
            }
        }

        #endregion

        #region Validaciones

        private static bool ValidarViatico(Viatico viatico, out string mensaje)
        {
            mensaje = string.Empty;

            if (viatico == null)
            {
                mensaje = "Los datos del viático son requeridos.";
                return false;
            }

            if (!viatico.CodigoInspeccion.HasValue || viatico.CodigoInspeccion <= 0)
            {
                mensaje = "El código de inspección es requerido.";
                return false;
            }

            if (!viatico.CodigoTecnico.HasValue || viatico.CodigoTecnico <= 0)
            {
                mensaje = "El código de técnico es requerido.";
                return false;
            }

            if (viatico.MontoTransporte.HasValue && viatico.MontoTransporte < 0)
            {
                mensaje = "El monto de transporte no puede ser negativo.";
                return false;
            }

            if (viatico.MontoAlimentacion.HasValue && viatico.MontoAlimentacion < 0)
            {
                mensaje = "El monto de alimentación no puede ser negativo.";
                return false;
            }

            if (viatico.MontoHospedaje.HasValue && viatico.MontoHospedaje < 0)
            {
                mensaje = "El monto de hospedaje no puede ser negativo.";
                return false;
            }

            if (viatico.OtrosGastos.HasValue && viatico.OtrosGastos < 0)
            {
                mensaje = "Otros gastos no pueden ser negativos.";
                return false;
            }

            // Validar estados permitidos
            if (!string.IsNullOrWhiteSpace(viatico.Estado))
            {
                var estadosPermitidos = new[] { "Pendiente", "Aprobado", "Rechazado", "Pagado" };
                if (!estadosPermitidos.Contains(viatico.Estado))
                {
                    mensaje = "Estado no válido.";
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}