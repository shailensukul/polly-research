using System.Net;
using Client.Model;
using Polly;

namespace Client.Service;

public interface IProductService
{
    public Task<(IEnumerable<ProductContract> Products, HttpStatusCode HttpStatusCode, string ReasonPhrase)> GetProducts();

    // This custom policy handles a 333 error code
    public static IAsyncPolicy<HttpResponseMessage> GetCustomPolicy(ILogger logger)
    {
        return Policy<HttpResponseMessage>.Handle<HttpRequestException>()
            .OrResult(msg =>
            {
                var content = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                bool handled = (int)msg.StatusCode == 333 && content.Contains("Weird error");
                if (handled)
                {
                    logger.LogWarning("Handling 333 weird error");
                }
                return handled;
            })
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}