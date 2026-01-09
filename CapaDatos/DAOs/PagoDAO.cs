using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Pago usando Npgsql + PostgreSQL
    /// Compatible con .NET Framework 4.7.2
    /// 
    /// Tabla sugerida: aocr_tbpago
    /// Columnas mínimas:
    ///   codigopago        INT PK
    ///   codigosolicitud   INT FK
    ///   fechapago         TIMESTAMP NULL
    ///   estado            VARCHAR
    ///   numerotransaccion VARCHAR NULL
    ///   montopago         NUMERIC NULL
    /// </summary>
    public class PagoDAO
    {
        // ==========================================
        // Conexión reutilizando tu ConexionDAO
        // ==========================================
        private NpgsqlConnection CrearConexion()
        {
            return ConexionDAO.CrearConexion();
        }

        // ==========================================
        // Helper: verificar si existe una columna
        // ==========================================
        private static bool TieneColumna(IDataRecord r, string nombre)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), nombre, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ==========================================
        // Mapeo a modelo Pago
        // (solo propiedades que sabemos que existen)
        // ==========================================
        private static Pago Map(IDataRecord r)
        {
            var p = new Pago();

            if (TieneColumna(r, "codigopago") && r["codigopago"] != DBNull.Value)
                p.CodigoPago = Convert.ToInt32(r["codigopago"]);

            // 🔧 AQUÍ ESTABA EL PROBLEMA (int? -> int)
            if (TieneColumna(r, "codigosolicitud"))
            {
                if (r["codigosolicitud"] != DBNull.Value)
                    p.CodigoSolicitud = Convert.ToInt32(r["codigosolicitud"]);
                else
                    p.CodigoSolicitud = 0; // o algún valor por defecto
            }

            if (TieneColumna(r, "fechapago") && r["fechapago"] != DBNull.Value)
                p.FechaPago = (DateTime?)Convert.ToDateTime(r["fechapago"]);

            if (TieneColumna(r, "estado") && r["estado"] != DBNull.Value)
                p.Estado = r["estado"].ToString();

            // Si tu modelo Pago tiene más propiedades (NumeroTransaccion, Monto, etc.),
            // aquí puedes agregarlas sin problema, por ejemplo:
            //
            // if (TieneColumna(r, "numerotransaccion") && r["numerotransaccion"] != DBNull.Value)
            //     p.NumeroTransaccion = r["numerotransaccion"].ToString();
            //
            // if (TieneColumna(r, "montopago") && r["montopago"] != DBNull.Value)
            //     p.Monto = (decimal?)Convert.ToDecimal(r["montopago"]);

            return p;
        }

        // ==========================================
        // Obtener POR ID
        // (ya lo usas en PagoBL.ObtenerPorId)
        // ==========================================
        public Pago ObtenerPorId(int codigoPago)
        {
            const string sql = @"
                SELECT codigopago, codigosolicitud, fechapago, estado
                FROM aocr_tbpago
                WHERE codigopago = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoPago);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                        return Map(rd);
                }
            }

            return null;
        }

        // ==========================================
        // Obtener POR SOLICITUD
        // (corrige CS1061 en PagoBL.ObtenerPorSolicitud)
        // ==========================================
        public List<Pago> ObtenerPorSolicitud(int codigoSolicitud)
        {
            var lista = new List<Pago>();

            const string sql = @"
                SELECT codigopago, codigosolicitud, fechapago, estado
                FROM aocr_tbpago
                WHERE codigosolicitud = @sol
                ORDER BY fechapago DESC, codigopago DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@sol", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(Map(rd));
                }
            }

            return lista;
        }

        // ==========================================
        // INSERTAR
        // (corrige CS1061 en PagoBL.Registrar)
        // ==========================================
        public bool Insertar(Pago pago)
        {
            const string sql = @"
                INSERT INTO aocr_tbpago
                (codigosolicitud, fechapago, estado)
                VALUES
                (@solicitud, @fechapago, @estado);";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@solicitud",
                    (object)pago.CodigoSolicitud);
                cmd.Parameters.AddWithValue("@fechapago",
                    (object)pago.FechaPago ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@estado",
                    (object)pago.Estado ?? DBNull.Value);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==========================================
        // ACTUALIZAR
        // (ya lo usas en PagoBL.Actualizar)
        // ==========================================
        public bool Actualizar(Pago pago)
        {
            const string sql = @"
                UPDATE aocr_tbpago
                SET codigosolicitud = @solicitud,
                    fechapago = @fechapago,
                    estado = @estado
                WHERE codigopago = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@solicitud",
                    (object)pago.CodigoSolicitud);
                cmd.Parameters.AddWithValue("@fechapago",
                    (object)pago.FechaPago ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@estado",
                    (object)pago.Estado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", pago.CodigoPago);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==========================================
        // ExistePorNumeroTransaccion
        // (corrige CS1061 en PagoBL.ExistePorNumeroTransaccion)
        // ==========================================
        public bool ExistePorNumeroTransaccion(string numeroTransaccion)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM aocr_tbpago
                WHERE numerotransaccion = @num;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@num", (object)numeroTransaccion ?? DBNull.Value);
                cn.Open();

                var n = Convert.ToInt32(cmd.ExecuteScalar());
                return n > 0;
            }
        }

        // ==========================================
        // ObtenerPagosValidadosHoy
        // (corrige CS1061 en PagoBL.ObtenerPagosValidadosHoy)
        // ==========================================
        public List<Pago> ObtenerPagosValidadosHoy()
        {
            var lista = new List<Pago>();

            const string sql = @"
                SELECT codigopago, codigosolicitud, fechapago, estado
                FROM aocr_tbpago
                WHERE estado = 'VALIDADO'
                  AND DATE(fechapago) = CURRENT_DATE
                ORDER BY fechapago DESC, codigopago DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(Map(rd));
                }
            }

            return lista;
        }

        // ==========================================
        // ObtenerMontoRecaudadoMes
        // (corrige CS1061 en PagoBL.ObtenerMontoRecaudadoMes)
        //
        // Usa columna montopago; si en tu BD se llama distinto
        // (ej. valortotal, totalpago), solo cambia el nombre.
        // ==========================================
        public decimal ObtenerMontoRecaudadoMes(int anio, int mes)
        {
            const string sql = @"
                SELECT COALESCE(SUM(montopago), 0)
                FROM aocr_tbpago
                WHERE EXTRACT(YEAR FROM fechapago) = @anio
                  AND EXTRACT(MONTH FROM fechapago) = @mes
                  AND estado = 'VALIDADO';";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@anio", anio);
                cmd.Parameters.AddWithValue("@mes", mes);

                cn.Open();
                var valor = cmd.ExecuteScalar();
                return valor != null && valor != DBNull.Value
                    ? Convert.ToDecimal(valor)
                    : 0m;
            }
        }
    }
}
