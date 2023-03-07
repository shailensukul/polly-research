using System.Net;
using Client.Model;

namespace Client.Service;

public interface IProductService
{
    public Task<(IEnumerable<ProductContract> Products, HttpStatusCode HttpStatusCode, string ReasonPhrase)> GetProducts();
}