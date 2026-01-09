using System;
using System.Collections.Generic;
using IBM.Data.DB2.iSeries;

namespace CapaDatos.DAOs
{
    public class EmpresaAS400DAO
    {
        private readonly string cadenaConexion =
           "DataSource=190.152.8.185;UserID=DGACCONEXI;Password=DGACTIC20@;Database=S10a1a05;DataCompression=True;Default Collection = DGACDATPRO;";

        public List<EmpresaDTO> ObtenerEmpresas()
        {
            var lista = new List<EmpresaDTO>();

            using (var conexion = new iDB2Connection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    SELECT CIACOD, CIANOM
                    FROM DGACDAT.CIAARC
                    FETCH FIRST 100 ROWS ONLY
                ";

                using (var cmd = new iDB2Command(query, conexion))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new EmpresaDTO
                        {
                            Codigo = reader["CIACOD"].ToString().Trim(),
                            Nombre = reader["CIANOM"].ToString().Trim()
                        });
                    }
                }
            }

            return lista;
        }
    }

    public class EmpresaDTO
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
    }
}
