using System.Diagnostics;
using System.Net;
using Client.Model;
using Microsoft.AspNetCore.Mvc;
using Client.Models;
using Client.Service;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Client.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;

    public HomeController(ILogger<HomeController> logger, IProductService productService)
    {
        _productService = productService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.Products = Array.Empty<SelectListItem>();
        ViewBag.Error = string.Empty;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> GetProducts()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5017/");
        IEnumerable<ProductContract> products = Array.Empty<ProductContract>();
        HttpResponseMessage response = await client.GetAsync("/Product/GetWithRandomFailure");
        if (response.IsSuccessStatusCode)
        {
            ViewBag.Error = string.Empty;
            products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductContract>>();
        }
        else
        {
            var reason = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"StatusCode: {response.StatusCode}\nReason:{reason}";
        }
        ViewBag.Products = products.Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.Description});

        return View("Index");
    }

    public async Task<IActionResult> GetProductsResilient()
    {
        var response = await _productService.GetProducts();
        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            ViewBag.Error = string.Empty;
        }
        else
        {
            ViewBag.Error = $"StatusCode: {response.HttpStatusCode}\nReason:{response.ReasonPhrase}";
        }

        ViewBag.Products = response.Products.Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.Description});

        return View("Index");
    }
}