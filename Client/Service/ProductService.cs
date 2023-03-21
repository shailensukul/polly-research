using System.Net;
using Client.Model;
using Polly;

namespace Client.Service;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<(IEnumerable<ProductContract> Products, HttpStatusCode HttpStatusCode, string ReasonPhrase)> GetProducts()
    {
        IEnumerable<ProductContract> products = Array.Empty<ProductContract>();
        HttpResponseMessage response = await _httpClient.GetAsync("/Product/v2/GetProducts");
        if (response.IsSuccessStatusCode)
        {
            products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductContract>>();
        }

        var reason = await response.Content.ReadAsStringAsync();
        return (products, response.StatusCode, reason);
    }
}