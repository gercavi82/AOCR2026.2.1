using System;
using System.Collections.Generic;
using System.Linq;
using CapaModelo;

// ✅ IMPORTA EL NAMESPACE CORRECTO DE TUS DAOs
using CapaDatos.DAOs;

// ✅ ALIAS EXPLÍCITO PARA EVITAR CS0103
using SubmenuDAOType = CapaDatos.DAOs.SubmenuDAO;

namespace CapaNegocio
{
    public class SubmenuBL
    {
        #region CRUD Básico

        public static bool Insertar(Submenu submenu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarSubmenu(submenu, out mensaje))
                    return false;

                bool resultado = SubmenuDAOType.Insertar(submenu);

                if (resultado)
                {
                    mensaje = "Submenú creado exitosamente.";
                    LogBL.RegistrarInfo($"Submenú creado: {submenu.NombreSubmenu}", "Submenu");
                }
                else
                {
                    mensaje = "Error al crear el submenú.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al insertar submenú", ex.ToString(), "Submenu");
                return false;
            }
        }

        public static bool Actualizar(Submenu submenu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (!ValidarSubmenu(submenu, out mensaje))
                    return false;

                bool resultado = SubmenuDAOType.Actualizar(submenu);

                if (resultado)
                {
                    mensaje = "Submenú actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Submenú actualizado: {submenu.NombreSubmenu}", "Submenu");
                }
                else
                {
                    mensaje = "Error al actualizar el submenú.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al actualizar submenú", ex.ToString(), "Submenu");
                return false;
            }
        }

        public static bool Eliminar(int idSubmenu, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                bool resultado = SubmenuDAOType.Eliminar(idSubmenu);

                if (resultado)
                {
                    mensaje = "Submenú eliminado exitosamente.";
                    LogBL.RegistrarInfo($"Submenú eliminado: ID {idSubmenu}", "Submenu");
                }
                else
                {
                    mensaje = "Error al eliminar el submenú.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al eliminar submenú", ex.ToString(), "Submenu");
                return false;
            }
        }

        public static Submenu ObtenerPorId(int idSubmenu)
        {
            try
            {
                return SubmenuDAOType.ObtenerPorId(idSubmenu);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener submenú", ex.ToString(), "Submenu");
                return null;
            }
        }

        public static List<Submenu> ObtenerTodos()
        {
            try
            {
                return SubmenuDAOType.ObtenerTodos();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener submenús", ex.ToString(), "Submenu");
                return new List<Submenu>();
            }
        }

        #endregion

        #region Métodos Específicos

        public static List<Submenu> ObtenerPorMenu(int idMenu)
        {
            try
            {
                return SubmenuDAOType.ObtenerPorMenu(idMenu);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener submenús por menú", ex.ToString(), "Submenu");
                return new List<Submenu>();
            }
        }

        public static List<Submenu> ObtenerPorRol(int codigoRol)
        {
            try
            {
                return SubmenuDAOType.ObtenerPorRol(codigoRol);
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener submenús por rol", ex.ToString(), "Submenu");
                return new List<Submenu>();
            }
        }

        public static bool CambiarOrden(int idSubmenu, int nuevoOrden, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (nuevoOrden < 0)
                {
                    mensaje = "El orden no puede ser negativo.";
                    return false;
                }

                bool resultado = SubmenuDAOType.CambiarOrden(idSubmenu, nuevoOrden);

                if (resultado)
                {
                    mensaje = "Orden actualizado exitosamente.";
                    LogBL.RegistrarInfo($"Orden de submenú actualizado: ID {idSubmenu} -> Orden {nuevoOrden}", "Submenu");
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
                LogBL.RegistrarError("Error al cambiar orden de submenú", ex.ToString(), "Submenu");
                return false;
            }
        }

        public static bool ReordenarSubmenus(int idMenu, List<int> idsSubmenus, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                if (idsSubmenus == null || idsSubmenus.Count == 0)
                {
                    mensaje = "No existen submenús para reordenar.";
                    return false;
                }

                int orden = 1;
                foreach (var idSubmenu in idsSubmenus)
                {
                    SubmenuDAOType.CambiarOrden(idSubmenu, orden);
                    orden++;
                }

                mensaje = "Submenús reordenados exitosamente.";
                LogBL.RegistrarInfo($"Submenús reordenados para menú {idMenu}", "Submenu");
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
                LogBL.RegistrarError("Error al reordenar submenús", ex.ToString(), "Submenu");
                return false;
            }
        }

        #endregion

        #region Validaciones

        private static bool ValidarSubmenu(Submenu submenu, out string mensaje)
        {
            mensaje = string.Empty;

            if (submenu == null)
            {
                mensaje = "Los datos del submenú son requeridos.";
                return false;
            }

            if (!submenu.IdMenu.HasValue || submenu.IdMenu <= 0)
            {
                mensaje = "El menú padre es requerido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(submenu.NombreSubmenu))
            {
                mensaje = "El nombre del submenú es requerido.";
                return false;
            }

            if (submenu.NombreSubmenu.Length > 100)
            {
                mensaje = "El nombre del submenú no puede exceder 100 caracteres.";
                return false;
            }

            if (submenu.Orden.HasValue && submenu.Orden < 0)
            {
                mensaje = "El orden no puede ser negativo.";
                return false;
            }

            return true;
        }

        #endregion

        #region Utilidades

        public static List<Submenu> ObtenerSubmenusOrdenados(int idMenu)
        {
            try
            {
                return SubmenuDAOType.ObtenerPorMenu(idMenu)
                    .OrderBy(s => s.Orden ?? int.MaxValue)
                    .ThenBy(s => s.NombreSubmenu)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogBL.RegistrarError("Error al obtener submenús ordenados", ex.ToString(), "Submenu");
                return new List<Submenu>();
            }
        }

        #endregion
    }
}
