using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        
        private readonly IProductRepository _productRepository; // used to access product data
        private readonly IProductService _productService; // used for product-related business logic

        public ProductController(IProductRepository productRepository, IProductService productService)
        {
            _productRepository = productRepository;
            _productService = productService;
        }

        [HttpGet]
        public async Task <IActionResult> GetProducts()
        {
            var products = await _productRepository.GetProductAsync();

            var dto = products.Select(p => new ProductListDto
            {
                ProductId = p.ProductId,
                Name = p.ProductName,
                Quantity = p.QuantityInStock,
                ReorderLevel = p.ReorderLevel,
                Price = p.ProductPrice
            });

            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            return Ok(product);
        }

        [HttpPost("Addproducts")]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductRequest dto)
        {
            var product = new Product
            {
                ProductName = dto.Name,
                QuantityInStock = dto.Quantity,
                ReorderLevel = dto.ReorderLevel,
                ProductPrice = dto.Price,
                ExpiryDate = dto.ExpiryDate
            };

            await _productRepository.AddProductAsync(product);

            return Ok();
        }

        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest dto)
        {
            if (dto == null)
            {
                return BadRequest("Request body is null");
            }
            
            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // ---- server-side expiry check ----
            if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value.Date < DateTime.Today)
                return BadRequest("Expiry date cannot be in the past.");

            existingProduct.ProductName = dto.Name;
            existingProduct.QuantityInStock = dto.Quantity;
            existingProduct.ReorderLevel = dto.ReorderLevel;
            existingProduct.ProductPrice = dto.Price;
            existingProduct.ExpiryDate = dto.ExpiryDate;


            await _productRepository.UpdateProductAsync(existingProduct);
            return NoContent();
        }

        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            await _productRepository.DeleteProductAsync(id);
            return NoContent();
        }

        [HttpGet("Low-stock/check")]
        public async Task <IActionResult> CheckLowStock()
        {
           await _productService.CheckLowStockAndNotifyAsync();
            return Ok("Low stock check completed and notifications sent if necessary.");
        }

        [HttpGet("Expired-products/check")]
        public async Task <IActionResult> CheckExpiredProducts()
        {
           await _productService.CheckExpiredProductsAndNotifyAsync();
            return Ok("Expired products check completed and notifications sent if necessary.");
        }

    }
}
