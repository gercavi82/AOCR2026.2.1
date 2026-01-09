using System.Collections.Generic;
using System.Linq;
using CapaDatos.DAOs; // Asegúrate de que este namespace sea correcto según tu proyecto
using CapaModelo;

namespace CapaNegocio
{
    public static class RolBL
    {
        public static List<Rol> ObtenerTodos()
        {
            return RolDAO.ObtenerTodos();
        }

        public static List<Rol> ObtenerActivos()
        {
            // Filtramos en memoria usando LINQ
            return RolDAO.ObtenerTodos()
                          .Where(r => r.Activo)
                          .ToList();
        }

        public static Rol ObtenerPorId(int id)
        {
            return RolDAO.ObtenerPorId(id);
        }

        // Métodos de acción (Insertar, Actualizar, Eliminar)
        public static bool Insertar(Rol rol, out string mensaje) => RolDAO.Insertar(rol, out mensaje);
        public static bool Actualizar(Rol rol, out string mensaje) => RolDAO.Actualizar(rol, out mensaje);
        public static bool Eliminar(int id, out string mensaje) => RolDAO.Eliminar(id, out mensaje);

        // =======================================================
        // AGREGAR ESTE MÉTODO PARA CORREGIR EL ERROR CS0117
        // =======================================================
        public static bool CambiarEstado(int id, bool activo, out string mensaje)
            => RolDAO.CambiarEstado(id, activo, out mensaje);
    }
}