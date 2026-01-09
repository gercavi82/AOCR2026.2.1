using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class HallazgoBL
    {
        private readonly HallazgoDAO _hallazgoDAO;
        private readonly InspeccionDAO _inspeccionDAO;

        public HallazgoBL()
        {
            _hallazgoDAO = new HallazgoDAO();
            _inspeccionDAO = new InspeccionDAO();
        }

        // ============================================================
        // LISTAR POR INSPECCIÓN
        // ============================================================
        public List<Hallazgo> ObtenerPorInspeccion(int idInspeccion)
        {
            if (idInspeccion <= 0)
                throw new ArgumentException("ID de inspección inválido");

            return _hallazgoDAO.ObtenerPorInspeccion(idInspeccion);
        }

        // ============================================================
        // OBTENER POR ID
        // ============================================================
        public Hallazgo ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido");

            return _hallazgoDAO.ObtenerPorId(id);
        }

        // ============================================================
        // CREAR HALLAZGO
        // ============================================================
        public bool Crear(Hallazgo h, string usuario)
        {
            ValidarHallazgo(h);

            h.FechaDeteccion = DateTime.Now;
            h.CreatedAt = DateTime.Now;
            h.CreatedBy = usuario;
            h.Estado = "ABIERTO";

            return _hallazgoDAO.Crear(h) > 0;
        }

        // ============================================================
        // ACTUALIZAR HALLAZGO
        // ============================================================
        public bool Actualizar(Hallazgo h, string usuario)
        {
            if (h.CodigoHallazgo <= 0)
                throw new Exception("ID inválido para actualizar.");

            ValidarHallazgo(h);

            h.UpdatedAt = DateTime.Now;
            h.UpdatedBy = usuario;

            return _hallazgoDAO.Actualizar(h) > 0;
        }

        // ============================================================
        // CERRAR HALLAZGO
        // ============================================================
        public bool CerrarHallazgo(int idHallazgo, string usuario)
        {
            var h = _hallazgoDAO.ObtenerPorId(idHallazgo);

            if (h == null)
                throw new Exception("Hallazgo no encontrado");

            if (h.Estado == "CERRADO")
                throw new Exception("El hallazgo ya está cerrado");

            h.Estado = "CERRADO";
            h.FechaCierre = DateTime.Now;
            h.UpdatedAt = DateTime.Now;
            h.UpdatedBy = usuario;

            return _hallazgoDAO.Cerrar(h) > 0;
        }

        // ============================================================
        // ELIMINAR (SOFT DELETE)
        // ============================================================
        public bool Eliminar(int idHallazgo, string usuario)
        {
            var h = _hallazgoDAO.ObtenerPorId(idHallazgo);

            if (h == null)
                throw new Exception("Hallazgo no encontrado");

            return _hallazgoDAO.Eliminar(idHallazgo, usuario) > 0;
        }

        // ============================================================
        // VALIDACIONES
        // ============================================================
        private void ValidarHallazgo(Hallazgo h)
        {
            if (h == null)
                throw new Exception("Datos inválidos.");

            if (h.CodigoInspeccion <= 0)
                throw new Exception("Debe asignarse a una inspección.");

            if (string.IsNullOrWhiteSpace(h.Descripcion))
                throw new Exception("Debe ingresar una descripción del hallazgo.");

            if (string.IsNullOrWhiteSpace(h.Criticidad))
                throw new Exception("Debe especificar criticidad (ALTA / MEDIA / BAJA).");
        }
    }
}
