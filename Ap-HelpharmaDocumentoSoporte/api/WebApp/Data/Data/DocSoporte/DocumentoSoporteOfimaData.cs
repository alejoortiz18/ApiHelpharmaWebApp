using Data.Interfaces;
using Microsoft.Data.SqlClient;
using Models.Dto.DocumentoSoporteDto;
using Models.Dto.Request;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Data.DocSoporte
{
    public class DocumentoSoporteOfimaData : IDocumentoSoporteOfimaData
    {
        private readonly IOfimaConnectionFactory _connectionFactory;

        public DocumentoSoporteOfimaData(IOfimaConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
                

        public async Task<PacienteDto?> GetSoporteOfima(string DCTOPRV)
        {
            PacienteDto? paciente = null;

            using var connection = _connectionFactory.CreateConnection();
            
            await connection.OpenAsync();

            string query = @"
                SELECT
                    m.ORDENENTMV as ordenes,
                    m.producto,
                    m.nombre,
                    CAST(m.cantidad AS INT) as cantidad,
                    m.ORDENNRO as lote,
                    cn.NOMBRE as convenio,
                    M.FECHA as fecha,
                    m.BODEGA as bodega,
                    tv.DESCRIPCIO as tipoEntrega,
                    T.TIPOCAR as cartera,
                    c.NOMBRE as nombrePaciente,
                    c.TIPODC as tipoId,
                    c.NIT as paciente,
                    c.DIRECCION as direccionPaciente,
                    c.TEL1 as telefonoPaciente,
                    c.CELULAR as celularPaciente,
                    c.COMENTARIO as complemento,
                    T.NOTA as observacion,
                    '' as valorCM,
                    M.VALORUNIT AS valorMx,
                    t.PASSWORDIN as usuario
                FROM trade t 
                    left join mvtrade m on t.origen = m.origen and t.tipodcto = m.tipodcto and t.nrodcto = m.nrodcto
                    left join trademas tm on t.tipodcto = tm.tipodcto and t.nrodcto = tm.nrodcto
                    INNER JOIN CANAL cn ON cn.CODCANAL = tm.CODCANAL 
                    left join mtprocli c on t.nit = c.nit
                    left join tipocar k on t.tipocar = k.codtc
                    inner join TIPOVTA TV ON TV.TIPOVTA = T.TIPOVTA
                WHERE
                    t.DCTOPRV = @DCTOPRV
                    and m.PRODUCTO <> 'S3501'
                    and m.PRODUCTO <> 'S3500';";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@DCTOPRV", SqlDbType.VarChar).Value = DCTOPRV;

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // ✅ Crear paciente una sola vez
                if (paciente == null)
                {
                    paciente = new PacienteDto
                    {
                        NombrePaciente = Convert.ToString(reader["nombrePaciente"]).Trim(),
                        TipoId = Convert.ToString(reader["tipoId"]).Trim(),
                        Paciente = Convert.ToString(reader["paciente"]).Trim(),
                        DireccionPaciente = Convert.ToString(reader["direccionPaciente"]).Trim(),
                        TelefonoPaciente = Convert.ToString(reader["telefonoPaciente"]).Trim(),
                        CelularPaciente = Convert.ToString(reader["celularPaciente"]).Trim(),
                        Complemento = Convert.ToString(reader["complemento"]).Trim(),
                        ValorCMTotal = 0,

                        Factura = new FacturaDto
                        {
                            Convenio = Convert.ToString(reader["convenio"]).Trim(),
                            Fecha = reader.IsDBNull(reader.GetOrdinal("fecha"))
                                    ? DateTime.MinValue
                                    : reader.GetDateTime(reader.GetOrdinal("fecha")),
                            Bodega = Convert.ToString(reader["bodega"]).Trim(),
                            TipoEntrega = Convert.ToString(reader["tipoEntrega"]).Trim(),
                            Cartera = Convert.ToString(reader["cartera"]).Trim(),
                            Observacion = Convert.ToString(reader["observacion"]).Trim(),
                            Usuario = Convert.ToString(reader["usuario"]).Trim(),
                            Ordenes = new List<OrdenDto>(),
                        },

                    };
                }
                var valorMx = reader["valorMx"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["valorMx"]);

                var producto = Convert.ToString(reader["producto"]).Trim();

                // ✅ regla de negocio: cuota moderadora positiva
                if (producto == "S3500")
                {
                    valorMx = Math.Abs(valorMx);
                }

                var orden = new OrdenDto
                {
                    Ordenes = Convert.ToString(reader["ordenes"]).Trim(),
                    Producto = producto.Trim(),
                    Nombre = Convert.ToString(reader["nombre"]).Trim(),
                    Cantidad = reader["cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cantidad"]),
                    Lote = Convert.ToString(reader["lote"]).Trim(),
                    ValorMx = valorMx
                };

                paciente.Factura.Ordenes.Add(orden);

                // ✅ ACUMULAR correctamente
                paciente.ValorCMTotal += producto == "S3500" ? orden.ValorMx :0;
            }

            return paciente;
        }

        public async Task<PacienteDto?> GetSoporteTradeOfima(TradeDto trade)
        {
            PacienteDto? paciente = null;

            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    m.ORDENENTMV as ordenes,
                    m.producto,
                    m.nombre,
                    CAST(m.cantidad AS INT) as cantidad,
                    m.ORDENNRO as lote,
                    cn.NOMBRE as convenio,
                    M.FECHA as fecha,
                    m.BODEGA as bodega,
                    tv.DESCRIPCIO as tipoEntrega,
                    T.TIPOCAR as cartera,
                    c.NOMBRE as nombrePaciente,
                    c.TIPODC as tipoId,
                    c.NIT as paciente,
                    c.DIRECCION as direccionPaciente,
                    c.TEL1 as telefonoPaciente,
                    c.CELULAR as celularPaciente,
                    c.COMENTARIO as complemento,
                    T.NOTA as observacion,
                    '' as valorCM,
                    M.VALORUNIT AS valorMx,
                    t.PASSWORDIN as usuario
                FROM trade t 
                    left join mvtrade m on t.origen = m.origen and t.tipodcto = m.tipodcto and t.nrodcto = m.nrodcto
                    left join trademas tm on t.tipodcto = tm.tipodcto and t.nrodcto = tm.nrodcto
                    INNER JOIN CANAL cn ON cn.CODCANAL = tm.CODCANAL 
                    left join mtprocli c on t.nit = c.nit
                    left join tipocar k on t.tipocar = k.codtc
                    inner join TIPOVTA TV ON TV.TIPOVTA = T.TIPOVTA
                WHERE
                    T.TIPODCTO= @TIPODCTO AND
                    T.NRODCTO= @NRODCTO
                    and m.PRODUCTO <> 'S3501'
                    and m.PRODUCTO <> 'S3500';";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@TIPODCTO", SqlDbType.VarChar).Value = trade.Tipodcto;
            command.Parameters.Add("@NRODCTO", SqlDbType.VarChar).Value = trade.Nrodcto;

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // ✅ Crear paciente una sola vez
                if (paciente == null)
                {
                    paciente = new PacienteDto
                    {
                        NombrePaciente = Convert.ToString(reader["nombrePaciente"]).Trim(),
                        TipoId = Convert.ToString(reader["tipoId"]).Trim(),
                        Paciente = Convert.ToString(reader["paciente"]).Trim(),
                        DireccionPaciente = Convert.ToString(reader["direccionPaciente"]).Trim(),
                        TelefonoPaciente = Convert.ToString(reader["telefonoPaciente"]).Trim(),
                        CelularPaciente = Convert.ToString(reader["celularPaciente"]).Trim(),
                        Complemento = Convert.ToString(reader["complemento"]).Trim(),
                        ValorCMTotal = 0,

                        Factura = new FacturaDto
                        {
                            Convenio = Convert.ToString(reader["convenio"]).Trim(),
                            Fecha = reader.IsDBNull(reader.GetOrdinal("fecha"))
                                    ? DateTime.MinValue
                                    : reader.GetDateTime(reader.GetOrdinal("fecha")),
                            Bodega = Convert.ToString(reader["bodega"]).Trim(),
                            TipoEntrega = Convert.ToString(reader["tipoEntrega"]).Trim(),
                            Cartera = Convert.ToString(reader["cartera"]).Trim(),
                            Observacion = Convert.ToString(reader["observacion"]).Trim(),
                            Usuario = Convert.ToString(reader["usuario"]).Trim(),
                            Ordenes = new List<OrdenDto>(),
                        },

                    };
                }
                var valorMx = reader["valorMx"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["valorMx"]);

                var producto = Convert.ToString(reader["producto"]).Trim();

                // ✅ regla de negocio: cuota moderadora positiva
                if (producto == "S3500")
                {
                    valorMx = Math.Abs(valorMx);
                }

                var orden = new OrdenDto
                {
                    Ordenes = Convert.ToString(reader["ordenes"]).Trim(),
                    Producto = producto.Trim(),
                    Nombre = Convert.ToString(reader["nombre"]).Trim(),
                    Cantidad = reader["cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cantidad"]),
                    Lote = Convert.ToString(reader["lote"]).Trim(),
                    ValorMx = valorMx
                };

                paciente.Factura.Ordenes.Add(orden);

                // ✅ ACUMULAR correctamente
                paciente.ValorCMTotal += producto == "S3500" ? orden.ValorMx : 0;
            }

            return paciente;
        }


        public async Task<SoporteEntregaDto?> GetDatosSoportes(TradeDto trade)
        {
            SoporteEntregaDto? soporte = null;

            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            string query = @"
            SELECT 
                cn.codcanal AS IdConvenio,
                cn.NOMBRE as NombreConvenio,
                M.FECHA as Fecha,
                m.BODEGA as IdBodega,
                bo.SEV_FARM as NombreSede,
                '' as NombreActividad,
                tv.DESCRIPCIO as TipoEntrega,
                tm.TIPOFAC as TipoPlan,
                T.TIPOCAR as IdCartera,
                c.NOMBRE as NombrePaciente,
                c.TIPODC as IdTipoId,
                c.NIT as IdPaciente,
                c.CELULAR as Celular,
                c.TEL1 as Telefono,
                c.DIRECCION as Direccion,
                c.COMENTARIO as Complemento,
                T.NOTA as Observacion,
                M.VALORUNIT AS ValorCM,
                m.ORDENENTMV as Ordenes,
                m.producto,
                m.nombre as NombreMedicamento,
                CAST(m.cantidad AS INT) as cantidad,
                0 as ValorMx
            FROM trade t 
                left join mvtrade m on t.origen = m.origen and t.tipodcto = m.tipodcto and t.nrodcto = m.nrodcto
                left join trademas tm on t.tipodcto = tm.tipodcto and t.nrodcto = tm.nrodcto
                INNER JOIN CANAL cn ON cn.CODCANAL = tm.CODCANAL 
                left join mtprocli c on t.nit = c.nit
                inner join TIPOVTA TV ON TV.TIPOVTA = T.TIPOVTA
                inner join MTBODEGA bo  on bo.LOGIN = m.BODEGA
            WHERE   
                T.NRODCTO = @NRODCTO
                AND T.TIPODCTO = @TIPODCTO
                AND m.PRODUCTO <> 'S3501'
                AND m.PRODUCTO <> 'S3500'
            ";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@TIPODCTO", SqlDbType.VarChar).Value = trade.Tipodcto;
            command.Parameters.Add("@NRODCTO", SqlDbType.VarChar).Value = trade.Nrodcto;

            using var reader = await command.ExecuteReaderAsync();

            decimal totalValorCM = 0;

            while (await reader.ReadAsync())
            {
                if (soporte == null)
                {
                    soporte = new SoporteEntregaDto
                    {
                        IdConvenio = reader["IdConvenio"]?.ToString()?.Trim(),
                        NombreConvenio = reader["NombreConvenio"]?.ToString()?.Trim(),
                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                        IdBodega = reader["IdBodega"]?.ToString()?.Trim(),
                        NombreSede = reader["NombreSede"]?.ToString()?.Trim(),
                        NombreActividad = reader["NombreActividad"]?.ToString()?.Trim(),
                        TipoEntrega = reader["TipoEntrega"]?.ToString()?.Trim(),
                        TipoPlan = reader["TipoPlan"]?.ToString()?.Trim(),
                        IdCartera = reader["IdCartera"]?.ToString()?.Trim(),
                        NombrePaciente = reader["NombrePaciente"]?.ToString()?.Trim(),
                        idTipoId = reader["IdTipoId"]?.ToString()?.Trim(),
                        IdPaciente = reader["IdPaciente"] == DBNull.Value ? string.Empty : Convert.ToString(reader["IdPaciente"])?.Trim() ?? string.Empty,
                        Celular = reader["Celular"]?.ToString()?.Trim(),
                        Telefono = reader["Telefono"]?.ToString()?.Trim(),
                        Direccion = reader["Direccion"]?.ToString()?.Trim(),
                        Complemento = reader["Complemento"]?.ToString()?.Trim(),
                        Observacion = reader["Observacion"]?.ToString()?.Trim(),
                        ValorCM = "0",
                        medicamentos = new List<OrdenDto>()
                    };
                }

                var producto = reader["producto"]?.ToString()?.Trim();

                var valorCM = reader["ValorCM"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["ValorCM"]);

                // 🔥 regla negocio
                if (producto == "S3500")
                {
                    valorCM = Math.Abs(valorCM);
                    totalValorCM += valorCM;
                }

                var orden = new OrdenDto
                {
                    Ordenes = reader["Ordenes"]?.ToString()?.Trim(),
                    Producto = producto,
                    Nombre = reader["NombreMedicamento"]?.ToString()?.Trim(),
                    Cantidad = reader["cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cantidad"]),
                    Lote = "",
                    ValorMx = 0
                };

                soporte.medicamentos.Add(orden);
            }

            if (soporte != null)
            {
                soporte.ValorCM = totalValorCM.ToString("0.##");
            }

            return soporte;
        }


        //public SoporteRequest GetSoporte(SoporteRequest soporte)
        //{
        //    PacienteDto? paciente = null;

        //    using var connection = _connectionFactory.CreateConnection();
        //    await connection.OpenAsync();

        //    string query = @"
        //       SELECT *
        //          FROM [FilesNas].[dbo].[ArchivosIndexados]
        //          where NumeroFactura = '' and Prefijo = '' and Extension = 'pdf'
        //        ;";

        //    using var command = new SqlCommand(query, connection);
        //    command.Parameters.Add("@NumeroFactura", SqlDbType.VarChar).Value = soporte.NumeroFactura;
        //    command.Parameters.Add("@Prefijo", SqlDbType.VarChar).Value = soporte.Prefijo;

        //    using var reader = await command.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        // ✅ Crear paciente una sola vez
        //        if (paciente == null)
        //        {
        //            paciente = new PacienteDto
        //            {
        //                NombrePaciente = Convert.ToString(reader["nombrePaciente"]).Trim(),
        //                TipoId = Convert.ToString(reader["tipoId"]).Trim(),
        //                Paciente = Convert.ToString(reader["paciente"]).Trim(),
        //                DireccionPaciente = Convert.ToString(reader["direccionPaciente"]).Trim(),
        //                TelefonoPaciente = Convert.ToString(reader["telefonoPaciente"]).Trim(),
        //                CelularPaciente = Convert.ToString(reader["celularPaciente"]).Trim(),
        //                Complemento = Convert.ToString(reader["complemento"]).Trim(),
        //                ValorCMTotal = 0,

        //                Factura = new FacturaDto
        //                {
        //                    Convenio = Convert.ToString(reader["convenio"]).Trim(),
        //                    Fecha = reader.IsDBNull(reader.GetOrdinal("fecha"))
        //                            ? DateTime.MinValue
        //                            : reader.GetDateTime(reader.GetOrdinal("fecha")),
        //                    Bodega = Convert.ToString(reader["bodega"]).Trim(),
        //                    TipoEntrega = Convert.ToString(reader["tipoEntrega"]).Trim(),
        //                    Cartera = Convert.ToString(reader["cartera"]).Trim(),
        //                    Observacion = Convert.ToString(reader["observacion"]).Trim(),
        //                    Usuario = Convert.ToString(reader["usuario"]).Trim(),
        //                    Ordenes = new List<OrdenDto>(),
        //                },

        //            };
        //        }
              
        //    }

        //    return paciente;

        //}
    }
}
