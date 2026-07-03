using Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Dto.DocumentoSoporteDto;
using Models.Dto.Request;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocSoporteController : ControllerBase
    {
        private readonly IDocSoportBusiness _docSoport;

        public DocSoporteController(IDocSoportBusiness doc) {
        _docSoport = doc;
        }

     

       [HttpPost("soportes/dctoprv")]
       [ProducesResponseType(StatusCodes.Status200OK)]
       [ProducesResponseType(StatusCodes.Status400BadRequest)]
       public async Task<IActionResult> GetSoportesByDCTOPRV([FromBody] SoporteDto request)
       {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Los datos son obligatorios.");

                var result = await _docSoport.GetSoporte(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
          
       }


        [HttpPost("soportes/Trade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSoportesTrade([FromBody] TradeDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Los datos son obligatorios.");

                var result = await _docSoport.GetSoporteTrade(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        
          
        }

        [HttpPost("soportes/DatosSoportes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDatosSoportesByName([FromBody] SoporteDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Los datos son obligatorios.");
                request.Soporte = request.Soporte.Replace("-", "");
                var result = await _docSoport.GetDatosSoportes(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new
                {
                    mensaje = ex.Message
                });
            }

        }
    }
}
