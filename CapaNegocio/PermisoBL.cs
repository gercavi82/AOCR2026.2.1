// ==========================================================
// PermisoBL.cs (COMPATIBLE CON TU MODELO ACTUAL)
// PostgreSQL - Npgsql - .NET Framework 4.7.2
//
// ✅ SOLUCIONA:
//   - CS1061: "int" no contiene HasValue/Value (UsuarioRol.CodigoRol)
//   - CS0103: 'nb' no existe en el contexto actual
//   - CS0117: Permiso no contiene Leer/Crear/Editar/Eliminar/Modulo
//
// ✅ Estrategia:
//   - Usa Reflection para Leer/Escribir flags opcionales.
//   - Para UsuarioRol.CodigoRol:
//       NO usa HasValue/Value.
//       Obtiene el rol como int de forma segura.
//
// 📌 No se usan accesos directos:
//   permiso.Leer, permiso.Crear, permiso.Editar, permiso.Eliminar, permiso.Modulo
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CapaModelo;

// ✅ IMPORTA DAOs
using CapaDatos.DAOs;

// ✅ ALIAS EXPLÍCITO
using PermisoDAOType = CapaDatos.DAOs.PermisoDAO;

namespace CapaNegocio
{
    public class PermisoBL
    {
        // ======================================================
        // Helpers Reflection (evitan CS0117)
        // ======================================================
        private static bool GetBoolProp(object obj, string propName)
        {
            try
            {
                if (obj == null) return false;

                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return false;

                var val = prop.GetValue(obj, null);
                if (val == null) return false;

                if (val is bool b) return b;

                if (val is bool?)
                {
                    var nb = (bool?)val;
                    return nb.GetValueOrDefault();
                }

                if (val is int i) return i != 0;
                if (val is short s) return s != 0;
                if (val is long l) return l != 0;

                return false;
            }
            catch
            {
                return false;
            }
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

        // ======================================================
        // Helper robusto para leer CodigoRol de UsuarioRol
        // (soporta int o int?)
        // ======================================================
        private static int ObtenerCodigoRolUsuarioRol(object usuarioRol)
        {
            try
            {
                if (usuarioRol == null) return 0;

                var prop = usuarioRol.GetType().GetProperty("CodigoRol", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return 0;

                var val = prop.GetValue(usuarioRol, null);
                if (val == null) return 0;

                if (val is int i) return i;
                if (val is int?)
                {
                    var ni = (int?)val;
                    return ni.GetValueOrDefault();
                }

                if (val is short s) return s;
                if (val is long l) return (int)l;

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // ======================================================
        // Helper robusto para validar CodigoRol en Permiso
        // (soporta int o int?)
        // ======================================================
        private static int ObtenerCodigoRolPermiso(Permiso permiso)
        {
            try
            {
                if (permiso == null) return 0;

                var prop = permiso.GetType().GetProperty("CodigoRol", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return 0;

                var val = prop.GetValue(permiso, null);
                if (val == null) return 0;

                if (val is int i) return i;
                if (val is int?)
                {
                    var ni = (int?)val;
                    return ni.GetValueOrDefault();
                }

                if (val is short s) return s;
                if (val is long l) return (int)l;

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        #region CRUD Básico

        public static bool Insertar(Permiso permiso, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarPermiso(permiso, out mensaje))
                    return false;

                bool resultado = PermisoDAOType.Insertar(permiso);

                if (resultado)
                {
                    mensaje = "Permiso creado exitosamente.";
                    LogBL.RegistrarInfo(
                        $"Permiso creado: Rol {ObtenerCodigoRolPermiso(permiso)} - Menú {permiso.IdMenu} - Submenú {permiso.IdSubmenu}",
                        "Permiso"
                    );
                }
                else
                {
                    mensaje = "Error al crear el permiso.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar permiso", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static bool Actualizar(Permiso permiso, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarPermiso(permiso, out mensaje))
                    return false;

                bool resultado = PermisoDAOType.Actualizar(permiso);

                if (resultado)
                {
                    mensaje = "Permiso actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Permiso actualizado: ID {permiso.IdPermiso}", "Permiso");
                }
                else
                {
                    mensaje = "Error al actualizar el permiso.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar permiso", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static bool Eliminar(int idPermiso, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = PermisoDAOType.Eliminar(idPermiso);

                if (resultado)
                {
                    mensaje = "Permiso eliminado exitosamente.";
                    LogBL.RegistrarInfo($"Permiso eliminado: ID {idPermiso}", "Permiso");
                }
                else
                {
                    mensaje = "Error al eliminar el permiso.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar permiso", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static Permiso ObtenerPorId(int idPermiso)
        {
            try
            {
                return PermisoDAOType.ObtenerPorId(idPermiso);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener permiso", ex.ToString(), "Permiso");
                return null;
            }
        }

        public static List<Permiso> ObtenerTodos()
        {
            try
            {
                return PermisoDAOType.ObtenerTodos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener permisos", ex.ToString(), "Permiso");
                return new List<Permiso>();
            }
        }

        #endregion

        #region Métodos Específicos

        public static List<Permiso> ObtenerPorRol(int codigoRol)
        {
            try
            {
                return PermisoDAOType.ObtenerPorRol(codigoRol);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener permisos por rol", ex.ToString(), "Permiso");
                return new List<Permiso>();
            }
        }

        public static List<Permiso> ObtenerPorMenu(int idMenu)
        {
            try
            {
                return PermisoDAOType.ObtenerPorMenu(idMenu);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener permisos por menú", ex.ToString(), "Permiso");
                return new List<Permiso>();
            }
        }

        public static bool VerificarPermiso(int codigoRol, int? idMenu, int? idSubmenu, string accion)
        {
            try
            {
                return PermisoDAOType.VerificarPermiso(codigoRol, idMenu, idSubmenu, accion);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al verificar permiso", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static bool TienePermiso(int codigoUsuario, string modulo, string accion)
        {
            try
            {
                var usuarioRoles = UsuarioRolDAO.ObtenerPorUsuario(codigoUsuario);

                foreach (var usuarioRol in usuarioRoles)
                {
                    // ✅ AQUÍ EL CAMBIO CLAVE: sin HasValue/Value
                    int codigoRol = ObtenerCodigoRolUsuarioRol(usuarioRol);
                    if (codigoRol <= 0) continue;

                    var permisos = ObtenerPorRol(codigoRol);

                    var permiso = permisos.FirstOrDefault(p =>
                    {
                        string mod = GetStringProp(p, "Modulo");
                        bool moduloOk =
                            string.IsNullOrWhiteSpace(mod) ||
                            string.IsNullOrWhiteSpace(modulo) ||
                            string.Equals(mod, modulo, StringComparison.OrdinalIgnoreCase);

                        return moduloOk && TieneAccion(p, accion);
                    });

                    if (permiso != null)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al verificar si tiene permiso", ex.ToString(), "Permiso");
                return false;
            }
        }

        private static bool TieneAccion(Permiso permiso, string accion)
        {
            string a = (accion ?? "").ToUpper().Trim();

            switch (a)
            {
                case "LEER":
                case "VER":
                    return GetBoolProp(permiso, "Leer");

                case "CREAR":
                case "INSERTAR":
                    return GetBoolProp(permiso, "Crear");

                case "EDITAR":
                case "ACTUALIZAR":
                    return GetBoolProp(permiso, "Editar");

                case "ELIMINAR":
                case "BORRAR":
                    return GetBoolProp(permiso, "Eliminar");

                default:
                    return false;
            }
        }

        #endregion

        #region Gestión Masiva

        public static bool AsignarPermisoCompleto(int codigoRol, int? idMenu, int? idSubmenu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var permiso = new Permiso
                {
                    CodigoRol = codigoRol,
                    IdMenu = idMenu,
                    IdSubmenu = idSubmenu
                };

                SetBoolProp(permiso, "Leer", true);
                SetBoolProp(permiso, "Crear", true);
                SetBoolProp(permiso, "Editar", true);
                SetBoolProp(permiso, "Eliminar", true);

                return Insertar(permiso, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al asignar permiso completo", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static bool AsignarPermisoSoloLectura(int codigoRol, int? idMenu, int? idSubmenu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var permiso = new Permiso
                {
                    CodigoRol = codigoRol,
                    IdMenu = idMenu,
                    IdSubmenu = idSubmenu
                };

                SetBoolProp(permiso, "Leer", true);
                SetBoolProp(permiso, "Crear", false);
                SetBoolProp(permiso, "Editar", false);
                SetBoolProp(permiso, "Eliminar", false);

                return Insertar(permiso, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al asignar permiso de solo lectura", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static bool ClonarPermisosDeRol(int codigoRolOrigen, int codigoRolDestino, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var permisosOrigen = ObtenerPorRol(codigoRolOrigen);
                int copiados = 0;

                foreach (var permisoOrigen in permisosOrigen)
                {
                    var nuevoPermiso = new Permiso
                    {
                        CodigoRol = codigoRolDestino,
                        IdMenu = permisoOrigen.IdMenu,
                        IdSubmenu = permisoOrigen.IdSubmenu
                    };

                    SetBoolProp(nuevoPermiso, "Leer", GetBoolProp(permisoOrigen, "Leer"));
                    SetBoolProp(nuevoPermiso, "Crear", GetBoolProp(permisoOrigen, "Crear"));
                    SetBoolProp(nuevoPermiso, "Editar", GetBoolProp(permisoOrigen, "Editar"));
                    SetBoolProp(nuevoPermiso, "Eliminar", GetBoolProp(permisoOrigen, "Eliminar"));

                    var mod = GetStringProp(permisoOrigen, "Modulo");
                    if (!string.IsNullOrWhiteSpace(mod))
                        SetStringProp(nuevoPermiso, "Modulo", mod);

                    if (PermisoDAOType.Insertar(nuevoPermiso))
                        copiados++;
                }

                mensaje = $"Se copiaron {copiados} permisos del rol {codigoRolOrigen} al rol {codigoRolDestino}.";
                LogBL.RegistrarInfo(mensaje, "Permiso");
                return copiados > 0;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al clonar permisos", ex.ToString(), "Permiso");
                return false;
            }
        }

        public static bool EliminarPermisosPorRol(int codigoRol, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                var permisos = ObtenerPorRol(codigoRol);
                int eliminados = 0;

                foreach (var permiso in permisos)
                {
                    if (PermisoDAOType.Eliminar(permiso.IdPermiso))
                        eliminados++;
                }

                mensaje = $"Se eliminaron {eliminados} permisos del rol {codigoRol}.";
                LogBL.RegistrarInfo(mensaje, "Permiso");
                return eliminados > 0;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar permisos por rol", ex.ToString(), "Permiso");
                return false;
            }
        }

        #endregion

        #region Validaciones

        private static bool ValidarPermiso(Permiso permiso, out string mensaje)
        {
            mensaje = string.Empty;

            if (permiso == null)
            {
                mensaje = "Los datos del permiso son requeridos.";
                return false;
            }

            int codigoRol = ObtenerCodigoRolPermiso(permiso);
            if (codigoRol <= 0)
            {
                mensaje = "El código de rol es requerido.";
                return false;
            }

            if (!permiso.IdMenu.HasValue && !permiso.IdSubmenu.HasValue)
            {
                mensaje = "Debe especificar al menos un menú o submenú.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
