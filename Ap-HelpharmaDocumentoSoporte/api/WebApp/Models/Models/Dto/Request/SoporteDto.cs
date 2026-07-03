using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Dto.Request
{
    public class SoporteDto
    {
        [Required(ErrorMessage = "El campo Soporte es obligatorio.")]
        public string Soporte { get; set; }
    }
}
