using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]   
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpPost("CreateSale")]
        public async Task <IActionResult> Create (CreateSaleRequest request)
        {
            try 
            {
                var result = await _saleService.CreateSaleAsync(request);
                return Ok(result);
            }
            catch (Exception exception)
            {
                return BadRequest(new { MSG = exception.Message });
            }
        }

        [HttpGet("GetAllSales")]
        public async Task <IActionResult> GetAllSales([FromServices] ISaleRepository saleRepository)
        {
            if (saleRepository == null)
            {
                return BadRequest(new { MSG = "Sale repository is not available." });
            }

            var sales = await saleRepository.FetchAllSalesAsync();
            return Ok(sales);
        }

        [HttpGet("GetSale/{saleId}")]
        public async Task <IActionResult> GetSaleById(int saleId, [FromServices] ISaleRepository saleRepository)
        {
            var sale = await saleRepository.FetchSaleByIdAsync(saleId);
            if (sale == null)
            {
                return NotFound(new { MSG = $"Sale with ID {saleId} not found." });
            }
            return Ok(sale);
        }
    }
}
