using System;
using System.Collections.Generic;
using System.Configuration;
using CapaModelo;
using Npgsql;

namespace CapaDatos.DAOs
{
    public class SolicitudAOCRDAO
    {
        private string ConnectionString =>
            ConfigurationManager.ConnectionStrings["AOCRConnection"].ConnectionString;

        // =====================================================
        // INSERTAR SOLICITUD Y DEVOLVER ID
        // =====================================================
        public int InsertarConReturn(SolicitudAOCR s)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
                INSERT INTO aocr_tbsolicitud (
                    nombre_operador, ruc, razon_social, email, telefono,
                    direccion, ciudad, provincia, pais,
                    representante_legal, cedula_representante,
                    tipo_solicitud, tipo_operacion,
                    descripcion_operacion, observaciones,
                    fecha_solicitud, estado, codigo_usuario, codigo_tecnico
                )
                VALUES (
                    @NombreOperador, @Ruc, @RazonSocial, @Email, @Telefono,
                    @Direccion, @Ciudad, @Provincia, @Pais,
                    @RepLegal, @CedulaRep,
                    @TipoSol, @TipoOp,
                    @DescOp, @ObsGen,
                    @Fecha, @Estado, @Usuario, @Tecnico
                )
                RETURNING codigo_solicitud;";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@NombreOperador",
    !string.IsNullOrWhiteSpace(s.NombreOperador)
        ? (object)s.NombreOperador
        : throw new ArgumentException("El campo NombreOperador no puede estar vacío."));
                    cmd.Parameters.AddWithValue("@Ruc", (object)s.Ruc ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RazonSocial", (object)s.RazonSocial ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)s.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", (object)s.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", (object)s.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ciudad", (object)s.Ciudad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Provincia", (object)s.Provincia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pais", (object)s.Pais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepLegal", (object)s.RepresentanteLegal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CedulaRep", (object)s.CedulaRepresentante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TipoSol", (object)s.TipoSolicitud ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TipoOp", (object)s.TipoOperacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DescOp", (object)s.DescripcionOperacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ObsGen", (object)s.ObservacionesGenerales ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha", s.FechaSolicitud);
                    cmd.Parameters.AddWithValue("@Estado", s.Estado);
                    cmd.Parameters.AddWithValue("@Usuario", s.CodigoUsuario);
                    cmd.Parameters.AddWithValue("@Tecnico", (object)s.CodigoTecnico ?? DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // =====================================================
        // LISTAR TODAS LAS SOLICITUDES ACTIVAS
        // =====================================================
        public List<SolicitudAOCR> ListarActivas()
        {
            var lista = new List<SolicitudAOCR>();
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
                SELECT codigo_solicitud, nombre_operador, tipo_solicitud,
                       estado, fecha_solicitud
                FROM aocr_tbsolicitud
                WHERE deleted_at IS NULL
                ORDER BY fecha_solicitud DESC";

                using (var cmd = new NpgsqlCommand(sql, cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new SolicitudAOCR
                        {
                            CodigoSolicitud = rd.GetInt32(0),
                            NombreOperador = rd.IsDBNull(1) ? "" : rd.GetString(1),
                            TipoSolicitud = rd.IsDBNull(2) ? 0 : rd.GetInt32(2),
                            Estado = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            FechaSolicitud = rd.GetDateTime(4)
                        });
                    }
                }
            }
            return lista;
        }

        // =====================================================
        // OBTENER SOLICITUD POR ID (MAPEO COMPLETO)
        // =====================================================
        public SolicitudAOCR ObtenerPorId(int id)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
                SELECT codigo_solicitud, nombre_operador, ruc, razon_social, email,
                       telefono, direccion, ciudad, provincia, pais,
                       representante_legal, cedula_representante, tipo_solicitud,
                       tipo_operacion, descripcion_operacion, observaciones,
                       fecha_solicitud, estado, codigo_usuario, codigo_tecnico
                FROM aocr_tbsolicitud
                WHERE codigo_solicitud = @id AND deleted_at IS NULL";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new SolicitudAOCR
                        {
                            CodigoSolicitud = rd.GetInt32(0),
                            NombreOperador = rd.IsDBNull(1) ? "" : rd.GetString(1),
                            Ruc = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            RazonSocial = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            Email = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            Telefono = rd.IsDBNull(5) ? "" : rd.GetString(5),
                            Direccion = rd.IsDBNull(6) ? "" : rd.GetString(6),
                            Ciudad = rd.IsDBNull(7) ? "" : rd.GetString(7),
                            Provincia = rd.IsDBNull(8) ? "" : rd.GetString(8),
                            Pais = rd.IsDBNull(9) ? "" : rd.GetString(9),
                            RepresentanteLegal = rd.IsDBNull(10) ? "" : rd.GetString(10),
                            CedulaRepresentante = rd.IsDBNull(11) ? "" : rd.GetString(11),
                            TipoSolicitud = rd.IsDBNull(2) ? 0 : rd.GetInt32(2),
                            TipoOperacion = rd.IsDBNull(13) ? "" : rd.GetString(13),
                            DescripcionOperacion = rd.IsDBNull(14) ? "" : rd.GetString(14),
                            ObservacionesGenerales = rd.IsDBNull(15) ? "" : rd.GetString(15),
                            FechaSolicitud = rd.GetDateTime(16),
                            Estado = rd.GetString(17),
                            CodigoUsuario = rd.GetInt32(18),
                            CodigoTecnico = rd.IsDBNull(19) ? (int?)null : rd.GetInt32(19)
                        };
                    }
                }
            }
        }

        // =====================================================
        // CAMBIAR ESTADO + HISTORIAL (TRANSACCIONAL)
        // =====================================================
        public bool CambiarEstado(int codigoSolicitud, string nuevoEstado, int codigoUsuario, string observaciones = "")
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        string estadoAnterior = ObtenerEstadoActual(codigoSolicitud, cn);
                        if (estadoAnterior == nuevoEstado) return false;

                        const string updateSql = @"
                        UPDATE aocr_tbsolicitud
                        SET estado = @nuevo, updated_at = NOW(), updated_by = @user
                        WHERE codigo_solicitud = @id";

                        using (var cmd = new NpgsqlCommand(updateSql, cn))
                        {
                            cmd.Parameters.AddWithValue("@nuevo", nuevoEstado);
                            cmd.Parameters.AddWithValue("@user", codigoUsuario.ToString());
                            cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                            cmd.ExecuteNonQuery();
                        }

                        var historialDAO = new HistorialEstadoDAO();
                        historialDAO.RegistrarCambio(codigoSolicitud, estadoAnterior, nuevoEstado, codigoUsuario, observaciones);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private string ObtenerEstadoActual(int codigoSolicitud, NpgsqlConnection cn)
        {
            const string sql = "SELECT estado FROM aocr_tbsolicitud WHERE codigo_solicitud = @id";
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        public bool ActualizarTecnico(int codigoSolicitud, int codigoTecnico, int codigoUsuario)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
                UPDATE aocr_tbsolicitud
                SET codigo_tecnico = @tecnico, updated_at = NOW(), updated_by = @user
                WHERE codigo_solicitud = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@tecnico", codigoTecnico);
                    cmd.Parameters.AddWithValue("@user", codigoUsuario.ToString());
                    cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =====================================================
        // ACTUALIZAR DATOS GENERALES
        // =====================================================
        public bool ActualizarGeneral(SolicitudAOCR s)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
                UPDATE aocr_tbsolicitud 
                SET nombre_operador = @nom,
                    ruc = @ruc,
                    razon_social = @razon,
                    email = @email,
                    telefono = @tel,
                    direccion = @dir,
                    ciudad = @ciu,
                    provincia = @pro,
                    representante_legal = @rep,
                    cedula_representante = @ced,
                    updated_at = NOW(),
                    updated_by = @user
                WHERE codigo_solicitud = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@nom", (object)s.NombreOperador ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ruc", (object)s.Ruc ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@razon", (object)s.RazonSocial ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", (object)s.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tel", (object)s.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@dir", (object)s.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ciu", (object)s.Ciudad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pro", (object)s.Provincia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@rep", (object)s.RepresentanteLegal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ced", (object)s.CedulaRepresentante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@user", s.UpdatedBy ?? "0");
                    cmd.Parameters.AddWithValue("@id", s.CodigoSolicitud);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<SolicitudAOCR> ObtenerPorUsuario(int codigoUsuario)
        {
            var lista = new List<SolicitudAOCR>();
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
                SELECT codigo_solicitud, nombre_operador, tipo_solicitud, estado, fecha_solicitud
                FROM aocr_tbsolicitud
                WHERE codigo_usuario = @usuario AND deleted_at IS NULL
                ORDER BY fecha_solicitud DESC";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@usuario", codigoUsuario);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new SolicitudAOCR
                            {
                                CodigoSolicitud = rd.GetInt32(0),
                                NombreOperador = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                TipoSolicitud = rd.IsDBNull(2) ? 0 : rd.GetInt32(2),
                                Estado = rd.IsDBNull(3) ? "" : rd.GetString(3),
                                FechaSolicitud = rd.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public SolicitudAOCR ObtenerPorCodigo(string codigo)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
        SELECT codigo_solicitud, nombre_operador, estado, fecha_solicitud
        FROM aocr_tbsolicitud
        WHERE CAST(codigo_solicitud AS TEXT) = @codigo
        AND deleted_at IS NULL";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new SolicitudAOCR
                        {
                            CodigoSolicitud = rd.GetInt32(0),
                            NombreOperador = rd.GetString(1),
                            Estado = rd.GetString(2),
                            FechaSolicitud = rd.GetDateTime(3)
                        };
                    }
                }
            }
        }
        public bool Actualizar(SolicitudAOCR s)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
        UPDATE aocr_tbsolicitud
        SET estado = @estado,
            updated_at = NOW(),
            updated_by = @revisor,
            observaciones = @obs
        WHERE codigo_solicitud = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@estado", s.Estado ?? "PENDIENTE");
                    cmd.Parameters.AddWithValue("@revisor", s.UsuarioRevisor ?? "inspector");
                    cmd.Parameters.AddWithValue("@obs", (object)s.ObservacionesInspector ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", s.CodigoSolicitud);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public List<SolicitudAOCR> ObtenerPendientesRevision()
        {
            var lista = new List<SolicitudAOCR>();
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
            SELECT codigo_solicitud, nombre_operador, tipo_solicitud, estado, fecha_solicitud
            FROM aocr_tbsolicitud
            WHERE estado = 'ENVIADO_A_INSPECTOR' AND deleted_at IS NULL
            ORDER BY fecha_solicitud DESC";

                using (var cmd = new NpgsqlCommand(sql, cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new SolicitudAOCR
                        {
                            CodigoSolicitud = rd.GetInt32(0),
                            NombreOperador = rd.IsDBNull(1) ? "" : rd.GetString(1),
                            TipoSolicitud = rd.IsDBNull(2) ? 0 : rd.GetInt32(2),
                            Estado = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            FechaSolicitud = rd.GetDateTime(4)
                        });
                    }
                }
            }
            return lista;
        }

        public List<SolicitudAOCR> ObtenerParaValidacionJefatura()
        {
            var lista = new List<SolicitudAOCR>();
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
            SELECT codigo_solicitud, nombre_operador, tipo_solicitud, estado, fecha_solicitud
            FROM aocr_tbsolicitud
            WHERE estado = 'ENVIADO_A_JEFATURA' AND deleted_at IS NULL
            ORDER BY fecha_solicitud DESC";

                using (var cmd = new NpgsqlCommand(sql, cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new SolicitudAOCR
                        {
                            CodigoSolicitud = rd.GetInt32(0),
                            NombreOperador = rd.IsDBNull(1) ? "" : rd.GetString(1),
                            TipoSolicitud = rd.IsDBNull(2) ? 0 : rd.GetInt32(2),
                            Estado = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            FechaSolicitud = rd.GetDateTime(4)
                        });
                    }
                }
            }
            return lista;
        }
        public List<SolicitudAOCR> ObtenerPorEstado(string estado)
        {
            var lista = new List<SolicitudAOCR>();
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
            SELECT codigo_solicitud, nombre_operador, tipo_solicitud, estado, fecha_solicitud
            FROM aocr_tbsolicitud
            WHERE estado = @estado AND deleted_at IS NULL
            ORDER BY fecha_solicitud DESC";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@estado", estado);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new SolicitudAOCR
                            {
                                CodigoSolicitud = rd.GetInt32(0),
                                NombreOperador = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                TipoSolicitud = rd.IsDBNull(2) ? 0 : rd.GetInt32(2),
                                Estado = rd.IsDBNull(3) ? "" : rd.GetString(3),
                                FechaSolicitud = rd.GetDateTime(4)
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public bool Eliminar(int id, out string mensaje)
        {
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                const string sql = @"
            UPDATE aocr_tbsolicitud
            SET deleted_at = NOW()
            WHERE codigo_solicitud = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int filas = cmd.ExecuteNonQuery();
                    if (filas > 0)
                    {
                        mensaje = "Solicitud eliminada correctamente.";
                        return true;
                    }
                    else
                    {
                        mensaje = "No se encontró la solicitud.";
                        return false;
                    }
                }
            }
        }



    }
}
