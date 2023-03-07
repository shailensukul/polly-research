# Polly Retry Demo


## What is Polly
[Polly](https://github.com/App-vNext/Polly) is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, Rate-limiting and Fallback in a fluent and thread-safe manner.

## Overview

The solution has 2 projects.

One is a Web API service which has 2 endpoints:

- /Product/Get - the "good" endpoint
- /Product/GetWithRandom failure, which succeeds once every 3 tries and fails other times with either a "service unavailable" error or a custom 333 = "weird error" error

![image](https://user-images.githubusercontent.com/564911/223391366-7298346a-6a4f-4492-a450-3bc7eddbde69.png)

The other project is an ASP.Net MVC app which tries to call the "bad" endpoint with and without a retry policy.
The policies are defined in Program.cs against a typed HttpClient:


```
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5017");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCustomPolicy());
```
...
```
static IAsyncPolicy<HttpResponseMessage> GetCustomPolicy()
{
    return Policy<HttpResponseMessage>.Handle<HttpRequestException>()
        .OrResult(msg =>
        {
            var content = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            bool handled = (int)msg.StatusCode == 333 && content.Contains("Weird error");
            if (handled)
            {
                Console.WriteLine("Handling 333 weird error");
            }
            return handled;
        })
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

The reslient button leverages the policies above to automatically retry failures to provide a consitent and stable experience.

![image](https://user-images.githubusercontent.com/564911/223392764-a481c61b-8e03-41a1-a4cd-4cb67403cde2.png)
