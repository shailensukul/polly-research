using Microsoft.AspNetCore.Mvc;
using Unreliable_Service.Data;
using Unreliable_Service.Models;

namespace Unreliable_Service.Controllers;

[ApiController]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly SampleDbContext context;

    public ProductController(ILogger<ProductController> logger, SampleDbContext context)
    {
        _logger = logger;
        this.context = context;
    }

    [HttpGet, Route("[controller]/v1/GetProducts")]
    [ApiExplorerSettings(GroupName = "v1")]
    public IEnumerable<ProductContract> Get()
    {
        var products = from p in this.context.Products
            select p;
        return products.ToArray();
    }

    [HttpGet, Route("[controller]/v2/GetProducts")]
    [ApiExplorerSettings(GroupName = "v2")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductContract))]
    [ProducesResponseType(typeof(ContentResult), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ContentResult), 333 )]
    public IActionResult GetV2()
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