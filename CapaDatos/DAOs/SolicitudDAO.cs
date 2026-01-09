using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Reflection;
using Npgsql;
using CapaModelo;

namespace CapaDatos.DAOs
{
    /// <summary>
    /// DAO de Solicitud AOCR para PostgreSQL (Npgsql)
    /// Compatible con .NET Framework 4.7.2
    /// </summary>
    public class SolicitudDAO
    {
        // ==============================
        // Conexión
        // ==============================
        private NpgsqlConnection CrearConexion()
        {
            var cs = ConfigurationManager.ConnectionStrings["AOCRConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new Exception("No existe la cadena de conexión 'AOCRConnection' en el config.");

            return new NpgsqlConnection(cs);
        }

        // ==============================
        // Helper reflection para int/int?
        // (para propiedades opcionales como CodigoSolicitud)
        // ==============================
        private static void SetIntProp(object obj, string propName, int? value)
        {
            try
            {
                if (obj == null) return;

                var prop = obj.GetType().GetProperty(
                    propName,
                    BindingFlags.Public | BindingFlags.Instance
                );

                if (prop == null || !prop.CanWrite)
                    return;

                if (prop.PropertyType == typeof(int))
                    prop.SetValue(obj, value ?? 0, null);
                else if (prop.PropertyType == typeof(int?))
                    prop.SetValue(obj, value, null);
            }
            catch
            {
                // silencioso: si el modelo no tiene la propiedad, no pasa nada
            }
        }

        // ==============================
        // Mapeo básico
        // ==============================
        private SolicitudAOCR MapSolicitud(IDataRecord r)
        {
            return new SolicitudAOCR
            {
                CodigoSolicitud = r["codigosolicitud"] != DBNull.Value ? Convert.ToInt32(r["codigosolicitud"]) : 0,
                NumeroSolicitud = r["numerosolicitud"]?.ToString(),
                CodigoUsuario = r["codigousuario"] != DBNull.Value ? Convert.ToInt32(r["codigousuario"]) : 0,
                FechaSolicitud = r["fechasolicitud"] != DBNull.Value ? Convert.ToDateTime(r["fechasolicitud"]) : DateTime.MinValue,
                Estado = r["estado"]?.ToString(),

                NombreOperador = r["nombreoperador"]?.ToString(),
                Ruc = r["ruc"]?.ToString(),
                RazonSocial = r["razonsocial"]?.ToString(),
                Email = r["email"]?.ToString(),
                Telefono = r["telefono"]?.ToString(),
                Direccion = r["direccion"]?.ToString(),
                Ciudad = r["ciudad"]?.ToString(),
                Provincia = r["provincia"]?.ToString(),
                Pais = r["pais"]?.ToString(),
                RepresentanteLegal = r["representantelegal"]?.ToString(),
                CedulaRepresentante = r["cedularepresentante"]?.ToString(),
                TipoOperacion = r["tipooperacion"]?.ToString(),
                DescripcionOperacion = r["descripcionoperacion"]?.ToString(),
                Observaciones = r["observaciones"]?.ToString(),

                CreatedAt = r["createdat"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["createdat"]) : null,
                CreatedBy = r["createdby"] != DBNull.Value ? r["createdby"].ToString() : null,
                UpdatedAt = r["updatedat"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["updatedat"]) : null,
                UpdatedBy = r["updatedby"] != DBNull.Value ? r["updatedby"].ToString() : null,
                DeletedAt = r["deletedat"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["deletedat"]) : null,
                DeletedBy = r["deletedby"] != DBNull.Value ? r["deletedby"].ToString() : null
            };
        }

        // ==============================
        // Listar activas
        // ==============================
        public List<SolicitudAOCR> ListarActivas()
        {
            var lista = new List<SolicitudAOCR>();

            const string sql = @"
                SELECT 
                    codigosolicitud, numerosolicitud, codigousuario, fechasolicitud, estado,
                    nombreoperador, ruc, razonsocial, email, telefono, direccion, ciudad, provincia, pais,
                    representantelegal, cedularepresentante, tipooperacion, descripcionoperacion, observaciones,
                    createdat, createdby, updatedat, updatedby, deletedat, deletedby
                FROM aocr_tbsolicitud
                WHERE deletedat IS NULL
                ORDER BY fechasolicitud DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapSolicitud(rd));
                }
            }

            return lista;
        }

        // ==============================
        // Obtener por Id
        // ==============================
        public SolicitudAOCR ObtenerPorId(int codigoSolicitud)
        {
            const string sql = @"
                SELECT 
                    codigosolicitud, numerosolicitud, codigousuario, fechasolicitud, estado,
                    nombreoperador, ruc, razonsocial, email, telefono, direccion, ciudad, provincia, pais,
                    representantelegal, cedularepresentante, tipooperacion, descripcionoperacion, observaciones,
                    createdat, createdby, updatedat, updatedby, deletedat, deletedby
                FROM aocr_tbsolicitud
                WHERE codigosolicitud = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                        return MapSolicitud(rd);
                }
            }

            return null;
        }

        // ==============================
        // Detalle (si tu modelo tiene estas listas)
        // ==============================
        public SolicitudAOCR ObtenerDetalle(int codigoSolicitud)
        {
            var solicitud = ObtenerPorId(codigoSolicitud);
            if (solicitud == null) return null;

            // Si en tu CapaModelo NO existen estas propiedades,
            // comenta estas líneas y usa solo ObtenerPorId.
            try
            {
                solicitud.Documentos = ObtenerDocumentosPorSolicitud(codigoSolicitud);
                solicitud.Pagos = ObtenerPagosPorSolicitud(codigoSolicitud);
                solicitud.Inspecciones = ObtenerInspeccionesPorSolicitud(codigoSolicitud);
                solicitud.ObservacionesLista = ObtenerObservacionesPorSolicitud(codigoSolicitud);
                solicitud.HistorialEstados = ObtenerHistorialPorSolicitud(codigoSolicitud);
            }
            catch
            {
                // Evita romper compilación si tu modelo aún no tiene esas colecciones
            }

            return solicitud;
        }

        // ==============================
        // Conteo por año
        // ==============================
        public int ContarSolicitudesAnio(int year)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM aocr_tbsolicitud
                WHERE numerosolicitud LIKE @pref;";

            string pref = $"AOCR-{year}-%";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@pref", pref);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ==============================
        // Crear + historial
        // ==============================
        public bool CrearConHistorial(
            SolicitudAOCR modelo,
            int codigoUsuario,
            string estadoAnterior,
            string estadoNuevo,
            string observaciones)
        {
            const string sqlInsert = @"
                INSERT INTO aocr_tbsolicitud
                (numerosolicitud, codigousuario, fechasolicitud, estado,
                 nombreoperador, ruc, razonsocial, email, telefono, direccion, ciudad, provincia, pais,
                 representantelegal, cedularepresentante, tipooperacion, descripcionoperacion, observaciones,
                 createdat, createdby)
                VALUES
                (@numero, @codUsuario, @fecha, @estado,
                 @nombreOperador, @ruc, @razon, @email, @tel, @dir, @ciudad, @prov, @pais,
                 @repLegal, @cedRep, @tipoOp, @descOp, @obs,
                 @createdAt, @createdBy)
                RETURNING codigosolicitud;";

            const string sqlHist = @"
                INSERT INTO aocr_tbhistorialestado
                (codigosolicitud, estadoanterior, estadonuevo, codigousuario, observaciones, fechacambio)
                VALUES
                (@idSol, @ant, @nuevo, @user, @obs, @fecha);";

            using (var cn = CrearConexion())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand(sqlInsert, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@numero", (object)modelo.NumeroSolicitud ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@codUsuario", modelo.CodigoUsuario);
                            cmd.Parameters.AddWithValue("@fecha", modelo.FechaSolicitud);
                            cmd.Parameters.AddWithValue("@estado", (object)modelo.Estado ?? "BORRADOR");

                            cmd.Parameters.AddWithValue("@nombreOperador", (object)modelo.NombreOperador ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ruc", (object)modelo.Ruc ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@razon", (object)modelo.RazonSocial ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@email", (object)modelo.Email ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@tel", (object)modelo.Telefono ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@dir", (object)modelo.Direccion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ciudad", (object)modelo.Ciudad ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@prov", (object)modelo.Provincia ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@pais", (object)modelo.Pais ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@repLegal", (object)modelo.RepresentanteLegal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@cedRep", (object)modelo.CedulaRepresentante ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@tipoOp", (object)modelo.TipoOperacion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@descOp", (object)modelo.DescripcionOperacion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@obs", (object)modelo.Observaciones ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@createdAt", (object)modelo.CreatedAt ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@createdBy",
                                (object)modelo.CreatedBy ?? codigoUsuario.ToString());

                            var id = cmd.ExecuteScalar();
                            modelo.CodigoSolicitud = Convert.ToInt32(id);
                        }

                        using (var cmdHist = new NpgsqlCommand(sqlHist, cn, tx))
                        {
                            cmdHist.Parameters.AddWithValue("@idSol", modelo.CodigoSolicitud);
                            cmdHist.Parameters.AddWithValue("@ant", (object)estadoAnterior ?? DBNull.Value);
                            cmdHist.Parameters.AddWithValue("@nuevo", (object)estadoNuevo ?? "BORRADOR");
                            cmdHist.Parameters.AddWithValue("@user", codigoUsuario);
                            cmdHist.Parameters.AddWithValue("@obs", (object)observaciones ?? DBNull.Value);
                            cmdHist.Parameters.AddWithValue("@fecha", DateTime.Now);

                            cmdHist.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }

        // ==============================
        // Actualizar
        // ==============================
        public bool Actualizar(SolicitudAOCR modelo)
        {
            const string sql = @"
                UPDATE aocr_tbsolicitud
                SET
                    nombreoperador = @nombreOperador,
                    ruc = @ruc,
                    razonsocial = @razon,
                    email = @email,
                    telefono = @tel,
                    direccion = @dir,
                    ciudad = @ciudad,
                    provincia = @prov,
                    pais = @pais,
                    representantelegal = @repLegal,
                    cedularepresentante = @cedRep,
                    tipooperacion = @tipoOp,
                    descripcionoperacion = @descOp,
                    observaciones = @obs,
                    updatedat = @updatedAt,
                    updatedby = @updatedBy
                WHERE codigosolicitud = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@nombreOperador", (object)modelo.NombreOperador ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ruc", (object)modelo.Ruc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@razon", (object)modelo.RazonSocial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@email", (object)modelo.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tel", (object)modelo.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dir", (object)modelo.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ciudad", (object)modelo.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@prov", (object)modelo.Provincia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@pais", (object)modelo.Pais ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@repLegal", (object)modelo.RepresentanteLegal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cedRep", (object)modelo.CedulaRepresentante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipoOp", (object)modelo.TipoOperacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descOp", (object)modelo.DescripcionOperacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@obs", (object)modelo.Observaciones ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@updatedAt", (object)modelo.UpdatedAt ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@updatedBy", (object)modelo.UpdatedBy ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@id", modelo.CodigoSolicitud);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Cambiar estado SIMPLE (para DocumentoBL)
        // Usuario string, sin historial obligatorio
        // ==============================
        public bool CambiarEstado(int codigoSolicitud, string estadoNuevo, string usuario)
        {
            const string sql = @"
                UPDATE aocr_tbsolicitud
                SET estado = @nuevo, updatedat = @updAt, updatedby = @updBy
                WHERE codigosolicitud = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@nuevo", estadoNuevo);
                cmd.Parameters.AddWithValue("@updAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@updBy", (object)usuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Cambiar estado + historial
        // ==============================
        public bool CambiarEstadoConHistorial(int codigoSolicitud, string estadoNuevo, int codigoUsuario, string observaciones)
        {
            const string sqlGetEstado = @"SELECT estado FROM aocr_tbsolicitud WHERE codigosolicitud = @id;";
            const string sqlUpd = @"
                UPDATE aocr_tbsolicitud
                SET estado = @nuevo, updatedat = @updAt, updatedby = @updBy
                WHERE codigosolicitud = @id;";

            const string sqlHist = @"
                INSERT INTO aocr_tbhistorialestado
                (codigosolicitud, estadoanterior, estadonuevo, codigousuario, observaciones, fechacambio)
                VALUES
                (@idSol, @ant, @nuevo, @user, @obs, @fecha);";

            using (var cn = CrearConexion())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        string estadoAnterior = null;

                        using (var cmdGet = new NpgsqlCommand(sqlGetEstado, cn, tx))
                        {
                            cmdGet.Parameters.AddWithValue("@id", codigoSolicitud);
                            var obj = cmdGet.ExecuteScalar();
                            estadoAnterior = obj?.ToString();
                        }

                        using (var cmdUpd = new NpgsqlCommand(sqlUpd, cn, tx))
                        {
                            cmdUpd.Parameters.AddWithValue("@nuevo", estadoNuevo);
                            cmdUpd.Parameters.AddWithValue("@updAt", DateTime.Now);
                            cmdUpd.Parameters.AddWithValue("@updBy", codigoUsuario);
                            cmdUpd.Parameters.AddWithValue("@id", codigoSolicitud);

                            if (cmdUpd.ExecuteNonQuery() <= 0)
                                throw new Exception("No se actualizó estado.");
                        }

                        using (var cmdHist = new NpgsqlCommand(sqlHist, cn, tx))
                        {
                            cmdHist.Parameters.AddWithValue("@idSol", codigoSolicitud);
                            cmdHist.Parameters.AddWithValue("@ant", (object)estadoAnterior ?? DBNull.Value);
                            cmdHist.Parameters.AddWithValue("@nuevo", estadoNuevo);
                            cmdHist.Parameters.AddWithValue("@user", codigoUsuario);
                            cmdHist.Parameters.AddWithValue("@obs", (object)observaciones ?? DBNull.Value);
                            cmdHist.Parameters.AddWithValue("@fecha", DateTime.Now);

                            cmdHist.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }

        // ==============================
        // Soft delete
        // ==============================
        public bool EliminarSoft(int codigoSolicitud, int codigoUsuario)
        {
            const string sql = @"
                UPDATE aocr_tbsolicitud
                SET deletedat = @dt, deletedby = @db
                WHERE codigosolicitud = @id;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@db", codigoUsuario);
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);

                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==============================
        // Hijos mínimos usados por SolicitudBL
        // ==============================
        public List<string> ObtenerTiposDocumentosPorSolicitud(int codigoSolicitud)
        {
            var list = new List<string>();

            const string sql = @"
                SELECT tipodocumento
                FROM aocr_tbdocumento
                WHERE codigosolicitud = @id AND deletedat IS NULL;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(rd["tipodocumento"]?.ToString());
                }
            }

            return list;
        }

        // ====== Hijos opcionales para detalle ======
        private List<Documento> ObtenerDocumentosPorSolicitud(int codigoSolicitud)
        {
            var list = new List<Documento>();

            const string sql = @"
                SELECT 
                    codigodocumento, codigosolicitud, tipodocumento, rutarchivo, estado
                FROM aocr_tbdocumento
                WHERE codigosolicitud = @id
                ORDER BY codigodocumento DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Documento
                        {
                            CodigoDocumento = rd["codigodocumento"] != DBNull.Value ? Convert.ToInt32(rd["codigodocumento"]) : 0,
                            CodigoSolicitud = rd["codigosolicitud"] != DBNull.Value ? Convert.ToInt32(rd["codigosolicitud"]) : 0,
                            TipoDocumento = rd["tipodocumento"]?.ToString(),
                            RutaArchivo = rd["rutarchivo"]?.ToString(),
                            Estado = rd["estado"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }

        private List<Pago> ObtenerPagosPorSolicitud(int codigoSolicitud)
        {
            var list = new List<Pago>();

            const string sql = @"
                SELECT
                    codigopago, codigosolicitud, monto, metodopago,
                    numerotransaccion, rutacomprobante, estado
                FROM aocr_tbpago
                WHERE codigosolicitud = @id
                ORDER BY codigopago DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Pago
                        {
                            CodigoPago = rd["codigopago"] != DBNull.Value ? Convert.ToInt32(rd["codigopago"]) : 0,
                            CodigoSolicitud = rd["codigosolicitud"] != DBNull.Value ? Convert.ToInt32(rd["codigosolicitud"]) : 0,
                            Monto = rd["monto"] != DBNull.Value ? Convert.ToDecimal(rd["monto"]) : 0m,
                            MetodoPago = rd["metodopago"]?.ToString(),
                            NumeroTransaccion = rd["numerotransaccion"]?.ToString(),
                            RutaComprobante = rd["rutacomprobante"]?.ToString(),
                            Estado = rd["estado"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }

        private List<Inspeccion> ObtenerInspeccionesPorSolicitud(int codigoSolicitud)
        {
            var list = new List<Inspeccion>();

            const string sql = @"
                SELECT
                    codigoinspeccion, codigosolicitud, estado, observaciones
                FROM aocr_tbinspeccion
                WHERE codigosolicitud = @id
                ORDER BY codigoinspeccion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Inspeccion
                        {
                            CodigoInspeccion = rd["codigoinspeccion"] != DBNull.Value ? Convert.ToInt32(rd["codigoinspeccion"]) : 0,
                            CodigoSolicitud = rd["codigosolicitud"] != DBNull.Value ? Convert.ToInt32(rd["codigosolicitud"]) : 0,
                            Estado = rd["estado"]?.ToString(),
                            Observaciones = rd["observaciones"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }

        private List<Observacion> ObtenerObservacionesPorSolicitud(int codigoSolicitud)
        {
            var list = new List<Observacion>();

            const string sql = @"
                SELECT
                    codigoobservacion, codigosolicitud, descripcion
                FROM aocr_tbobservacion
                WHERE codigosolicitud = @id
                ORDER BY codigoobservacion DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var obs = new Observacion
                        {
                            CodigoObservacion = rd["codigoobservacion"] != DBNull.Value
                                ? Convert.ToInt32(rd["codigoobservacion"])
                                : 0,
                            // Ojo: el modelo Observacion NO tiene CodigoSolicitud.
                            // Usamos reflection: si algún día agregas la propiedad,
                            // se llenará automáticamente.
                            Descripcion = rd["descripcion"]?.ToString()
                        };

                        int? codSol =
                            rd["codigosolicitud"] != DBNull.Value
                                ? (int?)Convert.ToInt32(rd["codigosolicitud"])
                                : null;

                        // Intentar setear propiedad "CodigoSolicitud" si existe
                        SetIntProp(obs, "CodigoSolicitud", codSol);

                        list.Add(obs);
                    }
                }
            }

            return list;
        }

        private List<HistorialEstado> ObtenerHistorialPorSolicitud(int codigoSolicitud)
        {
            var list = new List<HistorialEstado>();

            const string sql = @"
                SELECT
                    codigohistorial, codigosolicitud, estadoanterior, estadonuevo,
                    codigousuario, observaciones, fechacambio
                FROM aocr_tbhistorialestado
                WHERE codigosolicitud = @id
                ORDER BY fechacambio DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", codigoSolicitud);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new HistorialEstado
                        {
                            CodigoHistorial = rd["codigohistorial"] != DBNull.Value ? Convert.ToInt32(rd["codigohistorial"]) : 0,
                            CodigoSolicitud = rd["codigosolicitud"] != DBNull.Value ? Convert.ToInt32(rd["codigosolicitud"]) : 0,
                            EstadoAnterior = rd["estadoanterior"] != DBNull.Value ? rd["estadoanterior"]?.ToString() : null,
                            EstadoNuevo = rd["estadonuevo"]?.ToString(),
                            CodigoUsuario = rd["codigousuario"] != DBNull.Value ? Convert.ToInt32(rd["codigousuario"]) : 0,
                            Observaciones = rd["observaciones"]?.ToString(),
                            FechaCambio = rd["fechacambio"] != DBNull.Value ? Convert.ToDateTime(rd["fechacambio"]) : DateTime.Now
                        });
                    }
                }
            }

            return list;
        }
        // ==============================
        // Verificar si un usuario tiene solicitudes
        // ==============================
        public bool TieneSolicitudes(int codigoUsuario)
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM aocr_tbsolicitud
        WHERE codigousuario = @usr
          AND deletedat IS NULL;";

            using (var cn = CrearConexion())
            using (var cmd = new Npgsql.NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@usr", codigoUsuario);

                cn.Open();
                var obj = cmd.ExecuteScalar();
                int cantidad = (obj != null && obj != DBNull.Value) ? Convert.ToInt32(obj) : 0;
                return cantidad > 0;
            }
        }
        public List<SolicitudAOCR> ListarPorUsuario(int codigoUsuario)
        {
            var lista = new List<SolicitudAOCR>();

            const string sql = @"
        SELECT 
            codigosolicitud, numerosolicitud, codigousuario, fechasolicitud, estado,
            nombreoperador, ruc, razonsocial, email, telefono, direccion, ciudad, provincia, pais,
            representantelegal, cedularepresentante, tipooperacion, descripcionoperacion, observaciones,
            createdat, createdby, updatedat, updatedby, deletedat, deletedby
        FROM aocr_tbsolicitud
        WHERE codigousuario = @usr
          AND deletedat IS NULL
        ORDER BY fechasolicitud DESC;";

            using (var cn = CrearConexion())
            using (var cmd = new NpgsqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@usr", codigoUsuario);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapSolicitud(rd));
                }
            }

            return lista;
        }


    }

}
