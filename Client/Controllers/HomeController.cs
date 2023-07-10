using System.Diagnostics;
using System.Net;
using Client.Model;
using Microsoft.AspNetCore.Mvc;
using Client.Models;
using Client.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using Polly;
using System.Diagnostics.Tracing;

namespace Client.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductServiceClient _productServiceClient;

    public HomeController(ILogger<HomeController> logger, IProductServiceClient productServiceClient)
    {
        _productServiceClient = productServiceClient;
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
        HttpResponseMessage response = await client.GetAsync("/Product/v2/GetProducts");
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
        ViewBag.Products = products.Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.Description });

        return View("Index");
    }

    public async Task<IActionResult> GetProductsResilient(CancellationToken cancellation)
    {
        var response = await this._productServiceClient.GetProductsAsync(cancellation);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            ViewBag.Error = string.Empty;
        }
        else
        {
            ViewBag.Error = $"StatusCode: {response.HttpStatusCode}\nReason:{response.ReasonPhrase}";
        }

        ViewBag.Products = response.Products.Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.Description });

        return View("Index");
    }

    /// <summary>
    /// Create an asynchronouse request and then cancel it immediately after.
    /// This should work by pressing the button a few times and watching the log for the cancelled message
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<IActionResult> GetProductsResilientAndCancelRequest(CancellationToken cancellation)
    {
        CancellationTokenSource source = new CancellationTokenSource(50);
        CancellationToken token = source.Token;

        var responseAsync = this._productServiceClient.GetProductsAsync(token);
        source.Cancel();

        var response = responseAsync.Result;

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            ViewBag.Error = string.Empty;
        }
        else
        {
            ViewBag.Error = $"StatusCode: {response.HttpStatusCode}\nReason:{response.ReasonPhrase}";
        }


        ViewBag.Products = response.Products.Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.Description });


        return View("Index");

    }

}