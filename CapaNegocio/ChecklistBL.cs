using System;
using System.Collections.Generic;
using CapaModelo;
using CapaDatos.DAOs;

namespace CapaNegocio
{
    public class ChecklistBL
    {
        #region Inserción

        public static bool Insertar(ChecklistItem item, out string mensaje)
        {
            mensaje = "";

            try
            {
                if (!Validar(item, out mensaje))
                    return false;

                ChecklistDAO.Insertar(item);

                mensaje = "Checklist registrado correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error al registrar checklist: " + ex.Message;
                return false;
            }
        }

        public static bool InsertarMasivo(
            List<ChecklistItem> items,
            int codigoSolicitud,
            out string mensaje)
        {
            mensaje = "";

            if (items == null || items.Count == 0)
            {
                mensaje = "No existen items de checklist.";
                return false;
            }

            try
            {
                foreach (var item in items)
                {
                    item.CodigoSolicitud = codigoSolicitud;
                    ChecklistDAO.Insertar(item);
                }

                mensaje = "Checklist guardado correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error al guardar checklist: " + ex.Message;
                return false;
            }
        }

        #endregion

        #region Validaciones

        private static bool Validar(ChecklistItem item, out string mensaje)
        {
            mensaje = "";

            if (item == null)
            {
                mensaje = "Item de checklist no válido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.Descripcion))
            {
                mensaje = "La descripción del checklist es obligatoria.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
