using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpPost("CreateSale")]
        public IActionResult Create (CreateSaleRequest request)
        {
            try 
            {
                var saleId = _saleService.CreateSale(request);
                return Ok(new { SaleId = saleId });
            }
            catch (Exception exception)
            {
                return BadRequest(new { MSG = exception.Message });
            }
        }
    }
}
