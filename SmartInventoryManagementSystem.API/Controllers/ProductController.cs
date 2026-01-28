using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            return Ok(products);
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
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product is null");
            }
           await _productRepository.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProducts), new { id = product.ProductId }, product);
        }

        [HttpPut("UpdateProducts/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product is null or ID mismatch");
            }
            
            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            product.ProductId = id;
           await _productRepository.UpdateProductAsync(product);
            return NoContent();
        }

        [HttpDelete("DeleteProducts/{id}")]
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
