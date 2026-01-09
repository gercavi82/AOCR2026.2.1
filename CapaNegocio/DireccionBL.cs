using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class DireccionBL
    {
        private readonly DireccionDAO _dao = new DireccionDAO();

        public List<Direccion> ObtenerTodos() => _dao.ObtenerTodos();

        public Direccion ObtenerPorId(int id)
        {
            if (id <= 0) throw new Exception("ID inválido");
            return _dao.ObtenerPorId(id);
        }

        public bool Crear(Direccion d, string usuario)
        {
            Validar(d);
            d.CreatedBy = usuario;
            return _dao.Crear(d) > 0;
        }

        public bool Actualizar(Direccion d, string usuario)
        {
            Validar(d);
            d.UpdatedBy = usuario;
            return _dao.Actualizar(d);
        }

        public bool Eliminar(int id, string usuario)
        {
            if (id <= 0) throw new Exception("ID inválido");
            return _dao.Eliminar(id, usuario);
        }

        private void Validar(Direccion d)
        {
            if (string.IsNullOrWhiteSpace(d.Calle)) throw new Exception("Calle es obligatoria");
            if (string.IsNullOrWhiteSpace(d.Ciudad)) throw new Exception("Ciudad es obligatoria");
            if (string.IsNullOrWhiteSpace(d.Provincia)) throw new Exception("Provincia es obligatoria");
            if (string.IsNullOrWhiteSpace(d.Pais)) throw new Exception("País es obligatorio");
        }
    }
}
