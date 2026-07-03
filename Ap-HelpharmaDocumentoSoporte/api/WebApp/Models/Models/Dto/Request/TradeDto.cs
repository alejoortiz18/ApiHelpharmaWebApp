using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Dto.Request
{
    public class TradeDto
    {
        [Required]
        public string Tipodcto { get; set; }
        [Required]
        public string Nrodcto { get; set; }
    }
}
