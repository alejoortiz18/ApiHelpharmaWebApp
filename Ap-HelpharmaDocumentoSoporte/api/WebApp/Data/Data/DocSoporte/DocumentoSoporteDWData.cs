using Data.Interfaces;
using Microsoft.Data.SqlClient;
using Models.Dto.DocumentoSoporteDto;
using Models.Dto.Request;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Data.DocSoporte
{
    public class DocumentoSoporteDWData : IDocumentoSoporteDWData 
    {
        private readonly IDwConnectionFactory _connectionFactory;

        public DocumentoSoporteDWData(IDwConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PacienteDto?> GetSoporteDW(string prefijo, int noEntrega)
        {
            try
            {
                PacienteDto? paciente = null;

                using var connection = _connectionFactory.CreateConnection();

                await connection.OpenAsync();

                string query = @"
                SELECT m.ORDEN AS ordenes
	                ,m.IdMedicamento AS producto
	                ,m.nombre 
	                ,CAST(m.QtyEntrega AS INT) as cantidad
	                ,m.IdLote AS lote
	                ,cn.NOMBRE AS convenio
	                ,T.FECHA AS fecha
	                ,T.IdBodega AS bodega
	                ,tv.Nombre AS tipoEntrega
	                ,T.IdCartera AS cartera
	                ,c.NOMBRE AS nombrePaciente
	                ,c.IdTipoId AS tipoId
	                ,c.IdPaciente AS paciente
	                ,c.DIRECCION AS direccionPaciente
	                ,c.Telefono AS telefonoPaciente
	                ,c.CELULAR AS celularPaciente
	                ,c.Direccion2 AS complemento
	                ,T.Observacion AS observacion
	                ,T.ValorCM AS valorCM
	                ,M.Valor AS valorMx
	                ,T.IdUsuario AS usuario
                FROM MvEntregas M WITH (NOLOCK)
	                INNER JOIN Entregas T ON M.Prefijo=T.Prefijo AND M.NoEntrega=T.NoEntrega
	                INNER JOIN Convenios CN ON T.IdConvenio=CN.IdConvenio
	                INNER JOIN Pacientes C ON T.IdPaciente=C.IdPaciente
	                INNER JOIN Carteras K ON T.IdCartera=K.IdCartera
	                INNER JOIN TiposEntrega TV ON T.IdTipoEntrega=TV.IdTipoEntrega
                WHERE M.Prefijo=@prefijo AND M.NoEntrega=@noEntrega
                ";

                using var command = new SqlCommand(query, connection);
                command.Parameters.Add("@prefijo", SqlDbType.VarChar).Value = prefijo;
                command.Parameters.Add("@noEntrega", SqlDbType.VarChar).Value = noEntrega;

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
            catch (Exception ex)
            {
                throw;
            }

           
        }

        public async Task<SoporteEntregaDto?> GetDatosSoportes(TradeDto trade)
        {
            SoporteEntregaDto? soporte = null;

            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            string query = @"
           SELECT 
		        cn.IdConvenio
	            ,cn.NOMBRE AS NombreConvenio
	            ,T.FECHA AS Fecha
	            ,T.IdBodega AS IdBodega
	            ,T.IdBodega AS NombreSede
		        ,'' as NombreActividad --ADD
	            ,tv.Nombre AS TipoEntrega
		        ,T.TipoPlan
	            ,T.IdCartera AS IdCartera
	            ,c.NOMBRE AS NombrePaciente
	            ,c.IdTipoId AS IdTipoId
	            ,c.IdPaciente AS IdPaciente
	            ,c.CELULAR AS celular
	            ,c.Telefono AS Telefono
	            ,c.DIRECCION AS Direccion
		        ,c.Direccion2 as Complemento
	            ,T.Observacion AS Observacion
	            ,T.ValorCM AS ValorCM
		        ,m.ORDEN AS Ordenes
	            ,m.IdMedicamento AS Producto
	            ,m.nombre AS NombreMedicamento
	            ,CAST(m.QtyEntrega AS INT) as Cantidad
	            ,M.Valor AS ValorMx
            FROM MvEntregas M WITH (NOLOCK)
	            INNER JOIN Entregas T ON M.Prefijo=T.Prefijo AND M.NoEntrega=T.NoEntrega
	            INNER JOIN Convenios CN ON T.IdConvenio=CN.IdConvenio
	            INNER JOIN Pacientes C ON T.IdPaciente=C.IdPaciente
	            INNER JOIN Carteras K ON T.IdCartera=K.IdCartera
	            INNER JOIN TiposEntrega TV ON T.IdTipoEntrega=TV.IdTipoEntrega
            WHERE	M.Prefijo=@TIPODCTO AND 
			        M.NoEntrega=@NRODCTO

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
                    soporte = new SoporteEntregaDto();

                    soporte.IdConvenio = reader["IdConvenio"]?.ToString()?.Trim();
                    soporte.NombreConvenio = reader["NombreConvenio"]?.ToString()?.Trim();
                    soporte.Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"));
                    soporte.IdBodega = reader["IdBodega"]?.ToString()?.Trim();
                    soporte.NombreSede = reader["NombreSede"]?.ToString()?.Trim();
                    soporte.NombreActividad = reader["NombreActividad"]?.ToString()?.Trim();
                    soporte.TipoEntrega = reader["TipoEntrega"]?.ToString()?.Trim();
                    soporte.TipoPlan = reader["TipoPlan"]?.ToString()?.Trim();
                    soporte.IdCartera = reader["IdCartera"]?.ToString()?.Trim();
                    soporte.NombrePaciente = reader["NombrePaciente"]?.ToString()?.Trim();
                    soporte.idTipoId = reader["IdTipoId"]?.ToString()?.Trim();
                    soporte.IdPaciente = reader["IdPaciente"] == DBNull.Value ? string.Empty : Convert.ToString(reader["IdPaciente"])?.Trim() ?? string.Empty;
                    soporte.Celular = reader["Celular"]?.ToString()?.Trim();
                    soporte.Telefono = reader["Telefono"]?.ToString()?.Trim();
                    soporte.Direccion = reader["Direccion"]?.ToString()?.Trim();
                    soporte.Complemento = reader["Complemento"]?.ToString()?.Trim();
                    soporte.Observacion = reader["Observacion"]?.ToString()?.Trim();
                    soporte.ValorCM = "0";
                    soporte.medicamentos = new List<OrdenDto>();
                    
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

                var orden = new OrdenDto();
                orden.Ordenes = reader["Ordenes"]?.ToString()?.Trim();
                orden.Producto = producto;
                orden.Nombre = reader["NombreMedicamento"]?.ToString()?.Trim();
                orden.Cantidad = reader["cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cantidad"]);
                orden.Lote = "";
                orden.ValorMx = 0;
                

                soporte.medicamentos.Add(orden);
            }

            if (soporte != null)
            {
                soporte.ValorCM = totalValorCM.ToString("0.##");
            }

            return soporte;
        }
    }
}

