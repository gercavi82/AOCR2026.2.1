using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    public class FinancieroDAO
    {
        private readonly string _connectionString;

        public FinancieroDAO()
        {
            _connectionString = ConexionDAO.ObtenerCadenaConexion();
        }

        public bool Insertar(Financiero fin)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO financiero (
                        codigo_solicitud, tipo_movimiento, concepto, monto,
                        fecha_movimiento, metodo_pago, numero_comprobante, banco,
                        numero_cuenta, estado, observaciones,
                        created_at, created_by
                    ) VALUES (
                        @CodigoSolicitud, @TipoMovimiento, @Concepto, @Monto,
                        @FechaMovimiento, @MetodoPago, @NumeroComprobante, @Banco,
                        @NumeroCuenta, @Estado, @Observaciones,
                        NOW(), @CreatedBy
                    )
                    RETURNING codigo_financiero;";
                int id = db.QuerySingle<int>(sql, fin);
                fin.CodigoFinanciero = id;
                return id > 0;
            }
        }

        public bool Actualizar(Financiero fin)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE financiero SET
                        codigo_solicitud = @CodigoSolicitud,
                        tipo_movimiento = @TipoMovimiento,
                        concepto = @Concepto,
                        monto = @Monto,
                        fecha_movimiento = @FechaMovimiento,
                        metodo_pago = @MetodoPago,
                        numero_comprobante = @NumeroComprobante,
                        banco = @Banco,
                        numero_cuenta = @NumeroCuenta,
                        estado = @Estado,
                        observaciones = @Observaciones,
                        updated_at = NOW(),
                        updated_by = @UpdatedBy
                    WHERE codigo_financiero = @CodigoFinanciero;";
                int filas = db.Execute(sql, fin);
                return filas > 0;
            }
        }

        public bool Eliminar(int codigoFinanciero)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = "DELETE FROM financiero WHERE codigo_financiero = @Id;";
                int filas = db.Execute(sql, new { Id = codigoFinanciero });
                return filas > 0;
            }
        }

        public Financiero ObtenerPorId(int codigoFinanciero)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT * FROM financiero
                    WHERE codigo_financiero = @Id;";
                return db.QueryFirstOrDefault<Financiero>(sql, new { Id = codigoFinanciero });
            }
        }

        public List<Financiero> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT * FROM financiero
                    ORDER BY fecha_movimiento DESC;";
                return db.Query<Financiero>(sql).ToList();
            }
        }

        public List<Financiero> ObtenerPorSolicitud(int codigoSolicitud)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT * FROM financiero
                    WHERE codigo_solicitud = @Solicitud
                    ORDER BY fecha_movimiento DESC;";
                return db.Query<Financiero>(sql, new { Solicitud = codigoSolicitud }).ToList();
            }
        }

        // — Métodos adicionales para pagos / estado —

        public bool ActualizarEstadoPago(int codigoFinanciero, string nuevoEstado)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE financiero
                    SET estado = @Estado,
                        updated_at = NOW()
                    WHERE codigo_financiero = @Id;";
                int filas = db.Execute(sql, new { Estado = nuevoEstado, Id = codigoFinanciero });
                return filas > 0;
            }
        }

        public bool RegistrarPago(int codigoFinanciero, decimal montoPagado, string metodoPago)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE financiero
                    SET monto_pagado = COALESCE(monto_pagado, 0) + @MontoPagado,
                        metodo_pago = @MetodoPago,
                        updated_at = NOW()
                    WHERE codigo_financiero = @Id;";
                int filas = db.Execute(sql, new { MontoPagado = montoPagado, MetodoPago = metodoPago, Id = codigoFinanciero });
                return filas > 0;
            }
        }

        public List<Financiero> ObtenerPorEstadoPago(string estadoPago)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT * FROM financiero
                    WHERE estado = @Estado
                    ORDER BY fecha_movimiento DESC;";
                return db.Query<Financiero>(sql, new { Estado = estadoPago }).ToList();
            }
        }

        public decimal ObtenerTotalIngresos(DateTime fechaInicio, DateTime fechaFin)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT COALESCE(SUM(monto), 0)
                    FROM financiero
                    WHERE tipo_movimiento = 'INGRESO'
                      AND estado = 'CONFIRMADO'
                      AND fecha_movimiento BETWEEN @Inicio AND @Fin;";
                return db.ExecuteScalar<decimal>(sql, new { Inicio = fechaInicio, Fin = fechaFin });
            }
        }
    }
}
