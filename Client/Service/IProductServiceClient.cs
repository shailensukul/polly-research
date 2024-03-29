﻿using System;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using Client.Model;
using Polly;
using Polly.Extensions.Http;

namespace Client.Service;

public interface IProductServiceClient
{
    public Task<(IEnumerable<ProductContract> Products, HttpStatusCode HttpStatusCode, string ReasonPhrase)> GetProductsAsync(CancellationToken cancellation);

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            // HTTP 5XX status codes (server errors)
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

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
                    logger?.LogWarning("Handling 333 weird error");
                }
                return handled;
            })
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .WrapAsync(GetRetryPolicy());
    }
}