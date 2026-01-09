using System;
using System.Collections.Generic;
using CapaDatos.DAOs;
using CapaModelo;

namespace CapaNegocio
{
    public class InspeccionBL
    {
        // ======================================================
        // LISTAR POR SOLICITUD
        // ======================================================
        public static List<Inspeccion> ObtenerPorSolicitud(int idSolicitud)
        {
            if (idSolicitud <= 0)
                throw new Exception("El código de solicitud es inválido.");

            // 👉 En tu DAO estos métodos están estáticos, por eso
            // los llamamos con el nombre del tipo, no con instancia.
            return InspeccionDAO.ObtenerPorSolicitud(idSolicitud);
        }

        // ======================================================
        // CREAR INSPECCIÓN
        // ======================================================
        // Antes: Crear(Inspeccion i, string usuario)
        // Ahora: usamos int codigoUsuario porque CreatedBy es int?
        public static bool Crear(Inspeccion i, int codigoUsuario)
        {
            if (i == null)
                throw new Exception("Datos de inspección inválidos.");

            if (codigoUsuario <= 0)
                throw new Exception("Código de usuario inválido.");

            // Campos de auditoría
            i.CreatedAt = DateTime.Now;
            i.CreatedBy = codigoUsuario;   // ✅ int → int?, ok

            // ⚠ Tu modelo Inspeccion NO tiene la propiedad Resultado,
            // por eso quitamos esta línea que causaba CS1061:
            // i.Resultado = "EN_PROCESO";

            // InspeccionDAO.Crear es estático
            return InspeccionDAO.Crear(i) > 0;
        }

        // ======================================================
        // GUARDAR INFORME DE INSPECCIÓN
        // ======================================================
        // Antes: GuardarInforme(int idInspeccion, string informe, string usuario)
        // Cambiamos usuario → codigoUsuario (int) para que coincida con el DAO.
        public static bool GuardarInforme(int idInspeccion, string informe, int codigoUsuario)
        {
            if (idInspeccion <= 0)
                throw new Exception("ID de inspección inválido.");

            if (codigoUsuario <= 0)
                throw new Exception("Código de usuario inválido.");

            if (string.IsNullOrWhiteSpace(informe))
                throw new Exception("El informe no puede estar vacío.");

            // En el DAO: GuardarInforme(int idInspeccion, string informe, int codigoUsuario)
            return InspeccionDAO.GuardarInforme(idInspeccion, informe, codigoUsuario) > 0;
        }

        // ======================================================
        // CERRAR INSPECCIÓN (APROBADO / RECHAZADO)
        // ======================================================
        // Antes: CerrarInspeccion(int idInspeccion, string resultado, string usuario)
        // Cambiamos usuario → codigoUsuario (int)
        public static bool CerrarInspeccion(int idInspeccion, string resultado, int codigoUsuario)
        {
            if (idInspeccion <= 0)
                throw new Exception("ID de inspección inválido.");

            if (codigoUsuario <= 0)
                throw new Exception("Código de usuario inválido.");

            if (resultado != "APROBADO" && resultado != "RECHAZADO")
                throw new Exception("Resultado inválido. Debe ser 'APROBADO' o 'RECHAZADO'.");

            // En el DAO: CerrarInspeccion(int idInspeccion, string resultado, int codigoUsuario)
            return InspeccionDAO.CerrarInspeccion(idInspeccion, resultado, codigoUsuario) > 0;
        }
    }
}
