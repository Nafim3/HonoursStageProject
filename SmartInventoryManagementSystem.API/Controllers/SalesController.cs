using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;

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
                var result = _saleService.CreateSale(request);
                return Ok(result);
            }
            catch (Exception exception)
            {
                return BadRequest(new { MSG = exception.Message });
            }
        }

        [HttpGet("GetAllSales")]
        public IActionResult GetAllSales([FromServices] ISaleRepository saleRepository)
        {
            var sales = saleRepository.FetchAllSales();
            return Ok(sales);
        }

        [HttpGet("GetSale/{saleId}")]
        public IActionResult GetSaleById(int saleId, [FromServices] ISaleRepository saleRepository)
        {
            var sale = saleRepository.FetchSaleById(saleId);
            if (sale == null)
            {
                return NotFound(new { MSG = $"Sale with ID {saleId} not found." });
            }
            return Ok(sale);
        }
    }
}
