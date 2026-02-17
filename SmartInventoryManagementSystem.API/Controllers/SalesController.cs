using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
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
                return BadRequest(new { MSG = "Sale repository is not available." });

            // 1️⃣ Fetch entities
            var salesEntities = await saleRepository.FetchAllSalesAsync();

            // 2️⃣ Map to DTOs
            var salesDto = salesEntities.Select(s => new GetAllSales
            {
                SaleId = s.SaleId,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount
            }).ToList();

            return Ok(salesDto);
        }

        [HttpGet("GetSale/{saleId}")]
        public async Task <IActionResult> GetSaleById(int saleId, [FromServices] ISaleRepository saleRepository)
        {
            var saleEntity = await saleRepository.FetchSaleByIdAsync(saleId);

            if (saleEntity == null)
                return NotFound(new { MSG = $"Sale with ID {saleId} not found." });

            // Map to DTO
            var saleDto = new GetByID
            {
                SaleId = saleEntity.SaleId,
                SaleDate = saleEntity.SaleDate,
                TotalAmount = saleEntity.TotalAmount,
                BuyerName = saleEntity.BuyerName,
                Items = saleEntity.SaleItems.Select(item => new SaleDetails
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.ProductName ?? "",
                    QuantitySold = item.Quantity,
                    ProductPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                }).ToList()
            };

            return Ok(saleDto);
        }

        [HttpGet("{saleId}/invoice")]
        public async Task <IActionResult> GetInvoice (int saleId)
        {
            var saleEntity = await _saleRepository.FetchSaleByIdAsync(saleId);
            if (saleEntity == null)
                return NotFound();

            // 2️⃣ Generate PDF from entity
            var pdfBytes = await _pdfService.GenerateInvoicePdf(saleEntity);

            // 3️⃣ Return PDF file
            return File(pdfBytes, "application/pdf", $"Invoice_{saleId}.pdf");
        }
    }
}
