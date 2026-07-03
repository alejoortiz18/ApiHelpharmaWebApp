using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dto.DocumentoSoporteDto
{
    public class FacturaDto
    {
        public FacturaDto()
        {
            Ordenes = new List<OrdenDto>();
        }
        public string Convenio { get; set; }
        public DateTime Fecha { get; set; }
        public string Bodega { get; set; }
        public string TipoEntrega { get; set; }
        public string Cartera { get; set; }
        public string Observacion { get; set; }
        public string Usuario { get; set; }
        public List<OrdenDto> Ordenes { get; set; }
    }

}
