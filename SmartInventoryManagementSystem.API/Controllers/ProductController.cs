using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Application.DTO.ProductDTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Services;
using System.Security.Claims;

namespace SmartInventoryManagementSystem.API.Controllers
{
    [Authorize]    
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        
        private readonly IProductRepository _productRepository; // used to access product data
        private readonly ICurrentUserService _currentUser;
            private readonly INotificationService _notificationService; 
        public ProductController(IProductRepository productRepository, ICurrentUserService currentUser, INotificationService notificationService)
        {
            _productRepository = productRepository;
            
            _currentUser = currentUser;
            _notificationService = notificationService;
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
            if (product == null || product.UserId != _currentUser.UserId)
                return NotFound($"Product with ID {id} not found.");
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
                ExpiryDate = dto.ExpiryDate,
                UserId = _currentUser.UserId
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
            if (existingProduct == null || existingProduct.UserId != _currentUser.UserId)
                return NotFound($"Product with ID {id} not found.");

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
            if (existingProduct == null || existingProduct.UserId != _currentUser.UserId)
                return NotFound($"Product with ID {id} not found.");
            await _productRepository.DeleteProductAsync(id);
            return NoContent();
        }



        [HttpGet("Expired-products/check")]
        public async Task <IActionResult> CheckExpiredProducts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Fetch expired products for this user
            var expiredProducts = await _productRepository.GetExpiredProductsAsync(userId);

            // Create alerts for each expired product
            foreach (var product in expiredProducts)
            {
                await _notificationService.NotifyExpiredProductsAsync(product, userId);
            }
            return Ok();
        }

    }


}
