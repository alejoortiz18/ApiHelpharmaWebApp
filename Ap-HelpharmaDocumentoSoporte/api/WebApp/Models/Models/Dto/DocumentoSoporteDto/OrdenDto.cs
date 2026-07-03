using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dto.DocumentoSoporteDto
{
    public class OrdenDto
    {
        public string Ordenes { get; set; }
        public string Producto { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public string Lote { get; set; }
        public decimal ValorMx { get; set; }
    }

}
