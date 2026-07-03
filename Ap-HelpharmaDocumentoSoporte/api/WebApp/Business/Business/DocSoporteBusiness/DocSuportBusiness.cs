using Data.Interfaces;
using Models.Dto.DocumentoSoporteDto;
using Models.Dto.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.DocSoporteBusiness
{
    public class DocSuportBusiness : IDocSoportBusiness
    {
        private readonly IDocumentoSoporteOfimaData _docSopDataOfima;
        private readonly IDocumentoSoporteDWData _docSopDWData;


        public DocSuportBusiness(IDocumentoSoporteOfimaData doc, IDocumentoSoporteDWData docSopDWData)
        {
            _docSopDataOfima = doc;
            _docSopDWData = docSopDWData;
        }

        public Task<PacienteDto?> GetSoporte(SoporteDto request)
        {
            var soporte = request.Soporte;

            string origen = char.IsDigit(soporte[2]) ? "ofima" : "DW";

            if(origen == "ofima")
            {
                TradeDto trade = new TradeDto
                {
                    Tipodcto = soporte.Substring(0, 2),
                    Nrodcto = soporte.Substring(2)
                };

                return _docSopDataOfima.GetSoporteTradeOfima(trade);
            }
            else
            {
                string soporteDW = soporte.Substring(0, 3);
                int noEntrega = int.Parse(soporte.Substring(3));
                return _docSopDWData.GetSoporteDW(soporteDW,noEntrega);
            }

        }

        public Task<PacienteDto?> GetSoporteTrade(TradeDto trade)
        {
            return _docSopDataOfima.GetSoporteTradeOfima(trade);
        }


        public Task<SoporteEntregaDto?> GetDatosSoportes(SoporteDto request)
        {
            var soporte = request.Soporte;

            string origen = char.IsDigit(soporte[2]) ? "ofima" : "DW";
            try
            {
                if (origen == "ofima")
                {
                    TradeDto trade = new TradeDto
                    {
                        Tipodcto = soporte.Substring(0, 2),
                        Nrodcto = soporte.Substring(2)
                    };

                    return _docSopDataOfima.GetDatosSoportes(trade);
                }
                else
                {
                    TradeDto trade = new TradeDto
                    {
                        Tipodcto = soporte.Substring(0, 3),
                        Nrodcto = soporte.Substring(3)//FMF
                    };
                    return _docSopDWData.GetDatosSoportes(trade);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error de codigo", ex);
            }


        }


    }
}
