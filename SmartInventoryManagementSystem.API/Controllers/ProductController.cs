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
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProduct();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            return Ok(product);
        }

        [HttpPost("Addproducts")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product is null");
            }
            _productRepository.AddProduct(product);
            return CreatedAtAction(nameof(GetProducts), new { id = product.ProductId }, product);
        }

        [HttpPut("UpdateProducts/{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product is null or ID mismatch");
            }
            
            var existingProduct = _productRepository.GetProductById(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            product.ProductId = id;
            _productRepository.UpdateProduct(product);
            return NoContent();
        }

        [HttpDelete("DeleteProducts/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var existingProduct = _productRepository.GetProductById(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            _productRepository.DeleteProduct(id);
            return NoContent();
        }

        [HttpGet("Low-stock/check")]
        public IActionResult CheckLowStock()
        {
            _productService.CheckLowStockAndNotify();
            return Ok("Low stock check completed and notifications sent if necessary.");
        }
    }
}
