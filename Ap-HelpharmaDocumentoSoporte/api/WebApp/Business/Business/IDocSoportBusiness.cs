using Models.Dto.DocumentoSoporteDto;
using Models.Dto.Request;

namespace Business
{
    public interface IDocSoportBusiness
    {
        Task<PacienteDto?> GetSoporte(SoporteDto request);
        Task<PacienteDto?> GetSoporteTrade(TradeDto trade);
        Task<SoporteEntregaDto?> GetDatosSoportes(SoporteDto request);
    }
}
