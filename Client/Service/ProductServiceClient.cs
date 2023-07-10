using System.Net;
using Client.Model;
using Polly;

namespace Client.Service;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public ProductServiceClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        this._logger = loggerFactory.CreateLogger("Custom");
    }
    public async Task<(IEnumerable<ProductContract> Products, HttpStatusCode HttpStatusCode, string ReasonPhrase)> GetProductsAsync(CancellationToken cancellation)
    {
        IEnumerable<ProductContract> products = Array.Empty<ProductContract>();
        HttpResponseMessage response = default;
        var policy = IProductServiceClient.GetCustomPolicy(_logger);

        try
        {
            response = await policy.ExecuteAsync(
                ct => _httpClient.GetAsync("/Product/v2/GetProducts", ct), cancellation);

            if (response.IsSuccessStatusCode)
            {
                products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductContract>>();
            }
        }

        catch (OperationCanceledException oce)
        {
            _logger.LogError($"Request timed out, the cancellation token was triggered. {oce.Message}");
            return (Array.Empty<ProductContract>(), HttpStatusCode.BadRequest, $"Request timed out, the cancellation token was triggered. {oce.Message}");
        }
        var reason = await response.Content.ReadAsStringAsync();
        return (products, response.StatusCode, reason);
    }
}