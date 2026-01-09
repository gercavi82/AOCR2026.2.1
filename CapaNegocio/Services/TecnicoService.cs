using System;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio.Services
{
    public class TecnicoService
    {
        private readonly InspeccionDAO _inspeccionDAO;
        private readonly HallazgoDAO _hallazgoDAO;

        public TecnicoService()
        {
            _inspeccionDAO = new InspeccionDAO();
            _hallazgoDAO = new HallazgoDAO();
        }

        public bool AsignarInspector(int solicitudId, string inspector, DateTime fecha, string tipoInspeccion, string lugar)
        {
            var inspeccion = new Inspeccion
            {
                SolicitudId = solicitudId,
                InspectorAsignado = inspector,
                FechaProgramada = fecha,
                TipoInspeccion = tipoInspeccion,
                Lugar = lugar,
                Estado = "PROGRAMADA"
            };

            return _inspeccionDAO.Crear(inspeccion);
        }

        public bool ProgramarInspeccion(int inspeccionId, DateTime fecha, string lugar)
        {
            var insp = _inspeccionDAO.ObtenerPorId(inspeccionId);
            if (insp == null) return false;

            insp.FechaProgramada = fecha;
            insp.Lugar = lugar;
            insp.Estado = "PROGRAMADA";

            return _inspeccionDAO.Actualizar(insp);
        }

        public bool RegistrarListaChequeo(int inspeccionId, string[] items, bool[] cumple, string[] obs)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var h = new Hallazgo
                {
                    InspeccionId = inspeccionId,
                    Item = items[i],
                    Cumple = cumple[i],
                    Observacion = obs[i]
                };

                _hallazgoDAO.Agregar(h);
            }

            return true;
        }

        public bool FinalizarInspeccion(int inspeccionId, bool aprobada, string conc, string recom)
        {
            var insp = _inspeccionDAO.ObtenerPorId(inspeccionId);
            if (insp == null) return false;

            insp.Aprobada = aprobada;
            insp.Conclusiones = conc;
            insp.Recomendaciones = recom;
            insp.FechaInforme = DateTime.Now;
            insp.Estado = "FINALIZADA";

            return _inspeccionDAO.Actualizar(insp);
        }

        public Inspeccion ObtenerInspeccion(int id)
        {
            return _inspeccionDAO.ObtenerInspeccionCompleta(id);
        }
    }
}
