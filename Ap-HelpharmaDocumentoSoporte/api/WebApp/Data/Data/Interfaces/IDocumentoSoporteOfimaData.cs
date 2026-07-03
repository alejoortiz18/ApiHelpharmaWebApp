using Models.Dto.DocumentoSoporteDto;
using Models.Dto.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Interfaces
{
    public interface IDocumentoSoporteOfimaData
    {
        Task<PacienteDto?> GetSoporteOfima(string DCTOPRV);
        Task<PacienteDto?> GetSoporteTradeOfima(TradeDto trade);
        Task<SoporteEntregaDto?> GetDatosSoportes(TradeDto trade);
    }
}
