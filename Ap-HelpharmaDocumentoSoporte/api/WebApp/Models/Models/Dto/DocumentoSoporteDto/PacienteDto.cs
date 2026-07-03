using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dto.DocumentoSoporteDto
{
    public class PacienteDto
    {
        public string NombrePaciente { get; set; }
        public string TipoId { get; set; }
        public string Paciente { get; set; }
        public string DireccionPaciente { get; set; }
        public string TelefonoPaciente { get; set; }
        public string CelularPaciente { get; set; }
        public string Complemento { get; set; }
        public decimal ValorCMTotal { get; set; }

        public FacturaDto Factura { get; set; }

        
    }

}
