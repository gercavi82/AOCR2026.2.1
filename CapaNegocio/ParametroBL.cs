using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    /// <summary>
    /// Lógica de negocio de Parámetros
    /// MVC5 / .NET Framework 4.7.2
    /// </summary>
    public static class ParametroBL
    {
        private static readonly ParametroDAO _dao = new ParametroDAO();

        // ==============================
        // Listados
        // ==============================
        public static List<Parametro> ListarTodos()
        {
            return _dao.ListarTodos();
        }

        public static List<Parametro> ListarActivos()
        {
            return _dao.ListarActivos();
        }

        // ==============================
        // Obtener
        // ==============================
        public static Parametro ObtenerPorId(int id)
        {
            return _dao.ObtenerPorId(id);
        }

        public static Parametro ObtenerPorClave(string clave)
        {
            return _dao.ObtenerPorClave(clave);
        }

        // ==============================
        // Crear
        // ==============================
        public static bool Crear(Parametro p, int codigoUsuario, out string mensaje)
        {
            mensaje = string.Empty;

            if (p == null)
            {
                mensaje = "Parámetro inválido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(p.Clave))
            {
                mensaje = "La clave es obligatoria.";
                return false;
            }

            // Evitar duplicados simples
            var existe = _dao.ObtenerPorClave(p.Clave);
            if (existe != null && existe.DeletedAt == null)
            {
                mensaje = "Ya existe un parámetro con esa clave.";
                return false;
            }

            p.Activo = true;

            bool ok = _dao.Crear(p, codigoUsuario);
            mensaje = ok ? "Parámetro creado correctamente." : "No se pudo crear el parámetro.";
            return ok;
        }

        // ==============================
        // Actualizar
        // ==============================
        public static bool Actualizar(Parametro p, int codigoUsuario, out string mensaje)
        {
            mensaje = string.Empty;

            if (p == null || p.CodigoParametro <= 0)
            {
                mensaje = "Parámetro inválido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(p.Clave))
            {
                mensaje = "La clave es obligatoria.";
                return false;
            }

            bool ok = _dao.Actualizar(p, codigoUsuario);
            mensaje = ok ? "Parámetro actualizado correctamente." : "No se pudo actualizar el parámetro.";
            return ok;
        }

        // ==============================
        // Eliminar Soft
        // ==============================
        public static bool EliminarSoft(int id, int codigoUsuario, out string mensaje)
        {
            mensaje = string.Empty;

            bool ok = _dao.EliminarSoft(id, codigoUsuario);
            mensaje = ok ? "Parámetro eliminado correctamente." : "No se pudo eliminar el parámetro.";
            return ok;
        }
    }
}
