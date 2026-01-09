using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CapaModelo;
using CapaDatos.DAOs;

// ✅ ALIAS EXPLÍCITOS (evitan conflictos)
using TecnicoDAOType = CapaDatos.DAOs.TecnicoDAO;
using InspeccionDAOType = CapaDatos.DAOs.InspeccionDAO;

namespace CapaNegocio
{
    public class TecnicoBL
    {
        // ======================================================
        // Helpers Reflection (tolerantes al modelo)
        // ======================================================
        private static int? GetIntProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is int i) return i;
                if (val is int?) return (int?)val;

                return Convert.ToInt32(val);
            }
            catch
            {
                return null;
            }
        }

        private static void SetIntProp(object obj, string propName, int? value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                if (prop.PropertyType == typeof(int))
                    prop.SetValue(obj, value ?? 0, null);
                else if (prop.PropertyType == typeof(int?))
                    prop.SetValue(obj, value, null);
            }
            catch
            {
                // silencioso
            }
        }

        private static string GetStringProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                return val?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void SetStringProp(object obj, string propName, string value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                if (prop.PropertyType == typeof(string))
                    prop.SetValue(obj, value, null);
            }
            catch
            {
                // silencioso
            }
        }

        private static bool? GetBoolPropNullable(object obj, string propName)
        {
            try
            {
                if (obj == null) return null;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;

                var val = prop.GetValue(obj, null);
                if (val == null) return null;

                if (val is bool b) return b;
                if (val is bool?) return (bool?)val;

                if (val is int i) return i != 0;
                if (val is short s) return s != 0;

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool GetBoolProp(object obj, string propName)
        {
            return GetBoolPropNullable(obj, propName).GetValueOrDefault();
        }

        private static void SetBoolProp(object obj, string propName, bool value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                if (prop.PropertyType == typeof(bool))
                    prop.SetValue(obj, value, null);
                else if (prop.PropertyType == typeof(bool?))
                    prop.SetValue(obj, (bool?)value, null);
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(obj, value ? 1 : 0, null);
                else if (prop.PropertyType == typeof(short))
                    prop.SetValue(obj, (short)(value ? 1 : 0), null);
            }
            catch
            {
                // silencioso
            }
        }

        // ======================================================
        // CRUD Básico
        // ======================================================
        public static bool Insertar(Tecnico tecnico, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarTecnico(tecnico, out mensaje))
                    return false;

                // ✅ CodigoUsuario opcional en el modelo => Reflection
                int codigoUsuario = GetIntProp(tecnico, "CodigoUsuario").GetValueOrDefault();

                // Verificar si el usuario ya está registrado como técnico
                if (codigoUsuario > 0 && TecnicoDAOType.ExistePorUsuario(codigoUsuario))
                {
                    mensaje = "El usuario ya está registrado como técnico.";
                    return false;
                }

                bool resultado = TecnicoDAOType.Insertar(tecnico);

                if (resultado)
                {
                    mensaje = "Técnico registrado exitosamente.";
                    LogBL.RegistrarInfo($"Técnico registrado: Usuario {codigoUsuario}", "Tecnico");
                }
                else
                {
                    mensaje = "Error al registrar el técnico.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar técnico", ex.ToString(), "Tecnico");
                return false;
            }
        }

        public static bool Actualizar(Tecnico tecnico, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarTecnico(tecnico, out mensaje))
                    return false;

                bool resultado = TecnicoDAOType.Actualizar(tecnico);

                if (resultado)
                {
                    mensaje = "Técnico actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Técnico actualizado: ID {tecnico.CodigoTecnico}", "Tecnico");
                }
                else
                {
                    mensaje = "Error al actualizar el técnico.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar técnico", ex.ToString(), "Tecnico");
                return false;
            }
        }

        public static bool Eliminar(int codigoTecnico, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Verificar si tiene inspecciones asignadas
                var inspecciones = InspeccionDAOType.ObtenerPorTecnico(codigoTecnico);
                if (inspecciones != null && inspecciones.Count > 0)
                {
                    mensaje = "No se puede eliminar el técnico porque tiene inspecciones asignadas.";
                    return false;
                }

                bool resultado = TecnicoDAOType.Eliminar(codigoTecnico);

                if (resultado)
                {
                    mensaje = "Técnico eliminado exitosamente.";
                    LogBL.RegistrarInfo($"Técnico eliminado: ID {codigoTecnico}", "Tecnico");
                }
                else
                {
                    mensaje = "Error al eliminar el técnico.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar técnico", ex.ToString(), "Tecnico");
                return false;
            }
        }

        public static Tecnico ObtenerPorId(int codigoTecnico)
        {
            try
            {
                return TecnicoDAOType.ObtenerPorId(codigoTecnico);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnico", ex.ToString(), "Tecnico");
                return null;
            }
        }

        public static List<Tecnico> ObtenerTodos()
        {
            try
            {
                return TecnicoDAOType.ObtenerTodos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnicos", ex.ToString(), "Tecnico");
                return new List<Tecnico>();
            }
        }

        // ======================================================
        // Métodos Específicos
        // ======================================================
        public static Tecnico ObtenerPorUsuario(int codigoUsuario)
        {
            try
            {
                return TecnicoDAOType.ObtenerPorUsuario(codigoUsuario);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnico por usuario", ex.ToString(), "Tecnico");
                return null;
            }
        }

        public static List<Tecnico> ObtenerActivos()
        {
            try
            {
                return TecnicoDAOType.ObtenerActivos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnicos activos", ex.ToString(), "Tecnico");
                return new List<Tecnico>();
            }
        }

        public static List<Tecnico> ObtenerPorEspecialidad(string especialidad)
        {
            try
            {
                return TecnicoDAOType.ObtenerPorEspecialidad(especialidad);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnicos por especialidad", ex.ToString(), "Tecnico");
                return new List<Tecnico>();
            }
        }

        public static List<Tecnico> ObtenerDisponibles()
        {
            try
            {
                return TecnicoDAOType.ObtenerDisponibles();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnicos disponibles", ex.ToString(), "Tecnico");
                return new List<Tecnico>();
            }
        }

        public static bool CambiarEstado(int codigoTecnico, bool activo, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var tecnico = TecnicoDAOType.ObtenerPorId(codigoTecnico);
                if (tecnico == null)
                {
                    mensaje = "Técnico no encontrado.";
                    return false;
                }

                // ✅ Activo opcional en el modelo
                SetBoolProp(tecnico, "Activo", activo);

                bool resultado = TecnicoDAOType.Actualizar(tecnico);

                if (resultado)
                {
                    string estado = activo ? "activado" : "desactivado";
                    mensaje = $"Técnico {estado} exitosamente.";
                    LogBL.RegistrarInfo($"Técnico {estado}: ID {codigoTecnico}", "Tecnico");
                }
                else
                {
                    mensaje = "Error al cambiar el estado del técnico.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al cambiar estado de técnico", ex.ToString(), "Tecnico");
                return false;
            }
        }

        public static bool CambiarDisponibilidad(int codigoTecnico, bool disponible, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var tecnico = TecnicoDAOType.ObtenerPorId(codigoTecnico);
                if (tecnico == null)
                {
                    mensaje = "Técnico no encontrado.";
                    return false;
                }

                // ✅ Disponible opcional en el modelo
                SetBoolProp(tecnico, "Disponible", disponible);

                bool resultado = TecnicoDAOType.Actualizar(tecnico);

                if (resultado)
                {
                    string estado = disponible ? "disponible" : "no disponible";
                    mensaje = $"Técnico marcado como {estado}.";
                    LogBL.RegistrarInfo($"Disponibilidad de técnico actualizada: ID {codigoTecnico} -> {estado}", "Tecnico");
                }
                else
                {
                    mensaje = "Error al cambiar la disponibilidad.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al cambiar disponibilidad de técnico", ex.ToString(), "Tecnico");
                return false;
            }
        }

        // ======================================================
        // Asignación de Inspecciones
        // ======================================================
        public static Tecnico ObtenerTecnicoDisponibleParaInspeccion(string especialidad = null)
        {
            try
            {
                var tecnicos = TecnicoDAOType.ObtenerDisponibles() ?? new List<Tecnico>();

                if (!string.IsNullOrEmpty(especialidad))
                {
                    tecnicos = tecnicos.Where(t => (t.Especialidad ?? "") == especialidad).ToList();
                }

                Tecnico tecnicoSeleccionado = null;
                int menorCantidad = int.MaxValue;

                foreach (var tecnico in tecnicos)
                {
                    var lista = InspeccionDAOType.ObtenerPorTecnico(tecnico.CodigoTecnico);
                    int cantidadInspecciones = lista != null ? lista.Count : 0;

                    if (cantidadInspecciones < menorCantidad)
                    {
                        menorCantidad = cantidadInspecciones;
                        tecnicoSeleccionado = tecnico;
                    }
                }

                return tecnicoSeleccionado;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener técnico disponible para inspección", ex.ToString(), "Tecnico");
                return null;
            }
        }

        public static int ContarInspeccionesAsignadas(int codigoTecnico)
        {
            try
            {
                var lista = InspeccionDAOType.ObtenerPorTecnico(codigoTecnico);
                return lista != null ? lista.Count : 0;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al contar inspecciones asignadas", ex.ToString(), "Tecnico");
                return 0;
            }
        }

        // ======================================================
        // Estadísticas
        // ======================================================
        public static Dictionary<string, int> ObtenerEstadisticasPorEspecialidad()
        {
            try
            {
                var tecnicos = TecnicoDAOType.ObtenerTodos() ?? new List<Tecnico>();
                var estadisticas = new Dictionary<string, int>();

                foreach (var tecnico in tecnicos)
                {
                    string esp = tecnico.Especialidad ?? "Sin Especialidad";
                    if (estadisticas.ContainsKey(esp))
                        estadisticas[esp]++;
                    else
                        estadisticas[esp] = 1;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener estadísticas por especialidad", ex.ToString(), "Tecnico");
                return new Dictionary<string, int>();
            }
        }

        public static Dictionary<int, int> ObtenerCargaTrabajo()
        {
            try
            {
                var tecnicos = TecnicoDAOType.ObtenerActivos() ?? new List<Tecnico>();
                var cargaTrabajo = new Dictionary<int, int>();

                foreach (var tecnico in tecnicos)
                {
                    int cantidadInspecciones = ContarInspeccionesAsignadas(tecnico.CodigoTecnico);
                    cargaTrabajo[tecnico.CodigoTecnico] = cantidadInspecciones;
                }

                return cargaTrabajo;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener carga de trabajo", ex.ToString(), "Tecnico");
                return new Dictionary<int, int>();
            }
        }

        // ======================================================
        // Validaciones (tolerantes al modelo)
        // ======================================================
        private static bool ValidarTecnico(Tecnico tecnico, out string mensaje)
        {
            mensaje = string.Empty;

            if (tecnico == null)
            {
                mensaje = "Los datos del técnico son requeridos.";
                return false;
            }

            // ✅ CodigoUsuario opcional en el modelo
            var codigoUsuario = GetIntProp(tecnico, "CodigoUsuario");
            if (!codigoUsuario.HasValue || codigoUsuario.Value <= 0)
            {
                mensaje = "El código de usuario es requerido (propiedad CodigoUsuario no encontrada o inválida).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(tecnico.Especialidad))
            {
                mensaje = "La especialidad es requerida.";
                return false;
            }

            var especialidadesPermitidas = new[]
            {
                "Estructuras", "Motores", "Aviónica", "Sistemas Eléctricos",
                "Sistemas Hidráulicos", "General", "NDT"
            };

            if (!especialidadesPermitidas.Contains(tecnico.Especialidad))
            {
                mensaje = $"Especialidad no válida. Especialidades permitidas: {string.Join(", ", especialidadesPermitidas)}";
                return false;
            }

            // ✅ Certificaciones opcional en el modelo
            var certs = GetStringProp(tecnico, "Certificaciones");
            if (!string.IsNullOrWhiteSpace(certs) && certs.Length > 500)
            {
                mensaje = "Las certificaciones no pueden exceder 500 caracteres.";
                return false;
            }

            // ✅ Años de experiencia opcional en el modelo
            var anios = GetIntProp(tecnico, "AniosExperiencia");
            if (anios.HasValue && anios.Value < 0)
            {
                mensaje = "Los años de experiencia no pueden ser negativos.";
                return false;
            }

            return true;
        }
    }
}
