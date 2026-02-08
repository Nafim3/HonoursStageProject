using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Repositories;
using System.Security.Claims;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]   
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;
            private readonly ISaleRepository _saleRepository;
            private readonly IPdfService _pdfService;

        public SalesController(ISaleService saleService, ISaleRepository saleRepository, IPdfService pdfService)
        {
            _saleService = saleService;
            _saleRepository = saleRepository;
            _pdfService = pdfService;
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

        [HttpGet("{saleId}/invoice")]
        public async Task <IActionResult> GetInvoice (int saleId)
        {
            var sale = await _saleRepository.FetchSaleByIdAsync(saleId);

            if (sale == null)
                return NotFound();

            var pdf = _pdfService.GenerateInvoicePdf(sale);

            return File(pdf, "application/pdf", $"invoice-{saleId}.pdf");
        }
    }
}
