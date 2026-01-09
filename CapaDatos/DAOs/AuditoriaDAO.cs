using System;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Auditoría usando PostgreSQL + Dapper
    /// </summary>
    public class AuditoriaDAO
    {
        private NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion(); // Asegúrate de tener esta clase
        }

        public void Registrar(Auditoria log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            if (log.Fecha == default)
                log.Fecha = DateTime.Now;

            const string sql = @"
                INSERT INTO aocr_tbauditoria
                (
                    entidad,
                    accion,
                    usuario,
                    fecha,
                    datos_previos,
                    datos_nuevos
                )
                VALUES
                (
                    @Entidad,
                    @Accion,
                    @Usuario,
                    @Fecha,
                    @DatosPrevios,
                    @DatosNuevos
                );";

            using (var con = CrearConexion())
            {
                con.Open();
                con.Execute(sql, log);
            }
        }
    }
}
