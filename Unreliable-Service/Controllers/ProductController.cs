using Microsoft.AspNetCore.Mvc;
using Unreliable_Service.Data;
using Unreliable_Service.Models;

namespace Unreliable_Service.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly SampleDbContext context;

    public ProductController(ILogger<ProductController> logger, SampleDbContext context)
    {
        _logger = logger;
        this.context = context;
    }

    [HttpGet(Name = "~/GetProducts")]
    public IEnumerable<ProductContract> Get()
    {
        var products = from p in this.context.Products
            select p;
        return products.ToArray();
    }

    [HttpGet(Name = "~/GetProductsWithRandomFailure")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductContract))]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetWithRandomFailure()
    {
        var random = new Random().Next(1, 4);
        switch (random)
        {
            case 2:
                return StatusCode(333, "Weird error");
            case 3:
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service unavailable");
            default:
                var products = from p in this.context.Products
                    select p;
                return Ok(products.ToArray());
        }
    }
}