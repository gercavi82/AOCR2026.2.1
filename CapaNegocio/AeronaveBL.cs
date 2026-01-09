using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class AeronaveBL
    {
        // El método donde probablemente tienes el error (Línea 90)
        public bool ValidarYGuardar(Aeronave nave, out string mensaje)
        {
            mensaje = "";

            if (nave == null) { mensaje = "Datos nulos"; return false; }

            // ✅ CORRECCIÓN: Cambiar 'Marca' por 'Fabricante'
            if (string.IsNullOrWhiteSpace(nave.Fabricante))
            {
                mensaje = "El fabricante es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(nave.Matricula))
            {
                mensaje = "La matrícula es obligatoria.";
                return false;
            }

            // ✅ CORRECCIÓN: Cambiar 'NumeroSerie' por 'Serie'
            if (string.IsNullOrWhiteSpace(nave.Serie))
            {
                mensaje = "El número de serie es obligatorio.";
                return false;
            }

            try
            {
                AeronaveDAO.Insertar(nave);
                mensaje = "Aeronave registrada correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error al insertar: " + ex.Message;
                return false;
            }
        }

        public List<Aeronave> ObtenerPorSolicitud(int solicitudId)
        {
            if (solicitudId <= 0) return new List<Aeronave>();
            return AeronaveDAO.ObtenerPorSolicitud(solicitudId);
        }
    }
}