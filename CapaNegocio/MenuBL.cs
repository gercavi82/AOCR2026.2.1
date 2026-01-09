using System;
using System.Collections.Generic;
using System.Linq;
using CapaModelo;
using CapaDatos.DAOs;

// ✅ ALIAS EXPLÍCITOS
using MenuDAOType = CapaDatos.DAOs.MenuDAO;
using SubmenuDAOType = CapaDatos.DAOs.SubmenuDAO;

namespace CapaNegocio
{
    public class MenuBL
    {
        #region CRUD Básico

        public static bool Insertar(Menu menu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarMenu(menu, out mensaje))
                    return false;

                bool resultado = MenuDAOType.Insertar(menu);

                if (resultado)
                {
                    mensaje = "Menú creado exitosamente.";
                    LogBL.RegistrarInfo($"Menú creado: {menu.NombreMenu}", "Menu");
                }
                else
                {
                    mensaje = "Error al crear el menú.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar menú", ex.ToString(), "Menu");
                return false;
            }
        }

        public static bool Actualizar(Menu menu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarMenu(menu, out mensaje))
                    return false;

                bool resultado = MenuDAOType.Actualizar(menu);

                if (resultado)
                {
                    mensaje = "Menú actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Menú actualizado: {menu.NombreMenu}", "Menu");
                }
                else
                {
                    mensaje = "Error al actualizar el menú.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar menú", ex.ToString(), "Menu");
                return false;
            }
        }

        public static bool Eliminar(int idMenu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // ✅ Verificar submenús asociados
                var submenus = SubmenuDAOType.ObtenerPorMenu(idMenu);

                // ✅ FIX CS0019: usar Any() en lugar de Count > 0
                if (submenus != null && submenus.Any())
                {
                    mensaje = "No se puede eliminar el menú porque tiene submenús asociados.";
                    return false;
                }

                bool resultado = MenuDAOType.Eliminar(idMenu);

                if (resultado)
                {
                    mensaje = "Menú eliminado exitosamente.";
                    LogBL.RegistrarInfo($"Menú eliminado: ID {idMenu}", "Menu");
                }
                else
                {
                    mensaje = "Error al eliminar el menú.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar menú", ex.ToString(), "Menu");
                return false;
            }
        }

        public static Menu ObtenerPorId(int idMenu)
        {
            try
            {
                return MenuDAOType.ObtenerPorId(idMenu);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener menú", ex.ToString(), "Menu");
                return null;
            }
        }

        public static List<Menu> ObtenerTodos()
        {
            try
            {
                return MenuDAOType.ObtenerTodos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener menús", ex.ToString(), "Menu");
                return new List<Menu>();
            }
        }

        #endregion

        #region Métodos Específicos

        public static List<Menu> ObtenerMenusPorRol(int codigoRol)
        {
            try
            {
                return MenuDAOType.ObtenerMenusPorRol(codigoRol);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener menús por rol", ex.ToString(), "Menu");
                return new List<Menu>();
            }
        }

        public static Dictionary<Menu, List<Submenu>> ObtenerMenuCompletoPorRol(int codigoRol)
        {
            try
            {
                var resultado = new Dictionary<Menu, List<Submenu>>();

                var menus = MenuDAOType.ObtenerMenusPorRol(codigoRol) ?? new List<Menu>();
                var submenusRol = SubmenuDAOType.ObtenerPorRol(codigoRol) ?? new List<Submenu>();

                foreach (var menu in menus)
                {
                    var submenus = submenusRol
                        .Where(s => s.IdMenu == menu.IdMenu)
                        .ToList();

                    resultado[menu] = submenus;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener menú completo por rol", ex.ToString(), "Menu");
                return new Dictionary<Menu, List<Submenu>>();
            }
        }

        public static bool CambiarOrden(int idMenu, int nuevoOrden, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (nuevoOrden < 0)
                {
                    mensaje = "El orden no puede ser negativo.";
                    return false;
                }

                bool resultado = MenuDAOType.CambiarOrden(idMenu, nuevoOrden);

                if (resultado)
                {
                    mensaje = "Orden actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Orden de menú actualizado: ID {idMenu} -> Orden {nuevoOrden}", "Menu");
                }
                else
                {
                    mensaje = "Error al actualizar el orden.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al cambiar orden de menú", ex.ToString(), "Menu");
                return false;
            }
        }

        public static bool ReordenarMenus(List<int> idsMenus, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (idsMenus == null || idsMenus.Count == 0)
                {
                    mensaje = "No existen menús para reordenar.";
                    return false;
                }

                int orden = 1;
                foreach (var idMenu in idsMenus)
                {
                    MenuDAOType.CambiarOrden(idMenu, orden);
                    orden++;
                }

                mensaje = "Menús reordenados exitosamente.";
                LogBL.RegistrarInfo("Menús reordenados", "Menu");
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al reordenar menús", ex.ToString(), "Menu");
                return false;
            }
        }

        #endregion

        #region Validaciones

        private static bool ValidarMenu(Menu menu, out string mensaje)
        {
            mensaje = string.Empty;

            if (menu == null)
            {
                mensaje = "Los datos del menú son requeridos.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(menu.NombreMenu))
            {
                mensaje = "El nombre del menú es requerido.";
                return false;
            }

            if (menu.NombreMenu.Length > 100)
            {
                mensaje = "El nombre del menú no puede exceder 100 caracteres.";
                return false;
            }

            if (menu.Orden.HasValue && menu.Orden < 0)
            {
                mensaje = "El orden no puede ser negativo.";
                return false;
            }

            return true;
        }

        #endregion

        #region Utilidades

        public static List<Menu> ObtenerMenusOrdenados()
        {
            try
            {
                return (MenuDAOType.ObtenerTodos() ?? new List<Menu>())
                    .OrderBy(m => m.Orden ?? int.MaxValue)
                    .ThenBy(m => m.NombreMenu)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener menús ordenados", ex.ToString(), "Menu");
                return new List<Menu>();
            }
        }

        #endregion
    }
}
