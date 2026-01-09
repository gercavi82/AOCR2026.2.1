using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CapaModelo;
using CapaDatos.DAOs;

// Alias por claridad
using InformeDAOType = CapaDatos.DAOs.InformeDAO;
using ChecklistDAOType = CapaDatos.DAOs.ChecklistDAO;

namespace CapaNegocio
{
    public class InformeBL
    {
        #region Helpers de Reflection

        private static int? GetIntNullableProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return null;
                var v = p.GetValue(obj, null);
                if (v == null) return null;
                return Convert.ToInt32(v);
            }
            catch { return null; }
        }

        private static void SetIntProp(object obj, string propName, int? value)
        {
            try
            {
                if (obj == null) return;
                var p = obj.GetType().GetProperty(propName);
                if (p == null || !p.CanWrite) return;

                if (p.PropertyType == typeof(int))
                    p.SetValue(obj, value ?? 0, null);
                else if (p.PropertyType == typeof(int?))
                    p.SetValue(obj, value, null);
            }
            catch { }
        }

        private static void SetStringProp(object obj, string propName, string value)
        {
            try
            {
                if (obj == null) return;
                var p = obj.GetType().GetProperty(propName);
                if (p == null || !p.CanWrite) return;
                p.SetValue(obj, value, null);
            }
            catch { }
        }

        private static void SetDateProp(object obj, string propName, DateTime? value)
        {
            try
            {
                if (obj == null) return;
                var p = obj.GetType().GetProperty(propName);
                if (p == null || !p.CanWrite) return;
                p.SetValue(obj, value, null);
            }
            catch { }
        }

        #endregion

        #region CRUD de Informe AOCR

        public static bool Insertar(Informe informe, out string mensaje)
        {
            mensaje = "";

            try
            {
                if (!ValidarInforme(informe, out mensaje))
                    return false;

                SetDateProp(informe, "FechaCreacion", DateTime.Now);

                int nuevoId = InformeDAOType.Insertar(informe);
                bool ok = nuevoId > 0;

                if (ok)
                {
                    SetIntProp(informe, "CodigoInforme", nuevoId);
                    mensaje = "Informe AOCR creado correctamente.";
                }
                else
                {
                    mensaje = "Error al crear el informe AOCR.";
                }

                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public static bool Actualizar(Informe informe, out string mensaje)
        {
            mensaje = "";

            try
            {
                if (!ValidarInforme(informe, out mensaje))
                    return false;

                bool ok = InformeDAOType.Actualizar(informe);

                if (ok) mensaje = "Informe AOCR actualizado correctamente.";
                else mensaje = "Error al actualizar informe AOCR.";

                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public static bool Eliminar(int codigoInforme, out string mensaje)
        {
            mensaje = "";

            try
            {
                bool ok = InformeDAOType.Eliminar(codigoInforme);

                if (ok) mensaje = "Informe AOCR eliminado correctamente.";
                else mensaje = "Error al eliminar informe AOCR.";

                return ok;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                return false;
            }
        }

        public static Informe ObtenerPorId(int codigoInforme)
        {
            try
            {
                return InformeDAOType.ObtenerPorId(codigoInforme);
            }
            catch
            {
                return null;
            }
        }

        public static List<Informe> ObtenerTodos()
        {
            try
            {
                return InformeDAOType.ObtenerTodos();
            }
            catch
            {
                return new List<Informe>();
            }
        }

        #endregion

        #region Estadísticas de Checklist por Solicitud AOCR

        public static List<ChecklistItem> ObtenerChecklistPorSolicitud(int codigoSolicitud)
        {
            try
            {
                return ChecklistDAOType.ObtenerPorSolicitud(codigoSolicitud);
            }
            catch
            {
                return new List<ChecklistItem>();
            }
        }

        public static Dictionary<string, int> ObtenerEstadisticasChecklist(int codigoSolicitud)
        {
            try
            {
                return ChecklistDAOType.ObtenerEstadisticasPorSolicitud(codigoSolicitud);
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        #endregion

        #region Generación de Informe AOCR

        public static bool GenerarInformeAutomaticoAOCR(
            int codigoSolicitud,
            out string mensaje)
        {
            mensaje = "";

            try
            {
                // Obtener checklist
                var checklists = ObtenerChecklistPorSolicitud(codigoSolicitud);

                // Obtener estadísticas
                var estadisticas = ObtenerEstadisticasChecklist(codigoSolicitud);

                // Generar texto
                string contenido = GenerarContenidoAOCR(checklists, estadisticas);

                var informe = new Informe();
                SetIntProp(informe, "CodigoSolicitud", codigoSolicitud);
                SetStringProp(informe, "Contenido", contenido);
                SetStringProp(informe, "Conclusiones", GenerarConclusiones(estadisticas));
                SetStringProp(informe, "Recomendaciones", "");
                SetStringProp(informe, "Estado", "Generado");
                SetDateProp(informe, "FechaCreacion", DateTime.Now);

                return Insertar(informe, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = "Error generando informe: " + ex.Message;
                return false;
            }
        }

        private static string GenerarContenidoAOCR(
            List<ChecklistItem> checklistItems,
            Dictionary<string, int> estadisticas)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== INFORME AOCR ===");
            sb.AppendLine(DateTime.Now.ToString("dd/MM/yyyy"));
            sb.AppendLine();
            sb.AppendLine("--- Checklist ---");

            foreach (var item in checklistItems)
            {
                string cumple = item.Cumple == true ? "Sí" : item.Cumple == false ? "No" : "No evaluado";
                sb.AppendLine($"{item.Descripcion}: {cumple}");
            }

            sb.AppendLine();
            sb.AppendLine("--- Estadísticas ---");
            foreach (var kv in estadisticas)
            {
                sb.AppendLine($"{kv.Key}: {kv.Value}");
            }

            return sb.ToString();
        }

        private static string GenerarConclusiones(
            Dictionary<string, int> estadisticas)
        {
            int total = estadisticas.ContainsKey("Total") ? estadisticas["Total"] : 0;
            int cumple = estadisticas.ContainsKey("Cumplen") ? estadisticas["Cumplen"] : 0;

            if (total == 0) return "No hay datos de checklist.";

            decimal porcentaje = (decimal)cumple / total * 100;
            return $"Porcentaje de cumplimiento: {porcentaje:F2}%";
        }

        #endregion

        #region Validaciones

        private static bool ValidarInforme(Informe informe, out string mensaje)
        {
            mensaje = "";

            if (informe == null)
            {
                mensaje = "Informe inválido.";
                return false;
            }

            int? sol = GetIntNullableProp(informe, "CodigoSolicitud");
            if (!sol.HasValue)
            {
                mensaje = "La solicitud AOCR es requerida.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
